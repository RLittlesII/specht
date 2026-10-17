using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Report;

public sealed partial class ReportSteps
{
    [Given("the manifest is deleted from the root")]
    public void GivenTheExplainManifestIsDeleted() => File.Delete(Path.Combine(Tree.Root, SpecTree.ManifestPath));

    [Given("the root's manifest is replaced by a file that is not well-formed JSON")]
    public void GivenTheExplainManifestDoesNotParse() => WriteExplainManifest("{");

    [Given("the root's manifest is replaced by one carrying a key the tool does not know")]
    public void GivenTheExplainManifestHasAnUnknownKey() => WriteExplainManifest("""{"unknown":true}""");

    [Given("the root's manifest is replaced by one pinning version 7.0.0")]
    public void GivenTheExplainPinIsNotShipped() => WriteExplainManifest("""{"schemaVersion":"7.0.0"}""");

    [Given("the root's manifest records an upstream schema source and explicitly selects the embedded source")]
    public void GivenTheExplainSourceContradictsTheManifest() => WriteExplainManifest("""
        {"schemaVersion":"0.2.0","schemaSource":"embedded","spechtSchema":{
          "location":"https://schemas.example/","version":"test","hashes":{
            "feature":"sha256:HASH","task":"sha256:HASH"}}}
        """.Replace("HASH", new string('0', 64), StringComparison.Ordinal));

    [When("the tool is asked to explain {word}")]
    public void WhenTheToolIsAskedToExplain(string id)
    {
        _explainedId = id;
        if (id == "SPEC031" && File.Exists(Path.Combine(Tree.Root, SpecTree.ManifestPath)))
        {
            // A normal check of this tree fails. Explain must still print only the page.
            Tree.WriteFeature("0001", "F1", featureFile: null);
        }
        (_stdout, _stderr, _explainExit) = AcceptanceTool.Launch(Tree.Root, "--root", ".", "--explain", id);
    }

    [Then("the standard output carries what the rule checks")]
    public void ThenExplainCarriesWhatTheRuleChecks() => _stdout.Should().Contain("## Cause");

    [Then("what the rule expects")]
    public void ThenExplainCarriesWhatTheRuleExpects() => _stdout.Should().Contain("## How to fix violations");

    [Then("the schema version the rule belongs to")]
    public void ThenExplainCarriesTheSchemaVersion() => _stdout.Should().Contain(ExplainPage);

    [Then("the entire authoritative page is printed, including YAML frontmatter")]
    public void ThenExplainPrintsTheEntirePage() => _stdout.Should().Be(ExplainPage, _stderr);

    [Then("no check ran")]
    public void ThenExplainRanNoCheck()
    {
        _stdout.Should().Be(ExplainPage);
        _stderr.Should().BeEmpty();
        Tree.Run().Violations.Should().NotBeEmpty();
    }

    [Then("the standard error names {word}")]
    public void ThenExplainNamesTheUnknownId(string id) => _stderr.Should().Contain(id);

    [Then("the standard output is empty")]
    [Then("no rule text is printed")]
    public void ThenExplainPrintedNoText() => _stdout.Should().BeEmpty();

    [Then("the exit code is {int}")]
    public void ThenExplainExitsWith(int code) => _explainExit.Should().Be(code, _stderr);

    private string ExplainPage
    {
        get
        {
            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath)))!.AsObject();
            var pin = manifest["schemaVersion"]?.GetValue<string>() ?? "0.1.0";
            return EngineServices.Versions().Versions.Single(version => version.Number.ToString() == pin).RulePages[_explainedId];
        }
    }

    private void WriteExplainManifest(string text) => File.WriteAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath), text);

    private int _explainExit;
    private string _explainedId = string.Empty;
}
