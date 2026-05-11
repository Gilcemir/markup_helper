namespace DocFormatter.Core.TagEmission;

/// <summary>
/// Mapping from SciELO tag name to the hex color the legacy Word Markup VBA
/// <c>color(tag)</c> function paints onto the tag literal. The values were
/// extracted from <c>examples/phase-2/after/</c> by walking each
/// <c>word/document.xml</c>, grouping adjacent same-color runs, and pairing
/// the resulting tag literal with its <c>w:color</c> attribute.
///
/// <para>
/// Each color is the hex RGB string the OpenXML <c>w:color</c> element
/// expects (uppercase, no leading <c>#</c>). Tags emitted in their own
/// <see cref="DocumentFormat.OpenXml.Wordprocessing.Run"/> receive this color
/// via their <see cref="DocumentFormat.OpenXml.Wordprocessing.RunProperties"/>;
/// the surrounding plain-text runs keep the document default (no color),
/// which paints them black in Word.
/// </para>
/// </summary>
public static class TagColors
{
    private static readonly IReadOnlyDictionary<string, string> Map =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Document shell and titles.
            ["doc"] = "FF99CC",
            ["doctitle"] = "800080",
            ["doi"] = "FF6600",
            ["toctitle"] = "FF99CC",

            // Author block.
            ["author"] = "00FFFF",
            ["fname"] = "FF99CC",
            ["surname"] = "FF99CC",
            ["suffix"] = "0000FF",
            ["xref"] = "0000FF",
            ["authorid"] = "FF99CC",
            ["normaff"] = "008000",

            // Corresponding author.
            ["corresp"] = "FF0000",
            ["email"] = "FF0000",
            ["label"] = "FF0000",

            // Abstract and keywords.
            ["xmlabstr"] = "FF0000",
            ["p"] = "FF0000",
            ["sectitle"] = "0000FF",
            ["kwdgrp"] = "800000",
            ["kwd"] = "339966",

            // History block.
            ["hist"] = "FF6600",
            ["received"] = "008080",
            ["accepted"] = "339966",
            ["histdate"] = "800000",
        };

    /// <summary>
    /// Returns the hex color (uppercase, no leading <c>#</c>) the corpus
    /// AFTER shape uses for tag literals named <paramref name="tagName"/>,
    /// or <c>null</c> when the tag has no entry. Callers that get
    /// <c>null</c> should emit the run without an explicit color (the Word
    /// default — black — applies).
    /// </summary>
    public static string? Lookup(string tagName)
        => Map.TryGetValue(tagName, out var color) ? color : null;
}
