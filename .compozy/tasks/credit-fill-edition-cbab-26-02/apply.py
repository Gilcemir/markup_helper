#!/usr/bin/env python3
"""Reconstruct <contrib-group> block with new <role> elements per author.

Usage (writes new block to stdout; does NOT touch XML — caller uses Edit):

    python3 apply.py <elocation_id> --assignments-file <file>

Assignments file format (JSON):
    {
      "mode": "credit" | "free-text",
      "assignments": [
        {"orcid": "0000-...", "roles": [
            {"slug": "conceptualization"},
            {"slug": "methodology"},
            {"label": "Some custom term", "free": true}    # only used in free-text mode
        ]},
        ...
      ]
    }

In credit mode each role entry must have a slug (resolved via credit_slugs from audit JSON).
In free-text mode each role entry has either slug (→ canonical label) or label (→ literal text);
no @content-type is written.
"""
from __future__ import annotations
import json
import re
import sys
from pathlib import Path

AUDIT = Path(__file__).resolve().parent / "audit.json"

CONTRIB_RE = re.compile(r"(<contrib\b[^>]*>)(.*?)(</contrib>)", re.DOTALL)
ORCID_RE = re.compile(r"<contrib-id\s+contrib-id-type=\"orcid\">([^<]+)</contrib-id>", re.IGNORECASE)


def build_role(mode: str, slug: str | None, label: str | None, credit_slugs: dict, credit_url_base: str) -> str:
    """Return a single <role>...</role> string with 5 tabs of indent."""
    indent = "\t" * 5
    if mode == "credit":
        assert slug is not None, "credit mode requires slug"
        text = credit_slugs[slug]
        return f'{indent}<role content-type="{credit_url_base}{slug}/">{text}</role>'
    # free-text mode
    if slug is not None:
        text = credit_slugs[slug]
    else:
        assert label is not None, "free-text role needs slug or label"
        text = label
    return f"{indent}<role>{text}</role>"


def rewrite_contrib(contrib_xml: str, roles_for_orcid: dict, mode: str, credit_slugs: dict, credit_url_base: str) -> str:
    """Rewrite one <contrib>...</contrib> block.

    Steps:
      1. Extract orcid (if any).
      2. Strip ALL existing <role>...</role> lines (and their trailing newline+indent).
      3. After last <xref>...</xref> (or after last non-role child), insert new <role> tags
         for this orcid.
    """
    open_tag_m = re.match(r"(<contrib\b[^>]*>)(.*)(</contrib>)$", contrib_xml.strip(), re.DOTALL)
    if not open_tag_m:
        raise ValueError(f"Cannot parse contrib block: {contrib_xml[:120]!r}")
    open_tag, inner, close_tag = open_tag_m.group(1), open_tag_m.group(2), open_tag_m.group(3)

    orcid_m = ORCID_RE.search(inner)
    orcid = orcid_m.group(1).strip() if orcid_m else None

    # Strip existing <role>...</role> entries (each followed by newline+tabs of indent).
    inner_no_roles = re.sub(r"\n\t*<role\b[^>]*>.*?</role>", "", inner, flags=re.DOTALL)

    # Build new roles
    role_list = roles_for_orcid.get(orcid, [])
    role_lines = []
    for r in role_list:
        role_lines.append(build_role(mode, r.get("slug"), r.get("label"), credit_slugs, credit_url_base))

    # Inject before close_tag; preserve trailing indentation pattern.
    # Pattern: inner_no_roles ends like "...<xref>...</xref>\n\t\t\t\t" then </contrib> closes
    # The pre-close indent before </contrib> is typically 4 tabs.
    # We need: ...last_child + "\n" + "\t\t\t\t\t<role.../>" * N + "\n\t\t\t\t" + </contrib>
    # Easier: find the trailing whitespace before close_tag in inner_no_roles, peel it, append roles + that whitespace.
    m = re.search(r"\n(\t*)$", inner_no_roles)
    if m:
        trailing_indent = m.group(1)
        body = inner_no_roles[: m.start()]
    else:
        trailing_indent = "\t" * 4
        body = inner_no_roles.rstrip()

    new_inner = body
    for line in role_lines:
        new_inner += "\n" + line
    new_inner += "\n" + trailing_indent

    return open_tag + new_inner + close_tag


def main():
    if len(sys.argv) < 4 or sys.argv[2] != "--assignments-file":
        print("usage: apply.py <elocation_id> --assignments-file <path> [--write]", file=sys.stderr)
        sys.exit(2)
    elocation = sys.argv[1]
    assignments_path = Path(sys.argv[3])
    write_mode = "--write" in sys.argv[4:]

    audit = json.load(open(AUDIT))
    credit_slugs = audit["credit_slugs"]
    credit_url_base = audit["credit_url_base"]

    article = next((a for a in audit["articles"] if a["elocation_id"] == elocation), None)
    if article is None:
        print(f"elocation_id {elocation} not in audit", file=sys.stderr)
        sys.exit(2)
    block = article["contrib_group"]["block"]

    spec = json.load(open(assignments_path))
    mode = spec["mode"]
    assert mode in ("credit", "free-text"), f"bad mode {mode!r}"
    roles_for_orcid: dict[str, list] = {}
    for entry in spec["assignments"]:
        orcid = entry.get("orcid")
        if not orcid:
            continue
        roles_for_orcid[orcid] = entry.get("roles", [])

    # The block starts with "<contrib-group>" and ends "</contrib-group>".
    cg_m = re.match(r"^(<contrib-group[^>]*>)(.*)(</contrib-group>)$", block.strip(), re.DOTALL)
    if not cg_m:
        print("Cannot parse contrib-group", file=sys.stderr)
        sys.exit(2)
    cg_open, cg_inner, cg_close = cg_m.group(1), cg_m.group(2), cg_m.group(3)

    # Replace each <contrib>...</contrib> in cg_inner using regex with a callback.
    def repl(m: re.Match) -> str:
        return rewrite_contrib(m.group(0), roles_for_orcid, mode, credit_slugs, credit_url_base)

    new_inner = re.sub(r"<contrib\b[^>]*>.*?</contrib>", repl, cg_inner, flags=re.DOTALL)
    new_block = cg_open + new_inner + cg_close

    if write_mode:
        xml_path = Path(article["xml_path"])
        text = xml_path.read_text(encoding="utf-8")
        occ = text.count(block)
        if occ != 1:
            print(f"refusing to write: old block occurs {occ} times in {xml_path}", file=sys.stderr)
            sys.exit(3)
        new_text = text.replace(block, new_block, 1)
        xml_path.write_text(new_text, encoding="utf-8")
        print(f"wrote {xml_path}", file=sys.stderr)
    else:
        print(new_block)


if __name__ == "__main__":
    main()
