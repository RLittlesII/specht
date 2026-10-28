using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Versioning;

public sealed partial class VersioningSteps
{
    [Given("the source's record holds a content hash for the feature template")]
    public void GivenTheSourcesRecordHoldsAContentHashForTheFeatureTemplate() =>
        EditManifest(static manifest => manifest["spechtSchema"]!["templates"] = new JsonObject { ["feature.md"] = TemplateHash });

    [Given(
        @"^the manifest records an upstream schema source whose content hash for the feature template is "
            + "(written without its algorithm|written in capital digits|shorter than the algorithm's|another algorithm's)$")]
    public void GivenTheManifestRecordsAnUpstreamSchemaSourceWhoseContentHashForTheFeatureTemplateIs(string form) =>
        WriteUpstreamSchemaSource(source => source["templates"] = new JsonObject
        {
            ["feature.md"] = form switch
            {
                "written without its algorithm" => new string('d', 64),
                "written in capital digits" => "sha256:" + new string('D', 64),
                "shorter than the algorithm's" => "sha256:" + new string('d', 63),
                _ => "sha512:" + new string('d', 128),
            },
        });

    [Given("the manifest records an upstream schema source with a content hash for a runbook template the pinned version does not ship")]
    public void GivenTheManifestRecordsAnUpstreamSchemaSourceWithAContentHashForARunbookTemplate() =>
        WriteUpstreamSchemaSource(static source => source["templates"] = new JsonObject { ["runbook.md"] = TemplateHash });

    [Then("the manifest is accepted")]
    public void ThenTheManifestIsAccepted() => _exitCode.Should().BeOneOf([0, 1], _stderr);

    [Then("the feature template is one the source supplies")]
    public void ThenTheFeatureTemplateIsOneTheSourceSupplies() =>
        EngineServices.Parser().Parse(File.ReadAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath)))
            .SpechtSchema!.Templates.Should().ContainSingle()
            .Which.Should().Be(new UpstreamTemplateFile(
                "feature.md", "https://schemas.example/specht/feature.md", TemplateHash));

    private static readonly string TemplateHash = "sha256:" + new string('d', 64);
}
