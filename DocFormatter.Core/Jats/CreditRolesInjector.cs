using System.Xml.Linq;
using DocFormatter.Core.Pipeline;

namespace DocFormatter.Core.Jats;

/// <summary>
/// Injects CRediT <c>&lt;role content-type="…"&gt;</c> elements into each
/// <c>&lt;contrib&gt;</c>, after that contributor's <c>&lt;xref&gt;</c> elements,
/// from the docx CREDIT statement (<see cref="DocxSource.CreditStatementRaw"/>).
/// The statement is parsed (<see cref="CreditStatementParser"/>), each written
/// term is mapped to a CRediT URL (<see cref="CreditTermTable"/>), and each author
/// key is resolved to a single <c>&lt;contrib&gt;</c>
/// (<see cref="AuthorInitialsResolver"/>). Roles are applied automatically only
/// when the statement is structured and <em>every</em> term maps and <em>every</em>
/// author resolves uniquely; otherwise — free prose, an unrecognized term, or an
/// unresolved author — a <see cref="Proposal"/> is sent to the
/// <see cref="IConfirmer"/> gate and only the operator-confirmed subset is written
/// (ADR-005). Whatever the path, the structured result is recorded as
/// <see cref="Phase3Context.Credit"/> for the diagnostic and the batch summary.
/// </summary>
/// <remarks>
/// Severity is <see cref="RuleSeverity.Optional"/>: the CREDIT statement is
/// author-supplied metadata the docx may not carry, so an absent statement is
/// reported and skipped, not an error. The injector is idempotent (ADR-005): a
/// <c>&lt;contrib&gt;</c> that already has any <c>&lt;role&gt;</c> is left
/// untouched and reported, so operator hand-edits survive re-runs. Auto and
/// confirmed paths emit only CRediT-typed roles, so the SPS all-or-nothing
/// <c>@content-type</c> rule holds by construction. The one exception is the
/// operator-chosen <see cref="ConfirmDisposition.FreeText"/> outcome (ADR-007):
/// on that disposition the injector emits the written term of every resolved
/// author as a <c>&lt;role&gt;</c> <em>without</em> <c>@content-type</c>,
/// document-wide, so the document stays uniform the other way.
/// </remarks>
public sealed class CreditRolesInjector : IJatsInjector
{
    private const string ContribName = "contrib";
    private const string XrefName = "xref";
    private const string RoleName = "role";
    private const string ContentTypeAttribute = "content-type";

    /// <summary>The applied-author set of a path that wrote nothing (skipped).</summary>
    private static readonly IReadOnlySet<string> NoAuthors = new HashSet<string>(StringComparer.Ordinal);

    /// <inheritdoc />
    public string Name => "credit-roles";

    /// <inheritdoc />
    public RuleSeverity Severity => RuleSeverity.Optional;

    /// <inheritdoc />
    public void Apply(Phase3Context ctx, IReport report)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(report);

        // The CREDIT statement is author-supplied; an absent section is a valid
        // downstream-reportable state, not an error (DocxSource contract). A
        // present header with an emptied body, however, is a statement the
        // operator expects to see — never drop it silently (INV-02, ADR-003).
        var raw = ctx.Source.CreditStatementRaw;
        if (string.IsNullOrWhiteSpace(raw))
        {
            if (ctx.Source.CreditHeaderFound)
            {
                report.Warn(Name, "CREDIT STATEMENT header found but the body is empty; nothing to apply.");
                ctx.Credit = EmptyOutcome(CreditDisposition.HeaderEmpty);
                return;
            }

            report.Info(Name, "No CREDIT statement on the docx source; skipped.");
            ctx.Credit = EmptyOutcome(CreditDisposition.Absent);
            return;
        }

