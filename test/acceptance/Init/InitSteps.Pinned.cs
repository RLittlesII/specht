using System.Text;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Acceptance.Init;

public sealed partial class InitSteps
{
    [Given("the tool ships schema versions 0.1.0 and 0.2.0")]
    public void GivenTheToolShipsTheOlderAndNewerVersions()
    {
        Read("manifest/v0.1.0/manifest.json").Should().NotBeNull();
        Read("manifest/v0.2.0/manifest.json").Should().NotBeNull();
    }

    [Given("a root directory holding only a manifest pinning version {word} and declaring the epic grammar")]
    [Given("a root directory holding a manifest pinning version {word}")]
    public void GivenARootHoldingAPinnedManifest(string version)
    {
        Prepare();
        _manifest = Encoding.UTF8.GetBytes($$"""{ "schemaVersion": "{{version}}", "identifiers": { "epic": "^[0-9]{4}$" } }""");
        Directory.CreateDirectory(Path.GetDirectoryName(Written(Manifest))!);
        File.WriteAllBytes(Written(Manifest), _manifest);
    }

    [Given("an initialized root pinned to version 0.1.0 with local edits to a schema and a template")]
    public void GivenAnInitializedRootWithLocalEdits()
    {
        GivenARootHoldingAPinnedManifest("0.1.0");
        var version = new SemanticVersion(0, 1, 0);
        foreach (var file in ToolOwnedFiles)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Written(file))!);
            File.WriteAllBytes(Written(file), Read(Embedded(file, version))!);
        }

        File.WriteAllBytes(Written(FeatureSchema), [0xEF, 0xBB, 0xBF, 0x7B, 0x0D, 0x0A]);
        File.WriteAllText(Written(FeatureTemplate), "# Local template edits\r\n");
    }

    [Given("the other frontmatter schemas have been deleted")]
    public void GivenTheOtherFrontmatterSchemasHaveBeenDeleted()
    {
        File.Delete(Written(ItemSchema));
        File.Delete(Written(EpicSchema));
        _snapshot = Snapshot();
    }

    [Given("a root directory holding only a manifest with local edits")]
    public void GivenARootHoldingAManifestWithLocalEdits()
    {
        GivenARootHoldingAPinnedManifest("0.1.0");
        _manifest = "{\r\n  \"schemaVersion\": \"0.1.0\", \"$note\": \"local edits\"\r\n}\r\n"u8.ToArray();
        File.WriteAllBytes(Written(Manifest), _manifest);
    }

    [Then("the three frontmatter schemas and the four templates are written at version 0.1.0")]
    public void ThenTheSelectedFilesAreWrittenAtTheOlderPin()
    {
        foreach (var file in ToolOwnedFiles)
        {
            File.ReadAllBytes(Written(file)).Should().Equal(Read(Embedded(file, new SemanticVersion(0, 1, 0))));
        }

        ThenTheManifestIsByteForByteAsItWas();
    }

    [Then("the missing frontmatter schemas are restored at version 0.1.0")]
    public void ThenTheDeletedSchemasAreRestoredAtTheOlderPin()
    {
        foreach (var file in new[] { ItemSchema, EpicSchema })
        {
            File.ReadAllBytes(Written(file)).Should().Equal(Read(Embedded(file, new SemanticVersion(0, 1, 0))));
            Listing(file).Should().Equal($"written .specht/{file}");
        }
    }

    [Then("every file that existed before the rerun is byte-for-byte unchanged")]
    public void ThenExistingFilesAreUnchanged()
    {
        var after = Snapshot();
        foreach (var (path, bytes) in _snapshot ?? throw new InvalidOperationException("No snapshot was taken."))
        {
            after[path].Should().Equal(bytes, path);
        }

        Listing(FeatureSchema).Should().Equal($"skipped .specht/{FeatureSchema}");
        Listing(FeatureTemplate).Should().Equal($"skipped .specht/{FeatureTemplate}");
    }

    [Then("the manifest is byte-for-byte as it was")]
    public void ThenTheManifestIsByteForByteAsItWas() => File.ReadAllBytes(Written(Manifest)).Should().Equal(_manifest);

    [Then("the feature template is byte-for-byte as it was")]
    public void ThenTheFeatureTemplateIsByteForByteAsItWas() =>
        File.ReadAllText(Written(FeatureTemplate)).Should().Be("# A synthetic feature template with local edits\n");

    [Then("the standard error names version 7.0.0 and the versions the tool ships")]
    public void ThenTheErrorNamesTheUnshippedPinAndShippedVersions()
    {
        _stdout.Should().BeEmpty();
        _stderr.Should().Contain("7.0.0").And.Contain(".specht/manifest.json").And.NotContain(Sandbox);
        foreach (var version in EngineServices.Versions().Versions)
        {
            _stderr.Should().Contain(version.Number.ToString());
        }
    }
}
