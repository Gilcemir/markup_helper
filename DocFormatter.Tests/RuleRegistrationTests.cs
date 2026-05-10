using DocFormatter.Core.Options;
using DocFormatter.Core.Pipeline;
using DocFormatter.Core.Rules;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DocFormatter.Tests;

public sealed class RuleRegistrationTests
{
    [Fact]
    public void AddPhase1Rules_RegistersAllElevenRulesInTechSpecOrder()
    {
        var services = new ServiceCollection();
        services.AddSingleton<FormattingOptions>();

        services.AddPhase1Rules();

        using var provider = services.BuildServiceProvider();
        var ruleTypes = provider.GetServices<IFormattingRule>().Select(r => r.GetType()).ToArray();

        Assert.Equal(
            new[]
            {
                typeof(ExtractTopTableRule),
                typeof(ParseHeaderLinesRule),
                typeof(ExtractAuthorsRule),
                typeof(ExtractCorrespondingAuthorRule),
                typeof(RewriteHeaderMvpRule),
                typeof(ApplyHeaderAlignmentRule),
                typeof(EnsureAuthorBlockSpacingRule),
                typeof(RewriteAbstractRule),
                typeof(LocateAbstractAndInsertElocationRule),
                typeof(MoveHistoryRule),
                typeof(PromoteSectionsRule),
            },
            ruleTypes);
    }

    [Fact]
    public void AddPhase1Rules_RegistersEachRuleAsTransient()
    {
        var services = new ServiceCollection();

        services.AddPhase1Rules();

        var ruleDescriptors = services
            .Where(d => d.ServiceType == typeof(IFormattingRule))
            .ToArray();

        Assert.Equal(11, ruleDescriptors.Length);
        Assert.All(ruleDescriptors, d => Assert.Equal(ServiceLifetime.Transient, d.Lifetime));
    }

    [Fact]
    public void AddPhase1Rules_ResolvesFreshInstancesOnEachRequest()
    {
        var services = new ServiceCollection();
        services.AddSingleton<FormattingOptions>();
        services.AddPhase1Rules();
        using var provider = services.BuildServiceProvider();

        var first = provider.GetServices<IFormattingRule>().ToArray();
        var second = provider.GetServices<IFormattingRule>().ToArray();

        Assert.Equal(first.Length, second.Length);
        for (var i = 0; i < first.Length; i++)
        {
            Assert.NotSame(first[i], second[i]);
            Assert.IsType(first[i].GetType(), second[i]);
        }
    }

    [Fact]
    public void AddPhase1Rules_ReturnsSameServiceCollectionForFluentChaining()
    {
        var services = new ServiceCollection();

        var returned = services.AddPhase1Rules();

        Assert.Same(services, returned);
    }

    [Fact]
    public void AddPhase2Rules_RegistersZeroRulesUntilLaterTasksLand()
    {
        var services = new ServiceCollection();

        services.AddPhase2Rules();

        using var provider = services.BuildServiceProvider();
        var rules = provider.GetServices<IFormattingRule>().ToArray();

        Assert.Empty(rules);
    }

    [Fact]
    public void AddPhase2Rules_ReturnsSameServiceCollectionForFluentChaining()
    {
        var services = new ServiceCollection();

        var returned = services.AddPhase2Rules();

        Assert.Same(services, returned);
    }

    [Fact]
    public void AddPhase1Rules_NullServiceCollection_Throws()
    {
        IServiceCollection? services = null;

        Assert.Throws<ArgumentNullException>(() => services!.AddPhase1Rules());
    }

    [Fact]
    public void AddPhase2Rules_NullServiceCollection_Throws()
    {
        IServiceCollection? services = null;

        Assert.Throws<ArgumentNullException>(() => services!.AddPhase2Rules());
    }
}