        var statement = CreditStatementParser.Parse(raw);
        if (statement.Shape == CreditShape.Prose)
        {
            // Free prose: verbs are not CRediT terms and mapping is genuine
            // natural-language judgment, so never auto-apply (ADR-005) — surface it.
            var result = ctx.Confirm.Confirm(new Proposal(
                Name,
                "(free prose)",
                "CREDIT statement is free prose; CRediT roles cannot be auto-mapped.")
            {
                AllowsOverride = false,
            });
            report.Warn(Name, $"CREDIT statement is free prose; roles not auto-applied ({result.Disposition}).");
            ctx.Credit = new CreditOutcome(raw, CreditShape.Prose, Array.Empty<CreditEntryOutcome>(), CreditDisposition.Prose);
            return;
        }

        var contribs = ctx.Xml.Descendants().Where(e => e.Name.LocalName == ContribName).ToList();
        var plan = BuildPlan(statement.Entries, contribs, out var unknownTerms, out var unresolvedAuthors);

        // Fully resolved (structured + all terms map + all authors unique): the
        // confidence gate is satisfied, so apply without prompting (ADR-001/005).
        if (unknownTerms.Count == 0 && unresolvedAuthors.Count == 0)
        {
            var applied = Emit(plan, report, ConfirmDisposition.AutoApplied);
            ctx.Credit = BuildOutcome(raw, statement.Shape, plan, applied, CreditDisposition.AutoApplied);
            return;
        }

        // Otherwise the document cannot be cleanly auto-mapped: propose, and apply
        // only the clean subset if the operator confirms (all-or-nothing holds
        // because every emitted role still carries @content-type). The operator may
        // instead switch the whole document to free text (ADR-007) — this is the
        // only branch that offers it.
        var reason = BuildReason(unknownTerms, unresolvedAuthors);
        var confirm = ctx.Confirm.Confirm(
            new Proposal(Name, SummarizeClean(plan), reason) { AllowsFreeText = true, AllowsOverride = false });
        if (confirm.Disposition == ConfirmDisposition.Skipped)
        {
            report.Warn(Name, $"CRediT roles not applied ({reason}).");
            ctx.Credit = BuildOutcome(raw, statement.Shape, plan, NoAuthors, CreditDisposition.Skipped);
            return;
        }

        if (confirm.Disposition == ConfirmDisposition.FreeText)
        {
            var placed = EmitFreeText(plan, report, unresolvedAuthors);
            ctx.Credit = BuildOutcome(raw, statement.Shape, plan, placed, CreditDisposition.FreeText);
            return;
        }

