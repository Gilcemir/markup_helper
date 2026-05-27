namespace DocFormatter.Core.Jats;

/// <summary>
/// Interactive <see cref="IConfirmer"/> policy — the default for the
/// <c>phase3</c> subcommand (ADR-006). Prints the proposal (tag, proposed value,
/// reason) to the injected output stream and reads a confirm/override response
/// from the injected input stream. An empty response accepts the proposal
/// (<see cref="ConfirmDisposition.Confirmed"/>); any other text replaces it
/// (<see cref="ConfirmDisposition.Overridden"/>). When the proposal sets
/// <see cref="Proposal.AllowsFreeText"/>, the operator may instead type the
/// free-text token (<c>f</c>) to take the document-scoped
/// <see cref="ConfirmDisposition.FreeText"/> outcome (ADR-007); that option is
/// offered only then and is never the implicit default.
/// </summary>
/// <remarks>
/// Streams are injected rather than reading <see cref="System.Console"/> directly,
/// consistent with how <c>CliApp.Run(args, out, err)</c> threads its writers.
/// </remarks>
public sealed class ConsoleConfirmer : IConfirmer
{
    /// <summary>The input token (case-insensitive) that selects free text when offered.</summary>
    public const string FreeTextToken = "f";

    private readonly TextReader _input;
    private readonly TextWriter _output;

    public ConsoleConfirmer(TextReader input, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        _input = input;
        _output = output;
    }

    /// <inheritdoc />
    public ConfirmResult Confirm(Proposal proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);

        _output.WriteLine($"Confirm <{proposal.Tag}>: {proposal.ProposedValue}");
        _output.WriteLine($"  reason: {proposal.Reason}");
        if (proposal.AllowsFreeText)
        {
            _output.WriteLine($"  [Enter] to accept, [{FreeTextToken}] to emit all roles as "
                + "free text (no content-type) for this document, or type a replacement value:");
        }
        else
        {
            _output.WriteLine("  [Enter] to accept, or type a replacement value:");
        }

        var response = _input.ReadLine();

        if (string.IsNullOrWhiteSpace(response))
        {
            return new ConfirmResult(proposal.ProposedValue, ConfirmDisposition.Confirmed);
        }

        var trimmed = response.Trim();

        // Free text is offered only when the proposal allows it, so the same token
        // typed against any other proposal is an ordinary override (never silently
        // upgraded to a document-scoped free-text switch).
        if (proposal.AllowsFreeText
            && string.Equals(trimmed, FreeTextToken, StringComparison.OrdinalIgnoreCase))
        {
            return new ConfirmResult(proposal.ProposedValue, ConfirmDisposition.FreeText);
        }

        return new ConfirmResult(trimmed, ConfirmDisposition.Overridden);
    }
}
