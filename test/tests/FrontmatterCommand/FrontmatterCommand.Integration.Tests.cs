using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Spectre.Console.Testing;

namespace Specht.Tests.FrontmatterCommand;

/// <summary>The real command graph maps file/stdin input, failure precedence and engine verdicts to its public stream.</summary>
[Trait("Tier", "Integration")]
public sealed class FrontmatterCommandIntegrationTests
{
    [Theory]
    [ClassData(typeof(FrontmatterCommandKinds))]
    public async Task ADocumentByPath_WhenChecked_ShouldPrintItsKindRuleAtTheKeysLineThenOnlyTheCount(string kind, string rule)
    {
        // Given
        var files = Files();
        using var input = new StringReader("must not be read");
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var app = EngineServices.FrontmatterApp(files, new TestConsole(), input, errors);

        // When
        var result = await app.RunAsync(["frontmatter", "--kind", kind, "drafts/issue.md", "--root", "repo"], TestContext.Current.CancellationToken);

        // Then
        result.ExitCode.Should().Be(1);
        var lines = result.Output.ReplaceLineEndings("\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(3);
        lines[1].Should().StartWith($"drafts/issue.md(3): error {rule}:").And.EndWith("[title]");
        lines[2].Should().Be("errors: 2, warnings: 0");
        errors.ToString().Should().BeEmpty();
        input.ReadToEnd().Should().Be("must not be read");
    }

    [Theory]
    [ClassData(typeof(FrontmatterCommandFailures))]
    public async Task SeveralInvalidInputs_WhenChecked_ShouldReportOnlyTheFirstFailureWithoutReadingStandardInput(
        string[] arguments, int code, string message)
    {
        // Given
        using var input = new StringReader("unread input");
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var app = EngineServices.FrontmatterApp(Files(), new TestConsole(), input, errors);

        // When
        var result = await app.RunAsync(["frontmatter", .. arguments, "--root", "absent"], TestContext.Current.CancellationToken);

        // Then
        result.ExitCode.Should().Be(code);
        result.Output.Should().BeEmpty();
        errors.ToString().Should().Contain(message);
        if (message is "story" or "Story")
        {
            errors.ToString().Should().Contain("feature").And.Contain("task").And.Contain("epic");
        }
        input.ReadToEnd().Should().Be("unread input");
    }

    [Theory]
    [ClassData(typeof(FrontmatterCommandLevels))]
    public async Task ADocumentOnStandardInput_WhenChecked_ShouldUseTheManifestLevelAndStdinLabelWithoutWriting(
        string level, int code, string summary)
    {
        // Given
        var files = Files();
        var manifest = JsonNode.Parse(files.File.ReadAllText("repo/.specht/manifest.json"))!.AsObject();
        manifest["rules"] = new JsonObject { ["SPEC003"] = level };
        files.File.WriteAllText("repo/.specht/manifest.json", manifest.ToJsonString());
        files.AddFile("repo/src/sample/.spec/README.md", new MockFileData("invalid tree"));
        var before = files.AllFiles.ToDictionary(path => path, path => files.File.ReadAllText(path));
        using var input = new StringReader("---\n\ntitle: 42\n---\n");
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var app = EngineServices.FrontmatterApp(files, new TestConsole(), input, errors);

        // When
        var result = await app.RunAsync(["frontmatter", "--kind", "task", "--stdin", "--root", "repo"], TestContext.Current.CancellationToken);

        // Then
        result.ExitCode.Should().Be(code);
        result.Output.TrimEnd().Should().EndWith(summary);
        if (level != "off")
        {
            result.Output.Should().Contain($"<stdin>(3): {level} SPEC003:");
        }
        errors.ToString().Should().BeEmpty();
        files.AllFiles.ToDictionary(path => path, path => files.File.ReadAllText(path)).Should().BeEquivalentTo(before);
    }

    [Theory]
    [ClassData(typeof(FrontmatterCommandRoots))]
    public async Task AnInvalidRootOrManifest_WhenChecked_ShouldPrintOnlyTheChecksFailureAndReturnItsCode(
        string root, string? manifest, int code, string message)
    {
        // Given
        var files = Files();
        if (manifest is null)
        {
            files.File.Delete("repo/.specht/manifest.json");
        }
        else
        {
            files.File.WriteAllText("repo/.specht/manifest.json", manifest);
        }
        using var input = new StringReader("---\ntitle: valid\n---\n");
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var app = EngineServices.FrontmatterApp(files, new TestConsole(), input, errors);

        // When
        var result = await app.RunAsync(["frontmatter", "--kind", "task", "--stdin", "--root", root], TestContext.Current.CancellationToken);

        // Then
        result.ExitCode.Should().Be(code);
        result.Output.Should().BeEmpty();
        errors.ToString().Should().Contain(message);
        errors.ToString().Should().NotContain(files.Path.GetFullPath("repo"));
    }

    [Fact]
    public async Task AValidDocument_WhenCheckedTwice_ShouldPrintTheSameCleanSummaryAndSucceed()
    {
        // Given
        var files = Files();
        files.File.WriteAllText("drafts/issue.md", "---\ntitle: valid\n---\n");
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        using var input = new StringReader(string.Empty);
        var app = EngineServices.FrontmatterApp(files, new TestConsole(), input, errors);
        string[] args = ["frontmatter", "--kind", "task", "drafts/issue.md", "--root", "repo"];

        // When
        var first = await app.RunAsync(args, TestContext.Current.CancellationToken);
        using var secondInput = new StringReader(string.Empty);
        using var secondErrors = new StringWriter(CultureInfo.InvariantCulture);
        var secondApp = EngineServices.FrontmatterApp(files, new TestConsole(), secondInput, secondErrors);
        var second = await secondApp.RunAsync(args, TestContext.Current.CancellationToken);

        // Then
        first.ExitCode.Should().Be(0);
        second.ExitCode.Should().Be(0);
        first.Output.TrimEnd().Should().Be("errors: 0, warnings: 0");
        second.Output.Should().Be(first.Output);
        errors.ToString().Should().BeEmpty();
    }

    private static MockFileSystem Files() => new(new Dictionary<string, MockFileData>
    {
        ["repo/.specht/manifest.json"] = new("""{"schemaVersion":"0.2.0","schemaSource":"disk","identifiers":{"epic":"^[0-9]{4}$"}}"""),
        ["repo/.specht/schema/frontmatter/feature.schema.json"] = new(Schema),
        ["repo/.specht/schema/frontmatter/task.schema.json"] = new(Schema),
        ["repo/.specht/schema/frontmatter/epic.schema.json"] = new(Schema),
        ["drafts/issue.md"] = new("---\n\ntitle: 42\n---\n"),
    });

    private const string Schema = """{"type":"object","required":["title"],"properties":{"title":{"type":"string"}}}""";
}
