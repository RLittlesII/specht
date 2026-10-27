using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Tool.Features.Rules;
using Specht.Versioning;
using Spectre.Console.Testing;

namespace Specht.Tests.Rules;

/// <summary>Pins the JSON document boundary and four-value projection without a command parser or disk.</summary>
[Trait("Tier", "Unit")]
public sealed class RulesCommandJsonUnitTests
{
    [Fact]
    public async Task AnEmptyVocabulary_WhenJsonIsSelected_ShouldWriteOneEmptyDocument()
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        files.AddFile(files.Path.Combine(root, ".specht", "manifest.json"), new MockFileData("{}"));
        SchemaVersion version = new SchemaVersionFixture();
        var versions = new SchemaVersions([version]);
        var reader = new RuleEntriesReader(EngineServices.Schemas(files, versions), new SpecManifestParser(versions));
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new RulesCommand(console, reader, errors);

        // When
        var exit = await command.ExecuteAsync(null!, new RulesCommand.Settings { Root = root, Json = true }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(0);
        errors.ToString().Should().BeEmpty();
        using var document = JsonDocument.Parse(console.Output);
        document.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        document.RootElement.GetArrayLength().Should().Be(0);
    }

    [Theory]
    [ClassData(typeof(RuleCatalogueLevels))]
    public async Task CatalogueValues_WhenJsonIsSelected_ShouldPreserveOrderedValuesAndEscapedFamilies(SpecRuleLevel level)
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        files.AddFile(files.Path.Combine(root, ".specht", "manifest.json"), new MockFileData(
            "{\"rules\":{\"SPEC010\":\"" + level.ToString().ToLowerInvariant() + "\"}}"));
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(StringComparer.Ordinal) { "SPEC020", "SPEC010" });
        version = version with
        {
            RulePages = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["SPEC010"] = "## Metadata\n\n| Property | Value |\n| --- | --- |\n| Family | Fixture \"quoted\" \\ path |\n| Default severity | Warning |\n",
                ["SPEC020"] = "## Metadata\n\n| Property | Value |\n| --- | --- |\n| Family | Fixture off |\n| Default severity | Off |\n",
            },
        };
        var versions = new SchemaVersions([version]);
        var reader = new RuleEntriesReader(EngineServices.Schemas(files, versions), new SpecManifestParser(versions));
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new RulesCommand(console, reader, errors);
        var settings = new RulesCommand.Settings { Root = root, Json = true };
        files.AddFile("outside/sentinel.txt", new MockFileData("unchanged"));
        var before = files.AllFiles.ToDictionary(static path => path, path => files.File.ReadAllText(path));

        // When
        var exit = await command.ExecuteAsync(null!, settings, TestContext.Current.CancellationToken);
        var first = console.Output;
        var secondExit = await command.ExecuteAsync(null!, settings, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(0);
        secondExit.Should().Be(exit);
        errors.ToString().Should().BeEmpty();
        console.Output.Should().Be(first + first);
        files.AllFiles.ToDictionary(static path => path, path => files.File.ReadAllText(path)).Should().BeEquivalentTo(before);
        using var document = JsonDocument.Parse(first);
        var entries = document.RootElement.EnumerateArray().ToArray();
        entries.Should().HaveCount(2);
        entries.Select(static entry => entry.GetProperty("id").GetString()).Should().Equal("SPEC010", "SPEC020");
        entries[0].GetProperty("family").GetString().Should().Be("Fixture \"quoted\" \\ path");
        entries[0].GetProperty("defaultSeverity").GetString().Should().Be("warning");
        entries[0].GetProperty("effectiveLevel").GetString().Should().Be(level.ToString().ToLowerInvariant());
        entries[1].GetProperty("family").GetString().Should().Be("Fixture off");
        entries[1].GetProperty("defaultSeverity").GetString().Should().Be("off");
        entries[1].GetProperty("effectiveLevel").GetString().Should().Be("off");
        entries.Should().OnlyContain(static entry => entry.EnumerateObject().Count() == 4);
    }
}
