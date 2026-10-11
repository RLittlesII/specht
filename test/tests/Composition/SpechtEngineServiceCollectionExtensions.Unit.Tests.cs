using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Specht.Composition;
using Specht.Rules;
using Specht.Tool;

namespace Specht.Tests.Composition;

/// <summary>
/// The engine's one registration (ADR-0001 stage D, item 0107; <c>0001-F1</c> B-001, B-004, C-5, C-9): the registrations
/// are the rule list, so every rule the engine assembly declares is resolved once, in the order the runner evaluated
/// them in when they were a hand-written list, reporting the twenty-one ids of the vocabulary; resolving the runner
/// constructs every loader, reader and rule beneath it, so a registration one of them needs and the engine leaves out
/// fails the resolve; and a file system registered first is the one the engine reads through. Each provider is built by
/// <see cref="TypeRegistrar.Build"/>, the one call site that builds one (ADR-0003). Nothing here touches the disk.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtEngineServiceCollectionExtensionsUnitTests
{
    [Fact]
    public void EveryConcreteRuleTheEngineAssemblyDeclares_WhenTheEnginesRulesAreResolved_ShouldBeAmongThemExactlyOnce()
    {
        // Given
        var declared = typeof(ISpecRule).Assembly
            .GetTypes()
            .Where(static type => typeof(ISpecRule).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false })
            .Select(static type => type.FullName)
            .ToList();
        using var resolver = (TypeResolver)new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();

        // When
        var resolved = ((IEnumerable<ISpecRule>)resolver.Resolve(typeof(IEnumerable<ISpecRule>))!)
            .Select(static rule => rule.GetType().FullName)
            .ToList();

        // Then
        resolved.Should().BeEquivalentTo(declared);
    }

    [Fact]
    public void TheEnginesRules_WhenResolved_ShouldComeInOrdinalOrderOfTheirIds()
    {
        // Given
        using var resolver = (TypeResolver)new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();

        // When
        var ids = ((IEnumerable<ISpecRule>)resolver.Resolve(typeof(IEnumerable<ISpecRule>))!).Select(static rule => rule.Id).ToList();

        // Then
        ids.Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    [Fact]
    public void EveryRuleInTheEngine_WhenItsReportedIdsAreRead_ShouldNameExactlyTheTwentyOneVersion1RuleIds()
    {
        // Given
        using var resolver = (TypeResolver)new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();

        // When
        var ids = ((IEnumerable<ISpecRule>)resolver.Resolve(typeof(IEnumerable<ISpecRule>))!)
            .SelectMany(static rule => rule.ReportedIds)
            .ToList();

        // Then
        ids.Should().BeEquivalentTo(
            "SPEC001", "SPEC002", "SPEC003", "SPEC004", "SPEC010", "SPEC011", "SPEC012", "SPEC013", "SPEC020", "SPEC021", "SPEC030",
            "SPEC031", "SPEC040", "SPEC041", "SPEC043", "SPEC044", "SPEC050", "SPEC051", "SPEC052", "SPEC060", "SPEC061");
    }

    [Fact]
    public void TheEngineAlone_WhenItsRunnerIsResolved_ShouldConstructEveryLoaderReaderAndRule()
    {
        // Given
        using var resolver = (TypeResolver)new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();

        // When
        var resolving = () => resolver.Resolve(typeof(SpechtRunner));

        // Then
        resolving.Should().NotThrow().Which.Should().BeOfType<SpechtRunner>();
    }

    [Fact]
    public void AFileSystemRegisteredBeforeTheEngine_WhenTheEnginesFileSystemIsResolved_ShouldBeThatOneAndNotTheDefault()
    {
        // Given
        var registered = new MockFileSystem();
        using var resolver = (TypeResolver)new TypeRegistrar(
            new ServiceCollection().AddSingleton<IFileSystem>(registered).AddSpechtEngine()).Build();

        // When
        var resolved = resolver.Resolve(typeof(IFileSystem));

        // Then
        resolved.Should().BeSameAs(registered);
    }
}
