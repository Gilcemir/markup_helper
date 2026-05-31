#!/usr/bin/env python3
"""Deterministic preprocessor for the credit-fill skill.

Usage:
  extract.py audit --xml-dir <DIR> --docx-dir <DIR> [--only <ID>]
    Pairs articles by elocation-id, extracts the CREDIT STATEMENT section from
    each DOCX and the <contrib-group> from each XML, attempts a structured
    parse, and classifies each article as:
      - no-op-no-section        : DOCX has no recognizable CREDIT STATEMENT
      - no-op-match             : structured parse + every author resolved +
                                  every term in CRediT + xml-set == docx-set
      - needs-judgment          : anything else (prose, custom terms, mismatch)
      - error                   : malformed inputs

  Outputs JSON to stdout. The skill agent consumes it and reasons about the
  needs-judgment cases.

stdlib only (zipfile, re, json, argparse, pathlib).
"""

from __future__ import annotations

import argparse
import html
import json
import re
import sys
import unicodedata
import zipfile
from pathlib import Path

# ---------------------------------------------------------------------------
# CRediT canonical table (slug = URL fragment, label = display text)
# Source of truth: docs/scielo_context/jats/credit_roles.md
# ---------------------------------------------------------------------------

CREDIT_URL_BASE = "http://credit.niso.org/contributor-roles/"

CREDIT_SLUGS = {
    "conceptualization": "Conceptualization",
    "data-curation": "Data curation",
    "formal-analysis": "Formal analysis",
    "funding-acquisition": "Funding acquisition",
    "investigation": "Investigation",
    "methodology": "Methodology",
    "project-administration": "Project administration",
    "resources": "Resources",
    "software": "Software",
    "supervision": "Supervision",
    "validation": "Validation",
    "visualization": "Visualization",
    "writing-original-draft": "Writing – original draft",
    "writing-review-editing": "Writing – review & editing",
}


def normalize_term(text: str) -> str:
    """Aggressive normalization for matching CRediT terms across spellings."""
    if text is None:
        return ""
    s = unicodedata.normalize("NFKD", text)
    s = "".join(c for c in s if not unicodedata.combining(c))
    s = s.lower().strip()
    s = s.replace("—", "-").replace("–", "-").replace("&", "and")
    s = re.sub(r"[\.,;:]+$", "", s)
    s = re.sub(r"\s+", " ", s).strip(" -")
    return s


# Build normalized lookup: every plausible spelling → slug
NORM_LOOKUP: dict[str, str] = {}
for slug, label in CREDIT_SLUGS.items():
    NORM_LOOKUP[normalize_term(label)] = slug
    NORM_LOOKUP[normalize_term(slug.replace("-", " "))] = slug

# Manual aliases observed in the corpus
ALIASES = {
    "writing - original draft": "writing-original-draft",
    "writing original draft": "writing-original-draft",
    "writing - review and editing": "writing-review-editing",
    "writing review and editing": "writing-review-editing",
    "writing review editing": "writing-review-editing",
    "writing and review": "writing-review-editing",
    "writing - review editing": "writing-review-editing",
}
for k, v in ALIASES.items():
    NORM_LOOKUP[normalize_term(k)] = v


def map_term(raw: str) -> str | None:
    """Return canonical CRediT slug if raw matches, else None."""
    return NORM_LOOKUP.get(normalize_term(raw))


# ---------------------------------------------------------------------------
# DOCX text + section extraction
# ---------------------------------------------------------------------------

SECTION_HEADINGS = [
    r"CREDIT\s+STATEMENT",
    r"CRediT\s+(?:authorship|statement)",
    r"Authors?\s*'?\s*Contributions?(?:\s+Statement)?",
    r"Author\s+Contribution\s+Statement",
    r"Contribui[cç][aã]o\s+dos?\s+Autores?",
    r"Contribui[cç][oõ]es\s+dos?\s+Autores?",
]

END_MARKERS = [
    r"DATA\s+AVAILABILITY",
    r"DATA\s+ACCESSIBILITY",
    r"REFERENCES",
    r"REFER[EÊ]NCIAS",
    r"\[refs\]",
    r"\[/doc\]",
    r"CONFLICTS?\s+OF\s+INTEREST",
    r"DECLARATION\s+OF\s+INTEREST",
]


def extract_docx_text(docx_path: Path) -> str:
    with zipfile.ZipFile(docx_path) as z:
        raw = z.read("word/document.xml").decode("utf-8", errors="replace")
    text = re.sub(r"<[^>]+>", " ", raw)
    text = html.unescape(text)
    text = re.sub(r"\s+", " ", text).strip()
    return text


