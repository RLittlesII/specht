using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Tool.Features.Rules;
using Spectre.Console.Testing;

namespace Specht.Tests.Rules;

/// <summary>Pins direct command rendering and typed failure folds without a parser pipeline or disk.</summary>
[Trait("Tier", "Unit")]
public sealed class RulesCommandUnitTests
{
    [Theory]
    [ClassData(typeof(RuleCatalogueLevels))]
    public async Task AValidManifest_WhenExecuted_ShouldWriteFourFieldsAndSucceedAtEveryLevel(SpecRuleLevel level)
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        files.AddFile(files.Path.Combine(root, ".specht", "manifest.json"), new MockFileData(
            "{\"schemaVersion\":\"0.2.0\",\"rules\":{\"SPEC010\":\"" + level.ToString().ToLowerInvariant() + "\"}}"));
        var reader = new RuleEntriesReader(EngineServices.Schemas(files), EngineServices.Parser());
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new RulesCommand(console, reader, errors);
        var settings = new RulesCommand.Settings { Root = root };
        files.AddFile("outside/sentinel.txt", new MockFileData("unchanged"));
        var before = files.AllFiles.ToDictionary(static path => path, path => files.File.ReadAllText(path));

        // When
        var exit = await command.ExecuteAsync(null!, settings, TestContext.Current.CancellationToken);
        var first = console.Output;
        var secondExit = await command.ExecuteAsync(null!, settings, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(0);
        secondExit.Should().Be(exit);
        console.Output.Should().Be(first + first);
        files.AllFiles.ToDictionary(static path => path, path => files.File.ReadAllText(path)).Should().BeEquivalentTo(before);
        errors.ToString().Should().BeEmpty();
        var lines = first.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(static line => line.TrimEnd('\r')).ToArray();
        lines.Should().HaveCount(EngineServices.Versions().Versions.Single(static version => version.Number.ToString() == "0.2.0").RuleIds.Count);
        lines.Should().ContainSingle(line => line == $"SPEC010\tStructure\terror\t{level.ToString().ToLowerInvariant()}");
        lines.Should().ContainSingle(static line => line == "SPEC035\tClaims\toff\toff");
        lines.Should().OnlyContain(static line => line.Split('\t').Length == 4);
    }

    [Theory]
    [ClassData(typeof(RulesCommandRejectedManifests))]
    public async Task ARejectedManifest_WhenExecuted_ShouldFoldToInvalidManifestOnStderrOnly(string manifest, string namedFault)
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        files.AddFile(files.Path.Combine(root, ".specht", "manifest.json"), new MockFileData(manifest));
        var reader = new RuleEntriesReader(EngineServices.Schemas(files), EngineServices.Parser());
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new RulesCommand(console, reader, errors);

        // When
        var exit = await command.ExecuteAsync(null!, new RulesCommand.Settings { Root = root }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(3);
        console.Output.Should().BeEmpty();
        errors.ToString().Should().StartWith("specht: ").And.Contain(namedFault).And.NotContain(root);
    }

    [Fact]
    public async Task AMissingRoot_WhenExecuted_ShouldFoldToMissingInputAndEchoTheTypedPath()
    {
        // Given
        var files = new MockFileSystem();
        var reader = new RuleEntriesReader(EngineServices.Schemas(files), EngineServices.Parser());
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new RulesCommand(console, reader, errors);

        // When
        var exit = await command.ExecuteAsync(null!, new RulesCommand.Settings { Root = "absent" }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(2);
        console.Output.Should().BeEmpty();
        errors.ToString().Should().Be($"specht: 'absent' is not a directory.{Environment.NewLine}");
    }

    [Fact]
    public async Task AMissingManifest_WhenExecuted_ShouldFoldToMissingInputAndNameTheRelativeManifest()
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        files.AddDirectory(root);
        var reader = new RuleEntriesReader(EngineServices.Schemas(files), EngineServices.Parser());
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new RulesCommand(console, reader, errors);

        // When
        var exit = await command.ExecuteAsync(null!, new RulesCommand.Settings { Root = root }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(2);
        console.Output.Should().BeEmpty();
        errors.ToString().Should().StartWith("specht: ").And.Contain(SpecTree.ManifestPath).And.NotContain(root).And.NotContain("\\");
    }
}