        var confirmed = Emit(plan.Where(p => p.IsClean).ToList(), report, confirm.Disposition);
        report.Warn(Name, $"Unresolved CRediT left for manual handling ({reason}).");
        ctx.Credit = BuildOutcome(raw, statement.Shape, plan, confirmed, CreditDisposition.Confirmed);
    }

    /// <summary>An outcome with no body to parse (header-empty or absent).</summary>
    private static CreditOutcome EmptyOutcome(string disposition)
        => new(Raw: null, CreditShape.Prose, Array.Empty<CreditEntryOutcome>(), disposition);

    /// <summary>
    /// Projects the plan onto the <see cref="CreditOutcome"/> recorded on the
    /// context (ADR-005): one entry per parsed author, in statement order, marked
    /// <see cref="CreditEntryOutcome.Applied"/> only when this run wrote roles for
    /// it (<paramref name="appliedAuthors"/>, keyed by author key).
    /// </summary>
    private static CreditOutcome BuildOutcome(
        string raw,
        CreditShape shape,
        IReadOnlyList<PlanItem> plan,
        IReadOnlySet<string> appliedAuthors,
        string disposition)
    {
        var entries = plan
            .Select(item => new CreditEntryOutcome(
                item.AuthorKey,
                item.WrittenTerms,
                CreditResolution.From(item.Status),
                item.UnknownTerms,
                appliedAuthors.Contains(item.AuthorKey)))
            .ToList();
        return new CreditOutcome(raw, shape, entries, disposition);
    }

    /// <summary>
    /// Builds the per-contributor plan: resolves each author and maps each term,
    /// recording any unrecognized terms and unresolved authors that gate the
    /// document to a prompt. A plan item is <see cref="PlanItem.IsClean"/> when its
    /// author resolved uniquely and all its terms mapped.
    /// </summary>
    private static List<PlanItem> BuildPlan(
        IReadOnlyList<CreditEntry> entries,
        IReadOnlyList<XElement> contribs,
        out List<string> unknownTerms,
        out List<string> unresolvedAuthors)
    {
        unknownTerms = new List<string>();
        unresolvedAuthors = new List<string>();
        var plan = new List<PlanItem>();

        foreach (var entry in entries)
        {
            var resolution = AuthorInitialsResolver.Resolve(entry.AuthorKey, contribs);
            if (resolution.Status != ResolveStatus.Resolved)
            {
                unresolvedAuthors.Add($"{entry.AuthorKey} ({resolution.Status})");
            }

            var roles = new List<CreditRole>();
            var entryUnknownTerms = new List<string>();
            foreach (var term in entry.Terms)
            {
                if (CreditTermTable.TryMap(term, out var role))
                {
                    if (!roles.Any(r => r.ContentTypeUrl == role.ContentTypeUrl))
                    {
                        roles.Add(role);
                    }
                }
                else
                {
                    entryUnknownTerms.Add(term);
                }
            }

            unknownTerms.AddRange(entryUnknownTerms);

            // "Clean" means the author resolved AND every written term mapped — not
            // roles.Count == Terms.Count, which wrongly flags a duplicate-spelling
            // term (e.g. hyphen vs en-dash) as unclean because the two terms collapse
            // to one URL, silently dropping an otherwise auto-applicable author.
            var isClean = resolution.Status == ResolveStatus.Resolved && entryUnknownTerms.Count == 0;
            plan.Add(new PlanItem(
                entry.AuthorKey,
                resolution.Contrib,
                resolution.Status,
                roles,
                entry.Terms,
                entryUnknownTerms,
                isClean));
        }

        return plan;
    }

    /// <summary>
    /// Writes the roles for each clean plan item, skipping a contributor that
    /// already carries any <c>&lt;role&gt;</c> (idempotency, ADR-005) and reporting
    /// every disposition. Returns the author keys that received roles in this run.
    /// </summary>
    private IReadOnlySet<string> Emit(IReadOnlyList<PlanItem> plan, IReport report, ConfirmDisposition disposition)
    {
        var appliedAuthors = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in plan)
        {
            if (!item.IsClean || item.Contrib is null || item.Roles.Count == 0)
            {
                continue;
            }

            if (item.Contrib.Elements().Any(e => e.Name.LocalName == RoleName))
            {
                report.Info(Name, $"<{ContribName}> for '{item.AuthorKey}' already has <{RoleName}>; skipped.");
                continue;
            }

            EmitRoles(item.Contrib, item.Roles);
            appliedAuthors.Add(item.AuthorKey);
            var applied = string.Join(", ", item.Roles.Select(r => r.Display));
            report.Info(Name, $"Injected {item.Roles.Count} <{RoleName}> for '{item.AuthorKey}' ({disposition}): {applied}.");
        }

        return appliedAuthors;
    }

    /// <summary>
    /// The operator-chosen free-text path (ADR-007): emits the verbatim written
    /// term of every resolved author as a <c>&lt;role&gt;</c> <em>without</em>
    /// <c>@content-type</c> — including terms that would have CRediT-matched — so
    /// the whole document is uniform (the SPS per-document all-or-nothing rule).
    /// Idempotency is unchanged (a <c>&lt;contrib&gt;</c> that already has a
    /// <c>&lt;role&gt;</c> is skipped). Authors that did not resolve cannot be
    /// placed (e.g. a key that matches a <c>&lt;suffix&gt;</c> rather than a
    /// <c>&lt;surname&gt;</c>); they are reported, never silently dropped.
    /// Returns the author keys that received roles in this run.
    /// </summary>
    private IReadOnlySet<string> EmitFreeText(
        IReadOnlyList<PlanItem> plan,
        IReport report,
        IReadOnlyList<string> unresolvedAuthors)
    {
        var appliedAuthors = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in plan)
        {
            if (item.Contrib is null || item.WrittenTerms.Count == 0)
            {
                continue;
            }

            if (item.Contrib.Elements().Any(e => e.Name.LocalName == RoleName))
            {
                report.Info(Name, $"<{ContribName}> for '{item.AuthorKey}' already has <{RoleName}>; skipped.");
                continue;
            }

            EmitRoles(item.Contrib, item.WrittenTerms.Select(t => new CreditRole(t, ContentTypeUrl: null)).ToList());
            appliedAuthors.Add(item.AuthorKey);
            var applied = string.Join(", ", item.WrittenTerms);
            report.Info(
                Name,
                $"Injected {item.WrittenTerms.Count} free-text <{RoleName}> for '{item.AuthorKey}' "
                    + $"({ConfirmDisposition.FreeText}): {applied}.");
        }

        if (unresolvedAuthors.Count > 0)
        {
            // ADR-007 keeps suffix/composite-label resolution out of scope, so an
            // unresolved key still cannot be placed — surface it instead of dropping
            // it silently.
            report.Warn(
                Name,
                $"Free-text roles emitted; {unresolvedAuthors.Count} author(s) unresolved and not placed: "
                    + $"{string.Join(", ", unresolvedAuthors)}.");
        }

        return appliedAuthors;
    }

    /// <summary>
    /// Inserts one <c>&lt;role&gt;</c> per role after the contributor's last
    /// <c>&lt;xref&gt;</c> (else its last element child), chaining so the roles stay
    /// in statement order and the surrounding indentation is preserved. A role with
    /// a <see langword="null"/> <see cref="CreditRole.ContentTypeUrl"/> is written
    /// as free text without the <c>@content-type</c> attribute (ADR-007).
    /// </summary>
    private static void EmitRoles(XElement contrib, IReadOnlyList<CreditRole> roles)
    {
        var ns = contrib.Name.Namespace;
        var anchor = contrib.Elements().LastOrDefault(e => e.Name.LocalName == XrefName)
            ?? contrib.Elements().Last();
        var depth = IndentDepthOf(anchor);

        foreach (var role in roles)
        {
            var attributes = role.ContentTypeUrl is null
                ? null
                : new[] { new XAttribute(ContentTypeAttribute, role.ContentTypeUrl) };
            var element = JatsXmlWriter.BuildLeaf(ns + RoleName, role.Display, attributes);
            JatsXmlWriter.InsertAfter(anchor, element, depth);
            anchor = element;
        }
    }

    private static string SummarizeClean(IReadOnlyList<PlanItem> plan)
    {
        var clean = plan.Where(p => p.IsClean).Select(p => p.AuthorKey).ToList();
        return clean.Count == 0
            ? "(no contributor fully resolved)"
            : $"apply CRediT to: {string.Join(", ", clean)}";
    }

    private static string BuildReason(IReadOnlyList<string> unknownTerms, IReadOnlyList<string> unresolvedAuthors)
    {
        var parts = new List<string>();
        if (unknownTerms.Count > 0)
        {
            parts.Add($"unrecognized term(s): {string.Join(", ", unknownTerms.Distinct(StringComparer.Ordinal))}");
        }

        if (unresolvedAuthors.Count > 0)
        {
            parts.Add($"unresolved author(s): {string.Join(", ", unresolvedAuthors)}");
        }

        return string.Join("; ", parts);
    }

    /// <summary>
    /// The indentation depth (tab count) of the line carrying <paramref name="anchor"/>,
    /// read from its preceding whitespace text node so injected siblings align with
    /// it. Returns 0 when no leading whitespace is present.
    /// </summary>
    private static int IndentDepthOf(XElement anchor)
    {
        if (anchor.PreviousNode is not XText text)
        {
            return 0;
        }

        var value = text.Value;
        var newLineIndex = value.LastIndexOf('\n');
        var indent = newLineIndex >= 0 ? value[(newLineIndex + 1)..] : value;
        return indent.Count(c => c == '\t');
    }

    private sealed record PlanItem(
        string AuthorKey,
        XElement? Contrib,
        ResolveStatus Status,
        IReadOnlyList<CreditRole> Roles,
        IReadOnlyList<string> WrittenTerms,
        IReadOnlyList<string> UnknownTerms,
        bool IsClean);
}