def extract_credit_section(text: str) -> dict | None:
    for pattern in SECTION_HEADINGS:
        m = re.search(pattern, text, re.IGNORECASE)
        if not m:
            continue
        start = m.end()
        end_candidates = []
        for em in END_MARKERS:
            mm = re.search(em, text[start:], re.IGNORECASE)
            if mm:
                end_candidates.append(start + mm.start())
        end = min(end_candidates) if end_candidates else len(text)
        section = text[start:end].strip(" :.-–—")
        if not section:
            return None
        return {
            "heading_matched": m.group(0),
            "section": section,
            "char_start": m.start(),
            "char_end": end,
        }
    return None


# ---------------------------------------------------------------------------
# XML <contrib-group> parsing
# ---------------------------------------------------------------------------


def parse_contrib_group(xml_text: str) -> dict | None:
    m = re.search(r"<contrib-group>(.*?)</contrib-group>", xml_text, re.S)
    if not m:
        return None
    block = m.group(0)
    inner = m.group(1)
    authors = []
    for cm in re.finditer(r"<contrib\b[^>]*>(.*?)</contrib>", inner, re.S):
        body = cm.group(1)
        surname = _opt(re.search(r"<surname>(.*?)</surname>", body, re.S))
        given = _opt(re.search(r"<given-names>(.*?)</given-names>", body, re.S))
        suffix = _opt(re.search(r"<suffix>(.*?)</suffix>", body, re.S))
        orcid = _opt(
            re.search(
                r"<contrib-id\s+contrib-id-type=\"orcid\">(.*?)</contrib-id>",
                body,
                re.S,
            )
        )
        roles = []
        for rm in re.finditer(
            r"<role(?:\s+content-type=\"([^\"]*)\")?[^>]*>(.*?)</role>",
            body,
            re.S,
        ):
            url = rm.group(1)
            label = (rm.group(2) or "").strip()
            slug = None
            if url:
                m2 = re.search(r"contributor-roles/([a-z\-]+)/?", url)
                if m2:
                    slug = m2.group(1)
            if not slug:
                slug = map_term(label)
            roles.append({"content_type": url, "label": label, "slug": slug})
        authors.append(
            {
                "surname": surname,
                "given_names": given,
                "suffix": suffix,
                "orcid": orcid,
                "current_roles": roles,
            }
        )
    return {"block": block, "authors": authors}


def _opt(match) -> str | None:
    if not match:
        return None
    return re.sub(r"\s+", " ", match.group(1)).strip() or None


# ---------------------------------------------------------------------------
# Structured statement parser (role-keyed and author-keyed)
# ---------------------------------------------------------------------------


