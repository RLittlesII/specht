using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Versioning;

/// <summary>Explicit schema selection and controlled failures (0001-F7 B-009 to B-011, B-036, B-101 to B-107).</summary>
public sealed partial class VersioningSteps
{
    [Given("the manifest pins schema version {string}")]
    [Given("a valid manifest pinning schema version {string}")]
    public void GivenTheManifestPinsSchemaVersion(string version) => WriteSchemaVersion(JsonValue.Create(version));

    [Given("the manifest records no upstream schema source")]
    [Given("its manifest records no upstream schema source")]
    public void GivenNoUpstreamSource() => EditManifest(static manifest => manifest.Remove("spechtSchema"));

    [Given("the manifest selects the disk schema source")]
    public void GivenManifestDiskSource() => GivenManifestSource("disk");

    [Given("the manifest explicitly selects the embedded source")]
    public void GivenManifestEmbeddedSource() => GivenManifestSource("embedded");

    [Given("the manifest selects the {string} schema source")]
    public void GivenManifestSource(string source) => EditManifest(manifest => manifest["schemaSource"] = source);

    [Given("the on-disk source is selected")]
    public void GivenDiskSource() => _selection = "disk";

    [Given("the root's on-disk Feature schema has been edited to require a key the embedded one does not")]
    [Given("the local Feature schema requires a key absent from the specification")]
    [Given("the local and embedded Feature schemas require different keys")]
    public void GivenLocalFeatureRequiresAKey()
    {
        Tree.WriteRaw(SpecTree.FeatureSchemaPath, "{ \"required\": [\"local_key\"] }");
        Tree.WriteFeature("0001", "F1");
    }

    [Given("the root holds a specification without that key")]
    public void GivenSpecificationWithoutLocalKey() => Tree.WriteFeature("0001", "F1");

    [Given("the root holds the Feature and task schemas and no epic schema")]
    public void GivenNoEpicSchema() => File.Delete(Path.Combine(Tree.Root, SpecTree.EpicSchemaPath));

    [Given("the root holds a matching local copy of that source")]
    public void GivenMatchingSourceCopy() => Tree.WriteFeature("0001", "F1");

    [Given("a local schema differs from its recorded hash")]
    public void GivenAnEditedSourceCopy() => File.AppendAllText(Path.Combine(Tree.Root, SpecTree.FeatureSchemaPath), "\n");

    [Given("a required named local schema file is absent")]
    public void GivenMissingNamedSchema() => File.Delete(Path.Combine(Tree.Root, SpecTree.ItemSchemaPath));

    [Given("a required named local schema cannot be compiled as a JSON Schema")]
    public void GivenInvalidNamedSchema() => Tree.WriteRaw(SpecTree.ItemSchemaPath, "{ \"type\": 42 }");

    [Given("a required named local schema has {string}")]
    public void GivenNamedSchemaFault(string fault)
    {
        _failedSchema = SpecTree.FeatureSchemaPath;
        Tree.WriteRaw(_failedSchema, fault switch
        {
            "malformed JSON" => "{",
            "an unresolved local reference" => "{ \"$ref\": \"#/missing\" }",
            _ => "{ \"type\": 42 }",
        });
        Tree.WriteFeature("0001", "F1");
    }

    [When("the check runs with the on-disk source selected")]
    public void WhenSelectedDiskCheckRuns() => WhenCommandLineSourceRuns("disk");

    [When("the check runs without selecting a source")]
    [When("the check runs without a command-line schema source")]
    public void WhenUnselectedCheckRuns()
    {
        _selection = null;
        WhenTheCheckRuns();
    }

    [When("the check runs with command-line schema source {string}")]
    [When("the check runs with command-line schema source {string} and JSON output")]
    public void WhenCommandLineSourceRuns(string source)
    {
        _selection = source;
        WhenTheCheckRuns();
    }

    [Then("the specification is reported for the missing key")]
    public void ThenLocalKeyIsRequired() => Violations.Should().Contain(static violation =>
        violation["file"]!.GetValue<string>() == FeaturePath
        && FrontmatterRules.Contains(violation["ruleId"]!.GetValue<string>())
        && violation["message"]!.GetValue<string>().Contains("local_key", StringComparison.Ordinal));

    [Then("no violation is reported for that key")]
    public void ThenLocalKeyIsNotRequired() => Violations.Should().NotContain(static violation =>
        violation["message"]!.GetValue<string>().Contains("local_key", StringComparison.Ordinal));

    [Then("the document says the schemas came from disk")]
    public void ThenDiskSourceIsReported() => Document["schemaSource"]!.GetValue<string>().Should().Be("disk");

    [Then("the document says the schemas came from the upstream source")]
    public void ThenUpstreamSourceIsReported() => ThenTheDocumentSaysTheSchemasCameFromTheUpstreamCopy();

    [Then("the verdict uses the {string} Feature schema")]
    public void ThenSelectedVerdict(string source)
    {
        Document["schemaSource"]!.GetValue<string>().Should().Be(source);
        if (source == "disk")
        {
            ThenLocalKeyIsRequired();
        }
        else
        {
            ThenLocalKeyIsNotRequired();
        }
    }

    [Then("the check completes using the Feature and task schemas")]
    public void ThenCheckCompletesWithoutEpicSchema()
    {
        ThenTheCheckCompletes();
        ThenDiskSourceIsReported();
    }

    [Then("the pinned version remains {string}")]
    public void ThenPinIsUnchanged(string version) =>
        JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath)))!["schemaVersion"]!
            .GetValue<string>().Should().Be(version);

    [Then("the manifest is rejected for the source selection key")]
    public void ThenSourceKeyIsRejected()
    {
        ThenTheManifestIsRejected();
        _stderr.Should().Contain("schemaSource");
    }

    [Then("the standard error names the contradiction")]
    public void ThenContradictionIsNamed() => _stderr.Should().Contain("embedded").And.Contain("spechtSchema");

    [Then("the standard error names that schema by its root-relative path")]
    public void ThenRelativeSchemaIsNamed() => _stderr.Should().Contain(_failedSchema).And.NotContain(Tree.Root);

    private string? _selection;
    private string _failedSchema = SpecTree.ItemSchemaPath;
}
