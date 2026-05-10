using DocFormatter.Core.Rules;
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

        // Phase 2 emitter rules are added by tasks 06 / 07 / 09.

        return services;
    }
}