def parse_structured(section_text: str) -> dict:
    """Best-effort structured parse. Returns shape + assignments or empty dict.

    role-keyed:    "<role-or-composite>: <people-list>; <role>: <people>; ..."
    author-keyed:  "<author-or-authors>: <role-list>; ..."  with continuations
                   ("ATAJ: ...; DRSJ; TOS; FNV: <shared roles>")

    Splits on ';' OR '.' followed by space + uppercase token, so that
    "Funding acquisition. DRSJ" becomes two segments.
    """
    segments = [
        s.strip().rstrip(".;,")
        for s in re.split(r"[;.]\s+(?=[A-Z])", section_text)
        if s.strip()
    ]

    role_keyed_entries = []
    author_keyed_entries = []
    continuation_buffer = []
    confidence = {"role_keyed": 0, "author_keyed": 0}
    prose_signal = False

    for seg in segments:
        if ":" not in seg:
            # Looks like a continuation only if it's plausibly an author token:
            # short (<35 chars), no obvious prose verbs.
            if _is_plausible_author_token(seg):
                continuation_buffer.append(seg)
            else:
                prose_signal = True
            continue
        key, val = seg.split(":", 1)
        key = key.strip()
        val = val.strip().rstrip(".;,")

        # Roles vs people use different splitting strategies.
        # Heuristic decision: try key as roles first (smart split). If the
        # smart-split of the key yields CRediT matches, treat as role-keyed.
        key_as_roles = _split_roles(key)
        val_as_roles = _split_roles(val)
        key_as_people = _split_composite(key)
        val_as_people = _split_composite(val)

        n_key_credit = sum(1 for p in key_as_roles if map_term(p))
        n_val_credit = sum(1 for p in val_as_roles if map_term(p))

        if n_key_credit > 0 and n_key_credit >= n_val_credit:
            # role-keyed segment
            people = val_as_people
            roles = [(p, map_term(p)) for p in key_as_roles]
            # continuations aren't valid for role-keyed (they'd be more people without
            # a role key) — treat as orphan
            for orphan in continuation_buffer:
                role_keyed_entries.append(
                    {
                        "raw_role": "<orphan>",
                        "role_slug": None,
                        "raw_people": orphan,
                        "people_tokens": _split_composite(orphan),
                    }
                )
            continuation_buffer = []
            for raw_role, slug in roles:
                role_keyed_entries.append(
                    {
                        "raw_role": raw_role,
                        "role_slug": slug,
                        "raw_people": val,
                        "people_tokens": people,
                    }
                )
            confidence["role_keyed"] += 1
        else:
            # author-keyed segment; key (possibly composite "X and Y") +
            # continuation_buffer share the value's roles
            authors_for_segment = continuation_buffer + key_as_people
            continuation_buffer = []
            role_tokens = val_as_roles
            role_slugs = [map_term(p) for p in role_tokens]
            for author_token in authors_for_segment:
                author_keyed_entries.append(
                    {
                        "raw_author": author_token,
                        "role_tokens": role_tokens,
                        "role_slugs": role_slugs,
                        "raw_roles": val,
                    }
                )
            confidence["author_keyed"] += 1

    # leftover continuation buffer = orphan tokens we couldn't attribute
    orphans = continuation_buffer

    shape = None
    if confidence["role_keyed"] > 0 and confidence["author_keyed"] == 0:
        shape = "role-keyed"
    elif confidence["author_keyed"] > 0 and confidence["role_keyed"] == 0:
        shape = "author-keyed"
    elif confidence["role_keyed"] > 0 and confidence["author_keyed"] > 0:
        shape = "mixed"

    if prose_signal and (role_keyed_entries or author_keyed_entries):
        shape = "mixed"
    elif prose_signal and not (role_keyed_entries or author_keyed_entries):
        shape = None

    return {
        "shape": shape,
        "role_keyed": role_keyed_entries,
        "author_keyed": author_keyed_entries,
        "orphans": orphans,
        "prose_signal": prose_signal,
        "confidence": confidence,
    }


def _split_composite(text: str) -> list[str]:
    """Aggressive split on commas, ' and ', ' & '. For author/people lists."""
    parts = re.split(r",|\s+and\s+|\s+&\s+", text)
    return [p.strip().rstrip(".;,") for p in parts if p.strip(" .;,")]


def _split_roles(text: str) -> list[str]:
    """Split a role list, but try whole-token CRediT match before splitting on
    ' and ' / ' & '. Handles "Writing - Review & Editing" as one term."""
    out = []
    for part in re.split(r",\s+", text):
        part = part.strip().rstrip(".;,")
        if not part:
            continue
        if map_term(part):
            out.append(part)
            continue
        # Try splitting on " and " / " & "
        subs = [s.strip().rstrip(".;,") for s in re.split(r"\s+and\s+|\s+&\s+", part) if s.strip(" .;,")]
        # If at least one sub maps to CRediT, accept split; else keep whole as custom.
        if any(map_term(s) for s in subs):
            out.extend(subs)
        else:
            out.append(part)
    return out


_PROSE_VERBS = re.compile(
    r"\b(contributed|conceived|wrote|analyzed|designed|performed|prepared|"
    r"collected|developed|carried|participated|drafted|reviewed|revised|"
    r"managed|supervised|provided|interpreted|implemented|investigated)\b",
    re.IGNORECASE,
)


def _is_plausible_author_token(token: str) -> bool:
    if len(token) > 35:
        return False
    if _PROSE_VERBS.search(token):
        return False
    # too many lowercase words = probably prose, not a name
    words = token.split()
    if len(words) > 5:
        return False
    return True


# ---------------------------------------------------------------------------
# Author resolution (initials / names → contrib index)
# ---------------------------------------------------------------------------


def _strip_accents(s: str) -> str:
    if not s:
        return ""
    return "".join(
        c for c in unicodedata.normalize("NFKD", s) if not unicodedata.combining(c)
    )


