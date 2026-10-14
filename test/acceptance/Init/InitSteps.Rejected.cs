using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Init;

public sealed partial class InitSteps
{
    [Given("a root directory holding a manifest that is not well-formed JSON")]
    public void GivenAManifestThatIsNotWellFormedJson() =>
        WriteRejectedManifest("[", "does not parse into the manifest's shape");

    [Given("a root directory holding a manifest carrying a key the tool does not know")]
    public void GivenAManifestCarryingAnUnknownKey() =>
        WriteRejectedManifest("""{"unknownSetting":true}""", "does not know the key 'unknownSetting'");

    [Given("a root directory whose manifest records an upstream schema source and explicitly selects the embedded source")]
    public void GivenAManifestWithAContradictorySchemaSource()
    {
        GivenARootWhoseManifestRecordsAnUpstreamSchemaSourceWithItsVersionAndContentHash();
        var manifest = JsonNode.Parse(File.ReadAllText(Written(Manifest)))!.AsObject();
        manifest["schemaVersion"] = "0.2.0";
        manifest["schemaSource"] = "embedded";
        File.WriteAllText(Written(Manifest), manifest.ToJsonString());
        _rejectionReason = "schemaSource 'embedded' contradicts the recorded spechtSchema source";
    }

    [Then("the standard error names the reason")]
    [Then("the standard error names the contradiction")]
    public void ThenTheStandardErrorNamesTheManifestRejection() =>
        _stderr.Should().Contain(".specht/manifest.json").And.Contain(_rejectionReason).And.NotContain(Sandbox);

    [Then("the rejected init has no standard output")]
    public void ThenTheRejectedInitHasNoStandardOutput()
    {
        _stdout.Should().BeEmpty();
        if (_sourceHandler is not null)
        {
            Source.Requests.Should().BeEmpty("a rejected manifest is refused before fetching schemas");
        }
    }

    private void WriteRejectedManifest(string text, string reason)
    {
        Prepare();
        Directory.CreateDirectory(Path.GetDirectoryName(Written(Manifest))!);
        File.WriteAllText(Written(Manifest), text);
        _rejectionReason = reason;
    }

    private string _rejectionReason = string.Empty;
}
