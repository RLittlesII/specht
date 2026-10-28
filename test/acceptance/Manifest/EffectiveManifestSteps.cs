using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Manifest;

/// <summary>Exercises the manifest command and its preserved effective-document semantics.</summary>
[Binding]
[Scope(Feature = "The manifest command")]
public sealed partial class EffectiveManifestSteps
{
    [Given("a repository root holding a manifest that pins schema version {word}")]
    public void GivenRoot(string pin)
    {
        _tree = new SpecTree();
        var manifest = Manifest;
        manifest["schemaVersion"] = pin;
        Write(manifest);
    }

    [Given("the manifest renames the claims section")]
    public void GivenRenamedClaims()
    {
        var manifest = Manifest;
        var old = manifest["roles"]!["claims"]!.GetValue<string>();
        var sections = manifest["sections"]!.AsArray();
        sections[sections.Select(static node => node!.GetValue<string>()).ToList().IndexOf(old)] = "3. Consumer claims";
        manifest["roles"]!["claims"] = "3. Consumer claims";
        Write(manifest);
    }

    [Given("the manifest leaves out the name of the claims section")]
    public void GivenOmittedClaims()
    {
        var manifest = Manifest;
        manifest["roles"]!.AsObject().Remove("claims");
        Write(manifest);
    }

    [Given("the manifest sets no level for the out-of-order-section rule")]
    public void GivenOmittedLevel() => Manifest["rules"]?.AsObject().ContainsKey("SPEC010").Should().NotBe(true);

    [Given("a repository root holding a manifest that pins no schema version")]
    public void GivenNoPin()
    {
        var manifest = Manifest;
        manifest.Remove("schemaVersion");
        Write(manifest);
    }

    [Given("the root holds specifications with violations")]
    public void GivenViolations()
    {
        Tree.WriteFeature("0001", "F1", sections: SpecTree.SectionsWith("3. Acceptance Criteria", "## 3. Acceptance Criteria\n\nNo claims table.\n"));
        var manifest = Manifest;
        manifest["identifiers"]!.AsObject().Remove("epic");
        Write(manifest);
    }

    [When("the effective manifest is printed as JSON and written as the manifest of a copy of the root")]
    public void WhenWrittenToCopy()
    {
        Print("--json");
        _result.ExitCode.Should().Be(0, _result.Stderr);
        _copy = new SpecTree();
        foreach (var file in Directory.EnumerateFiles(Tree.Root, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(_copy.Root, Path.GetRelativePath(Tree.Root, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }

        File.WriteAllText(Path.Combine(_copy.Root, SpecTree.ManifestPath), _result.Stdout);
    }

    [Then("the claims section carries the name the manifest gives it")]
    public void ThenDeclaredName() => _document!["roles"]!["claims"]!.GetValue<string>().Should().Be("3. Consumer claims");

    [Then("the claims section carries the name the default manifest of version {word} gives it")]
    public void ThenDefaultName(string pin)
    {
        var defaults = JsonNode.Parse(EngineServices.Versions().Versions.Single(version => version.Number.ToString() == pin).Manifest)!;
        _document!["roles"]!["claims"]!.GetValue<string>().Should().Be(defaults["roles"]!["claims"]!.GetValue<string>());
    }

    [Then("the out-of-order-section rule carries the level of its own severity")]
    public void ThenRuleSeverity() => _document!["rules"]!["SPEC010"]!.GetValue<string>().Should().Be("error");

    [Then("the schema version printed is {word}")]
    public void ThenVersion(string pin) => _document!["schemaVersion"]!.GetValue<string>().Should().Be(pin);

    [Then("the check reports for the copy what it reports for the root")]
    public void ThenEquivalentReports()
    {
        var original = Tree.Run();
        original.Violations.Should().NotBeEmpty();
        var copy = _copy!.Run();
        SpecReportDocument.From(copy).ToJson().Should().Be(SpecReportDocument.From(original).ToJson());
        copy.SpecificationCount.Should().Be(original.SpecificationCount);
        _document!["identifiers"]!.AsObject().Should().NotContainKey("epic");
    }

    [AfterScenario]
    public void DisposeTrees()
    {
        _tree?.Dispose();
        _copy?.Dispose();
    }

    private SpecTree Tree => _tree ?? throw new InvalidOperationException("No synthetic root was prepared.");

    private JsonObject Manifest => JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath)))!.AsObject();

    private void Write(JsonObject manifest) => File.WriteAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath), manifest.ToJsonString());

    private SpecTree? _tree;
    private SpecTree? _copy;
    private JsonObject? _document;
}