def resolve_author(token: str, authors: list) -> list[int]:
    """Return list of candidate indices. Empty = unresolved. Multiple = ambiguous."""
    token = token.strip().rstrip(".,;").strip()
    if not token:
        return []

    norm_token = _strip_accents(token).lower()

    # Pure initials (2-7 uppercase letters)
    if re.fullmatch(r"[A-Z]{2,7}", token):
        return _resolve_initials(token, authors)

    # "Initials Surname" / "Surname Initials" / "Surname, Initials"
    cleaned = re.sub(r"[,.]", " ", token)
    cleaned = re.sub(r"\s+", " ", cleaned).strip()
    parts = cleaned.split(" ")
    if len(parts) >= 2:
        candidates = []
        for idx, a in enumerate(authors):
            for surname_field in (a.get("surname"), a.get("suffix")):
                if not surname_field:
                    continue
                sur = _strip_accents(surname_field).lower()
                for p in parts:
                    if p and sur == _strip_accents(p).lower():
                        candidates.append(idx)
                        break
        # dedup preserving order
        return list(dict.fromkeys(candidates))

    # Single token: maybe a surname
    if len(parts) == 1:
        candidates = []
        for idx, a in enumerate(authors):
            for surname_field in (a.get("surname"), a.get("suffix")):
                if not surname_field:
                    continue
                if _strip_accents(surname_field).lower() == norm_token:
                    candidates.append(idx)
                    break
        return list(dict.fromkeys(candidates))

    return []


PORTUGUESE_PARTICLES = {"de", "do", "da", "dos", "das", "del", "von", "der", "van", "le", "la", "e", "y"}


def _resolve_initials(token: str, authors: list) -> list[int]:
    candidates = []
    for idx, a in enumerate(authors):
        sur = a.get("surname") or ""
        given = a.get("given_names") or ""
        suffix = a.get("suffix") or ""
        if not sur or not given:
            continue
        given_words = [
            w for w in re.findall(r"\S+", given)
            if w.lower() not in PORTUGUESE_PARTICLES
        ]
        given_inits = "".join(w[0].upper() for w in given_words if w)
        sur_init = sur[0].upper() if sur else ""
        suf_init = suffix[0].upper() if suffix else ""
        variants = {
            given_inits + sur_init,
            sur_init + given_inits,
            given_inits + sur_init + suf_init,
            sur_init + given_inits + suf_init,
            given_inits + suf_init + sur_init,
        }
        if token in variants:
            candidates.append(idx)
    return candidates


# ---------------------------------------------------------------------------
# Pairing + audit
# ---------------------------------------------------------------------------


def pair_articles(xml_dir: Path, docx_dir: Path, only: str | None) -> list[dict]:
    pairs = []
    for xml_path in sorted(xml_dir.glob("*.xml")):
        m_eloc = re.search(r"(e\d+)\.xml$", xml_path.name)
        if not m_eloc:
            continue
        elocation = m_eloc.group(1)
        m_prefix = re.match(r"e(\d{4})", elocation)
        if not m_prefix:
            continue
        prefix = m_prefix.group(1)
        if only and only not in (elocation, prefix):
            continue
        docx_path = docx_dir / f"{prefix}.docx"
        if not docx_path.exists():
            pairs.append(
                {
                    "elocation_id": elocation,
                    "xml_path": str(xml_path),
                    "docx_path": None,
                    "error": f"docx-missing (looked for {docx_path.name})",
                }
            )
            continue
        pairs.append(
            {
                "elocation_id": elocation,
                "xml_path": str(xml_path),
                "docx_path": str(docx_path),
            }
        )
    return pairs


