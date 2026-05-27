namespace DocFormatter.Core.Reporting;

/// <summary>
/// Result of a <see cref="Phase3DiffUtility.CompareInjectedOnly"/> invocation.
/// <see cref="InjectedTagsOnly"/> is <c>true</c> only when the produced XML is
/// the source XML with whole lines inserted, where every inserted block carries
/// at least one Phase 3 injected-tag signature and no source line was removed or
/// modified. On a violation the offending lines are surfaced for diagnostics:
/// <see cref="RemovedOrModifiedLines"/> holds source lines absent from the
/// produced output (a deletion or in-place edit), and
/// <see cref="UnexpectedInsertedLines"/> holds inserted lines that belong to a
/// block carrying none of the injected-tag signatures (incidental reformatting).
/// </summary>
public sealed record Phase3DiffResult(
    bool InjectedTagsOnly,
    IReadOnlyList<string> RemovedOrModifiedLines,
    IReadOnlyList<string> UnexpectedInsertedLines);

/// <summary>
/// Line-level comparator for the Phase 3 golden-corpus gate (task 12). Unlike the
/// docx body-text comparator <see cref="Phase2DiffUtility"/>, Phase 3 modifies XML
/// text in place with whitespace preserved (ADR-006), so the only legitimate
/// difference between a source document and its injected output is whole lines
/// inserted for the four Phase 3 tags: <c>&lt;article-id pub-id-type="other"&gt;</c>,
/// <c>&lt;fn fn-type="edited-by"&gt;</c>, <c>&lt;sec sec-type="data-availability"&gt;</c>,
/// and CRediT <c>&lt;role content-type="http://credit.niso.org/contributor-roles/…"&gt;</c>.
///
/// <para>
/// The comparison runs a longest-common-subsequence line diff between the two
/// documents and classifies the result. A pass requires that (1) no source line
/// is deleted or modified — which proves the whitespace-preserving writer did not
/// reformat anything — and (2) every contiguous run of inserted lines contains at
/// least one injected-tag signature, so the wrapper/child lines an injected block
/// emits (e.g. <c>&lt;label&gt;</c>, <c>&lt;p&gt;</c>, <c>&lt;title&gt;</c>, the
/// closing tag) are accepted as part of that block while a stray inserted line is
/// not.
/// </para>
/// </summary>
public static class Phase3DiffUtility
{
    /// <summary>
    /// Substrings that mark a line as belonging to one of the four Phase 3 tags.
    /// A contiguous block of inserted lines is accepted when any of its lines
    /// contains one of these (the block's wrapper/child lines ride along).
    /// </summary>
    public static readonly IReadOnlyList<string> InjectedTagSignatures = new[]
    {
        "pub-id-type=\"other\"",
        "fn-type=\"edited-by\"",
        "sec-type=\"data-availability\"",
        "content-type=\"http://credit.niso.org/contributor-roles/",
    };

    public static Phase3DiffResult CompareInjectedOnly(string sourceXml, string producedXml)
    {
        ArgumentNullException.ThrowIfNull(sourceXml);
        ArgumentNullException.ThrowIfNull(producedXml);

        // Split on '\n' keeping any trailing '\r'; both sides share the same
        // newline style (the writer preserves the source's), so equality holds
        // and a divergent newline would correctly surface as a difference.
        var source = sourceXml.Split('\n');
        var produced = producedXml.Split('\n');

        var ops = DiffLines(source, produced);

        var removedOrModified = new List<string>();
        var unexpectedInserted = new List<string>();
        var currentBlock = new List<string>();

        void FlushBlock()
        {
            if (currentBlock.Count == 0)
            {
                return;
            }
            var hasSignature = currentBlock.Any(LineHasSignature);
            if (!hasSignature)
            {
                unexpectedInserted.AddRange(currentBlock);
            }
            currentBlock.Clear();
        }

        foreach (var (kind, line) in ops)
        {
            switch (kind)
            {
                case DiffKind.Insert:
                    currentBlock.Add(line);
                    break;
                case DiffKind.Delete:
                    FlushBlock();
                    removedOrModified.Add(line);
                    break;
                default: // Equal
                    FlushBlock();
                    break;
            }
        }
        FlushBlock();

        var injectedOnly = removedOrModified.Count == 0 && unexpectedInserted.Count == 0;
        return new Phase3DiffResult(injectedOnly, removedOrModified, unexpectedInserted);
    }

    private static bool LineHasSignature(string line)
    {
        foreach (var signature in InjectedTagSignatures)
        {
            if (line.Contains(signature, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    private enum DiffKind
    {
        Equal,
        Insert, // present in produced only
        Delete, // present in source only
    }

    // Longest-common-subsequence line diff. Walks the two line arrays, emitting
    // Equal for matched lines, Delete for source-only lines, Insert for
    // produced-only lines, in original order.
    private static List<(DiffKind Kind, string Line)> DiffLines(string[] a, string[] b)
    {
        var n = a.Length;
        var m = b.Length;
        var lcs = new int[n + 1, m + 1];
        for (var i = n - 1; i >= 0; i--)
        {
            for (var j = m - 1; j >= 0; j--)
            {
                lcs[i, j] = a[i] == b[j]
                    ? lcs[i + 1, j + 1] + 1
                    : Math.Max(lcs[i + 1, j], lcs[i, j + 1]);
            }
        }

        var ops = new List<(DiffKind, string)>(n + m);
        int x = 0, y = 0;
        while (x < n && y < m)
        {
            if (a[x] == b[y])
            {
                ops.Add((DiffKind.Equal, a[x]));
                x++;
                y++;
            }
            else if (lcs[x + 1, y] >= lcs[x, y + 1])
            {
                ops.Add((DiffKind.Delete, a[x]));
                x++;
            }
            else
            {
                ops.Add((DiffKind.Insert, b[y]));
                y++;
            }
        }
        while (x < n)
        {
            ops.Add((DiffKind.Delete, a[x]));
            x++;
        }
        while (y < m)
        {
            ops.Add((DiffKind.Insert, b[y]));
            y++;
        }
        return ops;
    }
}
