using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Tool.Features.Manifest;
using Spectre.Console.Testing;

namespace Specht.Tests.Manifest;

/// <summary>Pins the command's direct output and typed failure folds without a parser pipeline.</summary>
[Trait("Tier", "Unit")]
public sealed class ManifestCommandUnitTests
{
    [Fact]
    public async Task AValidRoot_WhenExecuted_ShouldPrintOnlyTheEffectiveManifestAndLeaveAllFilesUnchanged()
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/.specht/manifest.json"] = new("{}"),
            ["outside/sentinel.bin"] = new("unchanged"),
        });
        var schemas = EngineServices.Schemas(files);
        var reader = new EffectiveManifestReader(schemas, new RuleEntriesReader(schemas, EngineServices.Parser()), EngineServices.Versions());
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new ManifestCommand(console, reader, errors);
        var before = files.AllFiles.ToDictionary(static path => path, path => files.File.ReadAllText(path));

        // When
        var settings = new ManifestCommand.Settings { Root = files.Path.GetFullPath("repo") };
        var exit = await command.ExecuteAsync(null!, settings, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(0);
        errors.ToString().Should().BeEmpty();
        JsonNode.DeepEquals(JsonNode.Parse(console.Output), reader.Read("repo")).Should().BeTrue();
        files.AllFiles.ToDictionary(static path => path, path => files.File.ReadAllText(path)).Should().BeEquivalentTo(before);
    }

    [Theory]
    [ClassData(typeof(ManifestCommandFailures))]
    public async Task InvalidInput_WhenExecuted_ShouldFoldTheTypedFailureAndWriteOnlyToStderr(string? manifest, string root, int expected)
    {
        // Given
        var files = new MockFileSystem();
        files.Directory.CreateDirectory("repo");
        if (manifest is not null)
        {
            files.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        }
        var schemas = EngineServices.Schemas(files);
        var reader = new EffectiveManifestReader(schemas, new RuleEntriesReader(schemas, EngineServices.Parser()), EngineServices.Versions());
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new ManifestCommand(console, reader, errors);
        var selectedRoot = root == "." ? files.Path.GetFullPath("repo") : root;
        var before = files.AllFiles.ToDictionary(static path => path, path => files.File.ReadAllText(path));

        // When
        var exit = await command.ExecuteAsync(null!, new ManifestCommand.Settings { Root = selectedRoot }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(expected);
        console.Output.Should().BeEmpty();
        errors.ToString().Should().StartWith("specht: ").And.NotContain("\\");
        if (root == "absent")
        {
            errors.ToString().Should().Contain("'absent' is not a directory");
        }
        else
        {
            errors.ToString().Should().Contain(SpecTree.ManifestPath).And.NotContain(files.Path.GetFullPath("repo"));
        }
        files.AllFiles.ToDictionary(static path => path, path => files.File.ReadAllText(path)).Should().BeEquivalentTo(before);
    }
}
