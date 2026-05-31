using DocFormatter.Core.Jats;
using DocFormatter.Core.Rules;
using DocFormatter.Core.Rules.Phase2;
using Microsoft.Extensions.DependencyInjection;

namespace DocFormatter.Core.Pipeline;

public static class RuleRegistration
{
    public static IServiceCollection AddPhase1Rules(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IFormattingRule, ExtractTopTableRule>();
        services.AddTransient<IFormattingRule, ParseHeaderLinesRule>();
        services.AddTransient<IFormattingRule, ExtractAuthorsRule>();
        services.AddTransient<IFormattingRule, ExtractCorrespondingAuthorRule>();
        services.AddTransient<IFormattingRule, RewriteHeaderMvpRule>();
        services.AddTransient<IFormattingRule, ApplyHeaderAlignmentRule>();
        services.AddTransient<IFormattingRule, EnsureAuthorBlockSpacingRule>();
        services.AddTransient<IFormattingRule, RewriteAbstractRule>();
        services.AddTransient<IFormattingRule, LocateAbstractAndInsertElocationRule>();
        services.AddTransient<IFormattingRule, MoveHistoryRule>();
        services.AddTransient<IFormattingRule, PromoteSectionsRule>();

        return services;
    }

    public static IServiceCollection AddPhase2Rules(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Task 06 emitters. Order: elocation first so it can detect and remove
        // the standalone elocation paragraph before the abstract-heading
        // search runs (the elocation paragraph sits immediately before the
        // abstract heading in the corpus and would otherwise drift the
        // FindFollowingNonEmptyParagraph heuristic). Abstract before kwdgrp
        // mirrors corpus order; either order works because the two locators
        // do not interfere.
        services.AddTransient<IFormattingRule, EmitElocationTagRule>();
        services.AddTransient<IFormattingRule, EmitAbstractTagRule>();
        services.AddTransient<IFormattingRule, EmitKwdgrpTagRule>();

        // Task 07 emitters. EmitAuthorXrefsRule must run before
        // EmitCorrespTagRule because the corresp emitter wraps the corresp
        // paragraph (ID c1) and the author rule populates ctx with the
        // corresp index that the diagnostic block exposes. Either order works
        // semantically — the rules touch disjoint paragraphs — but author
        // first matches the corpus paragraph order.
        services.AddTransient<IFormattingRule, EmitAuthorXrefsRule>();
        services.AddTransient<IFormattingRule, EmitCorrespTagRule>();

        // Task 09 emitter (Phase 4 [hist] block). Independent of the other
        // Phase 2 rules — operates on the Received/Accepted/Published
        // paragraphs that Phase 1's MoveHistoryRule already placed before
        // the INTRODUCTION anchor. Order within AddPhase2Rules is
        // documentation-only.
        services.AddTransient<IFormattingRule, EmitHistTagRule>();

        return services;
    }

    /// <summary>
    /// Registers the four Phase 3 <see cref="IJatsInjector"/>s in their canonical
    /// run order (ADR-003), mirroring <see cref="AddPhase2Rules"/>. The order is
    /// load-bearing: the <c>other</c>-id (Critical) runs first so a missing DOI or
    /// <c>other</c> number aborts the document before the three Optional injectors
    /// do any work, and the remaining three follow the document's natural
    /// front-to-back metadata order (author-notes → back/data-availability →
    /// contrib/credit). <see cref="Phase3Pipeline"/> consumes them via
    /// <see cref="IEnumerable{T}"/> in registration order.
    /// </summary>
    public static IServiceCollection AddPhase3Injectors(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IJatsInjector, OtherIdInjector>();
        services.AddTransient<IJatsInjector, EditedByInjector>();
        services.AddTransient<IJatsInjector, DataAvailabilityInjector>();
        services.AddTransient<IJatsInjector, CreditRolesInjector>();

        return services;
    }
}