def audit_article(pair: dict) -> dict:
    if pair.get("error"):
        return {**pair, "classification": "error", "reason": pair["error"]}

    xml_text = Path(pair["xml_path"]).read_text(encoding="utf-8")
    docx_text = extract_docx_text(Path(pair["docx_path"]))
    cg = parse_contrib_group(xml_text)
    section = extract_credit_section(docx_text)

    result = {**pair, "contrib_group": cg, "section": section}

    if not cg:
        result["classification"] = "error"
        result["reason"] = "no <contrib-group> in XML"
        return result

    if not section or not section.get("section"):
        any_existing = any(a["current_roles"] for a in cg["authors"])
        result["classification"] = "no-op-no-section"
        result["any_existing_roles"] = any_existing
        return result

    structured = parse_structured(section["section"])
    result["structured"] = structured

    # Build proposed docx-assignment per author
    docx_assignment: dict[int, set[str]] = {}
    all_resolved = True
    all_terms_credit = True
    issues: list[str] = []

    if structured["shape"] in ("author-keyed",) and structured["author_keyed"]:
        for entry in structured["author_keyed"]:
            candidates = resolve_author(entry["raw_author"], cg["authors"])
            if len(candidates) != 1:
                all_resolved = False
                issues.append(
                    f"author '{entry['raw_author']}' resolves to "
                    f"{len(candidates)} authors"
                )
                continue
            idx = candidates[0]
            for token, slug in zip(entry["role_tokens"], entry["role_slugs"]):
                if not slug:
                    all_terms_credit = False
                    issues.append(
                        f"term '{token}' is not in CRediT (author "
                        f"{entry['raw_author']})"
                    )
                else:
                    docx_assignment.setdefault(idx, set()).add(slug)

    elif structured["shape"] in ("role-keyed",) and structured["role_keyed"]:
        for entry in structured["role_keyed"]:
            if not entry["role_slug"]:
                all_terms_credit = False
                issues.append(f"role '{entry['raw_role']}' is not in CRediT")
                continue
            for person in entry["people_tokens"]:
                candidates = resolve_author(person, cg["authors"])
                if len(candidates) != 1:
                    all_resolved = False
                    issues.append(
                        f"person '{person}' resolves to {len(candidates)} authors "
                        f"(role: {entry['raw_role']})"
                    )
                    continue
                idx = candidates[0]
                docx_assignment.setdefault(idx, set()).add(entry["role_slug"])
    else:
        result["classification"] = "needs-judgment"
        result["reason"] = (
            "free prose or unknown shape; LLM judgment required"
            if structured["shape"] is None
            else f"mixed shapes ({structured['shape']}); LLM judgment required"
        )
        return result

    result["docx_assignment"] = {
        str(k): sorted(v) for k, v in docx_assignment.items()
    }
    result["structured_issues"] = issues

    if not all_resolved or not all_terms_credit:
        result["classification"] = "needs-judgment"
        result["reason"] = "; ".join(issues[:5]) + (
            f" ... (+{len(issues) - 5} more)" if len(issues) > 5 else ""
        )
        return result

    # Build XML-set per author (canonicalize via slug)
    xml_set: dict[int, set[str]] = {}
    free_text_in_xml = False
    for idx, a in enumerate(cg["authors"]):
        slugs = set()
        for r in a["current_roles"]:
            if r["slug"]:
                slugs.add(r["slug"])
            else:
                free_text_in_xml = True
        xml_set[idx] = slugs
    result["xml_set"] = {str(k): sorted(v) for k, v in xml_set.items()}

    # Compare
    extras: dict[str, list[str]] = {}
    missing: dict[str, list[str]] = {}
    for idx in range(len(cg["authors"])):
        d = docx_assignment.get(idx, set())
        x = xml_set.get(idx, set())
        if d != x:
            if x - d:
                extras[str(idx)] = sorted(x - d)
            if d - x:
                missing[str(idx)] = sorted(d - x)

    if extras or missing or free_text_in_xml:
        result["classification"] = "needs-judgment"
        reasons = []
        if missing:
            reasons.append(f"{len(missing)} authors missing roles")
        if extras:
            reasons.append(f"{len(extras)} authors have extra roles")
        if free_text_in_xml:
            reasons.append("XML has free-text role (no @content-type)")
        result["reason"] = "; ".join(reasons)
        result["extras"] = extras
        result["missing"] = missing
        return result

    result["classification"] = "no-op-match"
    return result


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------


def cmd_audit(args) -> int:
    xml_dir = Path(args.xml_dir).resolve()
    docx_dir = Path(args.docx_dir).resolve()
    if not xml_dir.is_dir():
        print(f"ERROR: xml-dir not a directory: {xml_dir}", file=sys.stderr)
        return 2
    if not docx_dir.is_dir():
        print(f"ERROR: docx-dir not a directory: {docx_dir}", file=sys.stderr)
        return 2

    pairs = pair_articles(xml_dir, docx_dir, args.only)
    results = [audit_article(p) for p in pairs]
    payload = {
        "xml_dir": str(xml_dir),
        "docx_dir": str(docx_dir),
        "credit_url_base": CREDIT_URL_BASE,
        "credit_slugs": CREDIT_SLUGS,
        "articles": results,
    }
    json.dump(payload, sys.stdout, ensure_ascii=False, indent=2)
    sys.stdout.write("\n")
    return 0


def main() -> int:
    p = argparse.ArgumentParser(prog="extract.py")
    sub = p.add_subparsers(dest="cmd", required=True)

    a = sub.add_parser("audit", help="classify all paired articles")
    a.add_argument("--xml-dir", required=True)
    a.add_argument("--docx-dir", required=True)
    a.add_argument("--only", default=None, help="elocation-id (e.g., e51362627) or 4-digit DOCX prefix")
    a.set_defaults(func=cmd_audit)

    args = p.parse_args()
    return args.func(args)


if __name__ == "__main__":
    sys.exit(main())
