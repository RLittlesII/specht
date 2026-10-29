using System.Security.Cryptography;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Tool.Features.Init;

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

    [Given("the manifest records an upstream schema source whose record holds a content hash for the feature template")]
    public void GivenTheManifestRecordsAnUpstreamSchemaSourceWhoseRecordHoldsAContentHashForTheFeatureTemplate() =>
        WriteUpstreamSchemaSource(static source => source["templates"] = new JsonObject
        {
            ["feature.md"] = "sha256:" + Convert.ToHexStringLower(SHA256.HashData(UpstreamTemplate)),
        });

    [Given("the root's local copy of the feature template does not match that hash")]
    public void GivenTheRootsLocalCopyOfTheFeatureTemplateDoesNotMatchThatHash() => WriteFeatureTemplate(LocalTemplate);

    [Given("the source serves a feature template matching that hash")]
    public void GivenTheSourceServesAFeatureTemplateMatchingThatHash() => Upstream.Served["feature.md"] = UpstreamTemplate;

    [Given("the source serves a feature template that does not match that hash")]
    public void GivenTheSourceServesAFeatureTemplateThatDoesNotMatchThatHash() => Upstream.Refused["feature.md"] = "mismatch";

    [Given("the source does not serve the feature template")]
    public void GivenTheSourceDoesNotServeTheFeatureTemplate() => Upstream.Refused["feature.md"] = "404";

    [Given("the root's local copy of each schema and of the feature template matches its hash")]
    public void GivenTheRootsLocalCopyOfEachSchemaAndOfTheFeatureTemplateMatchesItsHash()
    {
        GivenTheRootsLocalCopyMatchesThatHash();
        WriteFeatureTemplate(UpstreamTemplate);
    }

    [Given("the root's feature template matches that hash and differs from the pinned version's")]
    public void GivenTheRootsFeatureTemplateMatchesThatHashAndDiffersFromThePinnedVersions()
    {
        var pinned = ManifestAfter()["schemaVersion"]!.GetValue<string>();
        InitWriter.ShippingCopy(typeof(InitWriter).Assembly)[$"templates/v{pinned}/feature.md"].Should().NotEqual(UpstreamTemplate);
        WriteFeatureTemplate(UpstreamTemplate);
    }

    [Then("the root's feature template is the one the source served")]
    public void ThenTheRootsFeatureTemplateIsTheOneTheSourceServed()
    {
        _exitCode.Should().Be(0, _stderr);
        File.ReadAllBytes(Path.Combine(Tree.Root, FeatureTemplatePath)).Should().Equal(Upstream.Served["feature.md"]);
    }

    [Then("the feature template was asked for at the source's address followed by its file name")]
    public void ThenTheFeatureTemplateWasAskedForAtTheSourcesAddressFollowedByItsFileName() =>
        Upstream.Requests.Select(static request => request.Address.AbsoluteUri).Should().Contain(SourceAddress + "feature.md", _stderr);

    [Then("the root's feature template is unchanged")]
    public void ThenTheRootsFeatureTemplateIsUnchanged()
    {
        _exitCode.Should().Be(0, _stderr);
        File.ReadAllBytes(Path.Combine(Tree.Root, FeatureTemplatePath)).Should().Equal(_filesBefore![FeatureTemplatePath]);
    }

    private void WriteFeatureTemplate(byte[] bytes)
    {
        var path = Path.Combine(Tree.Root, FeatureTemplatePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
    }

    private const string FeatureTemplatePath = SpecTree.TemplatesFolder + "/feature.md";

    private static readonly string TemplateHash = "sha256:" + new string('d', 64);

    private static readonly byte[] UpstreamTemplate = "# A synthetic upstream feature template\r\n"u8.ToArray();

    private static readonly byte[] LocalTemplate = "# A synthetic local feature template\n"u8.ToArray();
}
