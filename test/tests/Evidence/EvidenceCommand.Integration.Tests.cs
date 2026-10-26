using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Specht.Tests.Evidence;

/// <summary>
/// The evidence command's argument surface: positional paths supply the inputs in command-line order, and the help
/// shows them as the only way to name evidence (0201-F5 B-023, B-025 to B-027).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class EvidenceCommandIntegrationTests : IDisposable
{
    [Fact]
    public async Task TwoPositionalExports_WhenReconciled_ShouldSupplyBothWithoutError()
    {
        // Given
        Write();

        // When
        var result = await Run(["a.json", "b.json", "--json"]);

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        Supplied(result.Output).Should().Equal("a.json", "b.json");
    }

    [Fact]
    public async Task SeveralPositionalDirectories_WhenReconciled_ShouldNumberExternalDirectoriesInCommandLineOrder()
    {
        // Given
        Write();
        var first = External("a.json");
        var second = External("b.json");
        var third = External("a.json");

        // When
        var result = await Run([second, third, first, "--json"]);

        // Then
        Supplied(result.Output).Should().Equal("external-evidence-1/b.json", "external-evidence-2/a.json", "external-evidence-3/a.json");
    }

    [Fact]
    public async Task TheSameExportNamedTwice_WhenReconciled_ShouldSupplyItOnce()
    {
        // Given
        Write();

        // When
        var result = await Run(["a.json", "a.json", "b.json", "--json"]);

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        Supplied(result.Output).Should().Equal("a.json", "b.json");
    }

    [Fact]
    public async Task NoPath_WhenReconciled_ShouldFailWithIncompleteInputAndTheNoEvidenceFinding()
    {
        // Given
        Write();

        // When
        var result = await Run(["--json"]);

        // Then
        result.ExitCode.Should().Be(2);
        JsonNode.Parse(result.Output)!["findings"]!.AsArray().Should().Contain(static item =>
            item!["kind"]!.GetValue<string>() == "missingCompilation"
            && item["message"]!.GetValue<string>().Contains("No evidence was supplied", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TheEvidenceHelp_WhenRequested_ShouldShowOptionalPositionalPathsAndNoEvidenceOption()
    {
        // Given
        var console = new TestConsole();
        var app = EngineServices.EvidenceApp(console);

        // When
        var result = await app.RunAsync(["evidence", "--help"], TestContext.Current.CancellationToken);

        // Then
        result.ExitCode.Should().Be(0);
        var help = Regex.Replace(result.Output, @"\s+", " ");
        help.Should().MatchRegex(@"evidence \[PATH\w*\] \[OPTIONS\]");
        help.Should().NotContain("--evidence");
    }

    public void Dispose()
    {
        _root.Dispose();
        foreach (var directory in _externals)
        {
            Directory.Delete(directory, true);
        }
    }

    private static JsonObject Case => new()
    {
        ["caseId"] = "case-1",
        ["methodSymbolId"] = "M:Shared.Checks",
        ["rowId"] = null,
        ["displayName"] = "case-1",
    };

    private static JsonObject Outcome => new() { ["caseId"] = "case-1", ["outcome"] = "passed", ["detail"] = null };

    private static string[] Supplied(string output) =>
        [.. JsonNode.Parse(output)!["scope"]!["supplied"]!.AsArray().Select(static item => item!["path"]!.GetValue<string>())];

    private string External(string name)
    {
        var directory = Directory.CreateTempSubdirectory("specht-external-evidence-").FullName;
        _externals.Add(directory);
        File.Copy(Path.Combine(_root.Root, name), Path.Combine(directory, name));
        return directory;
    }

    private void Write()
    {
        _root.Write("a.json", "a", 1, "run-1", new JsonArray(Case), new JsonArray(Outcome));
        _root.Write("b.json", "b", 1, "run-1", new JsonArray(Case), new JsonArray(Outcome));
    }

    private async Task<CommandAppResult> Run(string[] arguments)
    {
        var app = EngineServices.EvidenceApp(new TestConsole());
        return await app.RunAsync(["evidence", "--root", _root.Root, .. arguments], TestContext.Current.CancellationToken);
    }

    private readonly PartitionedEvidenceRoot _root = new();
    private readonly List<string> _externals = [];
}
