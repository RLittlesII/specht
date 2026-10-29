using System.Net;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tool;

namespace Specht.Acceptance.Init;

/// <summary>
/// Steps for the templates an upstream schema source hashes (0001-F4 B-041 to B-044). The manifest is the one the schema
/// source steps write, with <c>spechtSchema.templates</c> holding the hash of synthetic feature-template bytes; the
/// synthetic handler serves those bytes at the source's location followed by <c>feature.md</c>.
/// </summary>
public sealed partial class InitSteps
{
    [Given("a root whose manifest records an upstream schema source whose record holds a content hash for the feature template")]
    public void GivenARootWhoseManifestRecordsAnUpstreamSchemaSourceWhoseRecordHoldsAContentHashForTheFeatureTemplate()
    {
        GivenARootWhoseManifestRecordsAnUpstreamSchemaSourceWithItsVersionAndContentHash();
        var manifest = JsonNode.Parse(_manifest!)!.AsObject();
        manifest["spechtSchema"]!["templates"] = new JsonObject
        {
            ["feature.md"] = "sha256:" + Convert.ToHexStringLower(SHA256.HashData(FetchedTemplateContent)),
        };
        _manifest = System.Text.Encoding.UTF8.GetBytes(manifest.ToJsonString());
        File.WriteAllBytes(Written(Manifest), _manifest);
    }

    [Given("its manifest names no template as the consumer's")]
    public void GivenItsManifestNamesNoTemplateAsTheConsumers() =>
        JsonNode.Parse(_manifest!)!.AsObject().ContainsKey("consumerTemplates").Should().BeFalse();

    [Given("no template file under the root")]
    public void GivenNoTemplateFileUnderTheRoot() =>
        Directory.Exists(Path.GetDirectoryName(Written(FeatureTemplate))).Should().BeFalse();

    [Given("the source serves a feature template matching that hash")]
    public void GivenTheSourceServesAFeatureTemplateMatchingThatHash()
    {
        Source.Mode = "matching";
        Source.Served[FeatureTemplateAddress] = FetchedTemplateContent;
    }

    [Given("the source serves a feature template that does not match that hash")]
    public void GivenTheSourceServesAFeatureTemplateThatDoesNotMatchThatHash() => Source.Mode = "mismatch";

    [Given("the source asks for a credential")]
    public void GivenTheSourceAsksForACredential() => Source.Status = HttpStatusCode.Unauthorized;

    [Given("the source does not serve the feature template")]
    public void GivenTheSourceDoesNotServeTheFeatureTemplate() => Source.Status = HttpStatusCode.NotFound;

    [Given("the feature template is present under the root")]
    public void GivenTheFeatureTemplateIsPresentUnderTheRoot()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Written(FeatureTemplate))!);
        File.WriteAllBytes(Written(FeatureTemplate), LocalTemplateContent);
    }

    [Then("the root's feature template is the one the source served")]
    public void ThenTheRootsFeatureTemplateIsTheOneTheSourceServed()
    {
        _exitCode.Should().Be(ExitCodes.Success, _stderr);
        Source.Requests.Should().Contain(FeatureTemplateAddress);
        File.ReadAllBytes(Written(FeatureTemplate)).Should().Equal(FetchedTemplateContent);
    }

    [Then("the feature template is unchanged")]
    public void ThenTheFeatureTemplateIsUnchanged() =>
        File.ReadAllBytes(Written(FeatureTemplate)).Should().Equal(LocalTemplateContent);

    private const string FeatureTemplateAddress = SourceLocation + "feature.md";

    private static readonly byte[] FetchedTemplateContent = [0xEF, 0xBB, 0xBF, 0x23, 0x20, 0x46, 0x0D, 0x0A];
    private static readonly byte[] LocalTemplateContent = "# A local feature template\n"u8.ToArray();
}
