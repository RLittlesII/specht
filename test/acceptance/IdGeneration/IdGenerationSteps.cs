using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Engine.Baseline;
using Specht.Tests.Shared;
using Specht.Tool;

namespace Specht.Acceptance.IdGeneration;

/// <summary>
/// Steps for <c>src/specht/IdGeneration/.spec/id-generation.feature</c> (<c>0001-F19</c>). The root is a synthetic tree
/// holding this repository's default manifest; "a tree whose check reports a violation" adds the baseline tree under the
/// manifest's own pin. "Opts claim ids in to Nano ID" writes a <c>claim</c> entry under the top-level <c>mint</c> key in
/// C-12's shape and, as A-6 reads a scenario, a claim grammar in <c>identifiers</c> that admits the ids it mints. "The
/// tree is checked" launches the built tool on the root, and "the id settings removed" is the same manifest without
/// <c>mint</c> (A-1). An invalid manifest ends the check with <see cref="ExitCodes.InvalidManifest"/>.
/// </summary>
[Binding]
[Scope(Feature = "Id generation")]
public sealed class IdGenerationSteps
{
    [Given("a repository root with a valid Specht manifest")]
    public void GivenARepositoryRootWithAValidSpechtManifest() => _tree = new SpechtTree();

    [Given("a tree whose check reports a violation")]
    public void GivenATreeWhoseCheckReportsAViolation()
    {
        var pin = ReadManifest()["schemaVersion"]!.GetValue<string>();
        BaselineTree.Write(Tree);
        Tree.Pin(pin);
    }

    [Given("a manifest pinned to schema {word}")]
    public void GivenAManifestPinnedToSchema(string version) => Tree.Pin(version);

    [Given("a manifest that opts claim ids in to Nano ID")]
    [Given("it opts claim ids in to Nano ID")]
    public void GivenAManifestThatOptsClaimIdsInToNanoId()
    {
        var manifest = ReadManifest();
        manifest["identifiers"]!["claim"] = "^B-([0-9]{3}[a-z]?|[0-9a-z]{6})$";
        manifest["mint"] = JsonNode.Parse(
            """
            {
              "claim": {
                "strategy": "nanoid",
                "nanoid": { "alphabet": "0123456789abcdefghjkmnpqrstvwxyz", "size": 6 },
                "prefix": "B-",
                "encoding": "none",
                "attempts": 8
              }
            }
            """);
        WriteManifest(manifest);
    }

    [Given("^it declares a grammar for (ADR numbers|lesson numbers|decision numbers|scenario ids)$")]
    public void GivenItDeclaresAGrammarFor(string type)
    {
        var manifest = ReadManifest();
        manifest["identifiers"]![type switch
        {
            "ADR numbers" => "adr",
            "lesson numbers" => "lesson",
            "decision numbers" => "decision",
            _ => "scenario",
        }] = type == "scenario ids" ? "^S-[0-9]+$" : "^[0-9]{4}$";
        WriteManifest(manifest);
    }

    [When("the tree is checked")]
    public void WhenTheTreeIsChecked() => _run = AcceptanceTool.Launch(Tree.Root, "--root", ".");

    [Then("the violations reported are the ones the same tree gets with the id settings removed")]
    public void ThenTheViolationsReportedAreTheOnesTheSameTreeGetsWithTheIdSettingsRemoved()
    {
        var manifest = ReadManifest();
        manifest.Remove("mint");
        WriteManifest(manifest);
        var (stdout, stderr, exitCode) = AcceptanceTool.Launch(Tree.Root, "--root", ".");

        exitCode.Should().Be(ExitCodes.Violations, stderr);
        Run.ExitCode.Should().Be(exitCode, Run.Stderr);
        Run.Stdout.Should().Be(stdout);
    }

    [Then("the check does not end as it does for an invalid manifest")]
    public void ThenTheCheckDoesNotEndAsItDoesForAnInvalidManifest()
    {
        Run.ExitCode.Should().NotBe(ExitCodes.InvalidManifest, Run.Stderr);
    }

    [AfterScenario]
    public void DisposeTree() => _tree?.Dispose();

    private SpechtTree Tree => _tree ?? throw new InvalidOperationException("No root was prepared.");

    private string ManifestPath => Path.Combine(Tree.Root, SpechtTree.ManifestPath);

    private (string Stdout, string Stderr, int ExitCode) Run => _run ?? throw new InvalidOperationException("The tree was not checked.");

    private JsonObject ReadManifest() => JsonNode.Parse(File.ReadAllText(ManifestPath))!.AsObject();

    private void WriteManifest(JsonObject manifest) => File.WriteAllText(ManifestPath, manifest.ToJsonString());

    private SpechtTree? _tree;

    private (string Stdout, string Stderr, int ExitCode)? _run;
}
