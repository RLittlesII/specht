using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Specht.Check;
using Specht.Composition;
using Specht.Evaluation;
using Specht.Manifest;
using Specht.Model;
using Specht.Resources;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Tool;
using Specht.Versioning;

namespace Specht.Tests.Composition;

/// <summary>
/// The engine's one registration (ADR-0001 stage D, item 0107; <c>0001-F1</c> B-001, B-004, C-5, C-9): the registrations
/// are the rule list, so every rule the engine assembly declares is resolved once, in the order the runner evaluated
/// them in when they were a hand-written list, each under its own id and together under every id of the shipped
/// vocabularies once (item 0164); the evaluator and the violation order resolve from the registrations, one instance
/// each; resolving the check
/// constructs every loader, reader and rule beneath it, so a registration one of them needs and the engine leaves out
/// fails the resolve; and a file system registered first is the one the engine reads through, the frontmatter schema store
/// it registers included (<c>0001-F7</c> B-024, item 0092). Each provider is built by
/// <see cref="TypeRegistrar.Build"/>, the one call site that builds one (ADR-0003). Nothing here touches the disk.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtEngineServiceCollectionExtensionsUnitTests
{
    [Fact]
    public void EveryConcreteRuleTheEngineAssemblyDeclares_WhenTheEnginesRulesAreResolved_ShouldBeAmongThemExactlyOnce()
    {
        // Given
        var declared = typeof(ISpechtRule).Assembly
            .GetTypes()
            .Where(static type => typeof(ISpechtRule).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false }
                && type != typeof(FaultIsolatedRule))
            .Select(static type => type.FullName)
            .ToList();
        using var resolver = (TypeResolver) new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();

        // When
        var resolved = ((IEnumerable<ISpechtRule>) resolver.Resolve(typeof(IEnumerable<ISpechtRule>))!)
            .Select(static rule => rule.Should().BeOfType<FaultIsolatedRule>().Which.Inner.GetType().FullName)
            .ToList();

        // Then
        resolved.Should().BeEquivalentTo(declared);
    }

    [Fact]
    public void TheEnginesRules_WhenResolved_ShouldComeInOrdinalOrderOfTheirIds()
    {
        // Given
        using var resolver = (TypeResolver) new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();

        // When
        var ids = ((IEnumerable<ISpechtRule>) resolver.Resolve(typeof(IEnumerable<ISpechtRule>))!).Select(static rule => rule.Id).ToList();

        // Then
        ids.Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    [Fact]
    public void EveryRuleInTheEngine_WhenItsIdIsRead_ShouldNameEachShippedRuleIdExactlyOnce()
    {
        // Given
        using var resolver = (TypeResolver) new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();

        // When
        var ids = ((IEnumerable<ISpechtRule>) resolver.Resolve(typeof(IEnumerable<ISpechtRule>))!)
            .Select(static rule => rule.Id)
            .ToList();

        // Then
        ids.Should().OnlyHaveUniqueItems().And.BeEquivalentTo(
            "SPEC001", "SPEC002", "SPEC003", "SPEC004", "SPEC010", "SPEC011", "SPEC012", "SPEC013", "SPEC020", "SPEC021", "SPEC022",
            "SPEC023", "SPEC030", "SPEC031", "SPEC032", "SPEC033", "SPEC034", "SPEC035", "SPEC040", "SPEC041", "SPEC043", "SPEC044", "SPEC050",
            "SPEC051", "SPEC052", "SPEC060", "SPEC061");
    }

    [Fact]
    public void TheEngineAlone_WhenItsCheckIsResolved_ShouldConstructEveryLoaderReaderAndRule()
    {
        // Given
        using var resolver = (TypeResolver) new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();

        // When
        var resolving = () => resolver.Resolve(typeof(SpechtCheck));

        // Then
        resolving.Should().NotThrow().Which.Should().BeOfType<SpechtCheck>();
    }

    [Fact]
    public void TheEngineAlone_WhenItsEvaluatorAndViolationOrderAreResolvedTwice_ShouldGiveTheSameInstanceOfEach()
    {
        // Given
        using var resolver = (TypeResolver) new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();

        // When
        var evaluators = (resolver.Resolve(typeof(SpechtEvaluator)), resolver.Resolve(typeof(SpechtEvaluator)));
        var orders = (resolver.Resolve(typeof(SpechtViolationOrder)), resolver.Resolve(typeof(SpechtViolationOrder)));

        // Then
        evaluators.Item1.Should().BeOfType<SpechtEvaluator>().And.BeSameAs(evaluators.Item2);
        orders.Item1.Should().BeOfType<SpechtViolationOrder>().And.BeSameAs(orders.Item2);
    }

    [Fact]
    public void AFileSystemRegisteredBeforeTheEngine_WhenTheEnginesFileSystemIsResolved_ShouldBeThatOneAndNotTheDefault()
    {
        // Given
        var registered = new MockFileSystem();
        using var resolver = (TypeResolver) new TypeRegistrar(
            new ServiceCollection().AddSingleton<IFileSystem>(registered).AddSpechtEngine()).Build();

        // When
        var resolved = resolver.Resolve(typeof(IFileSystem));

        // Then
        resolved.Should().BeSameAs(registered);
    }

    [Fact]
    public void AFileSystemRegisteredBeforeTheEngine_WhenTheEnginesSpecManifestStoreReads_ShouldReadThroughThatFileSystem()
    {
        // Given
        byte[] bytes = [(byte) '{', (byte) '}'];
        var registered = new MockFileSystem(
            new Dictionary<string, MockFileData> { [Path.Combine("repo", SpechtTree.ItemSchemaPath)] = new(bytes) });
        using var resolver = (TypeResolver) new TypeRegistrar(
            new ServiceCollection().AddSingleton<IFileSystem>(registered).AddSpechtEngine()).Build();

        // When
        var store = resolver.Resolve(typeof(SpechtManifestStore));

        // Then
        store.Should().BeOfType<SpechtManifestStore>().Which.ReadFrontmatterSchema("repo", "task.schema.json").Should().Equal(bytes);
    }

    [Fact]
    public void ACatalogRegisteredBeforeTheEngine_WhenTwoRootsAreChecked_ShouldUseThatCatalogForBothRoots()
    {
        // Given
        SchemaVersion first = new SchemaVersionFixture();
        SchemaVersion second = new SchemaVersionFixture().WithNumber(new SemanticVersion(0, 2, 0));
        var catalog = new SchemaVersions([first, second]);
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("one", SpechtTree.ManifestPath)] = new("{}"),
            [Path.Combine("two", SpechtTree.ManifestPath)] = new("""{"schemaVersion":"0.2.0"}"""),
        });
        using var resolver = (TypeResolver) new TypeRegistrar(new ServiceCollection()
            .AddSingleton<IFileSystem>(fileSystem).AddSingleton(catalog).AddSpechtEngine()).Build();
        var check = (SpechtCheck) resolver.Resolve(typeof(SpechtCheck))!;
        var parser = (SpechtManifestParser) resolver.Resolve(typeof(SpechtManifestParser))!;
        var resources = resolver.Resolve(typeof(SpechtResources));

        // When
        var one = check.Run("one", null);
        var two = check.Run("two", null);
        var again = check.Run("one", null);

        // Then
        one.SchemaVersion.Should().Be(first.Number);
        two.SchemaVersion.Should().Be(second.Number);
        again.SchemaVersion.Should().Be(first.Number);
        resolver.Resolve(typeof(SchemaVersions)).Should().BeSameAs(catalog);
        resolver.Resolve(typeof(SpechtManifestParser)).Should().BeSameAs(parser);
        resolver.Resolve(typeof(SpechtResources)).Should().BeSameAs(resources);
    }
}
