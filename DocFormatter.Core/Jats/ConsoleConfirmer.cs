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

    /// <summary>
    /// The input token (case-insensitive) that declines the proposal — the tag is
    /// not written (<see cref="ConfirmDisposition.Skipped"/>). Always available, so
    /// an operator who cannot supply a correct value can refuse rather than being
    /// forced to accept a wrong one (ADR-001).
    /// </summary>
    public const string SkipToken = "s";

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
        _output.WriteLine($"  {BuildOptionsLine(proposal)}");

        while (true)
        {
            var response = _input.ReadLine();

            // EOF (null) is not the same as an empty line: stdin is closed/redirected
            // and there is no operator to answer. Silently accepting every proposal
            // here would defeat the fail-loud intent (ADR-001), so abort with guidance
            // to pick an explicit non-interactive policy.
            if (response is null)
            {
                throw new ConfirmationInputUnavailableException(proposal);
            }

            if (string.IsNullOrWhiteSpace(response))
            {
                return new ConfirmResult(proposal.ProposedValue, ConfirmDisposition.Confirmed);
            }

            var trimmed = response.Trim();

            // Decline is always available: the tag is left unwritten (ADR-001).
            if (string.Equals(trimmed, SkipToken, StringComparison.OrdinalIgnoreCase))
            {
                return new ConfirmResult(string.Empty, ConfirmDisposition.Skipped);
            }

            // Free text is offered only when the proposal allows it, so the same token
            // typed against any other proposal is an ordinary override (never silently
            // upgraded to a document-scoped free-text switch).
            if (proposal.AllowsFreeText
                && string.Equals(trimmed, FreeTextToken, StringComparison.OrdinalIgnoreCase))
            {
                return new ConfirmResult(proposal.ProposedValue, ConfirmDisposition.FreeText);
            }

            if (proposal.AllowsOverride)
            {
                return new ConfirmResult(trimmed, ConfirmDisposition.Overridden);
            }

            // Override not allowed for this proposal (e.g. credit-roles): a typed
            // value cannot meaningfully re-map it, so re-prompt instead of recording
            // a no-op "Overridden".
            _output.WriteLine($"  invalid option. {BuildOptionsLine(proposal)}");
        }
    }

    private static string BuildOptionsLine(Proposal proposal)
    {
        var freeText = proposal.AllowsFreeText
            ? $", [{FreeTextToken}] to emit all roles as free text (no content-type) for this document"
            : string.Empty;
        var replace = proposal.AllowsOverride ? ", or type a replacement value" : string.Empty;
        return $"[Enter] to accept, [{SkipToken}] to skip (write nothing){freeText}{replace}:";
    }
}

/// <summary>
/// Raised by <see cref="ConsoleConfirmer"/> when interactive confirmation is
/// required but no input is available (stdin closed/redirected, EOF). Derives from
/// <see cref="System.OperationCanceledException"/> so the Phase 3 pipeline aborts
/// the document immediately — like <see cref="PromptNotAllowedException"/> — rather
/// than silently confirming the proposal.
/// </summary>
public sealed class ConfirmationInputUnavailableException : System.OperationCanceledException
{
    /// <summary>The proposal that could not be confirmed.</summary>
    public Proposal Proposal { get; }

    public ConfirmationInputUnavailableException(Proposal proposal)
        : base(BuildMessage(proposal))
    {
        Proposal = proposal;
    }

    private static string BuildMessage(Proposal proposal)
    {
        System.ArgumentNullException.ThrowIfNull(proposal);
        return $"no input available to confirm '{proposal.Tag}' (proposed '{proposal.ProposedValue}'): "
            + "rerun with --non-interactive=accept or --non-interactive=fail.";
    }
}
