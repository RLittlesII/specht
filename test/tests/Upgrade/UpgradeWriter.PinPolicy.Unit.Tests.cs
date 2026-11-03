using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Tool.Features;
using Specht.Tool.Features.Init;
using Specht.Tool.Features.Upgrade;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// The rewrite beneath <c>specht upgrade</c> under a floating pin policy (<c>0001-F7</c> B-146, C-18): over a set and
/// shipped files holding the embedded <c>0.3.0</c> and a <c>0.3.1</c> copied from it with every file changed, a root holding
/// <c>0.3.0</c>'s files and pinning <c>0.3.0</c> with <c>latest-patch</c>, the embedded source and no upstream schema source,
/// has its frontmatter schemas and templates brought to <c>0.3.1</c>'s bytes. Under OQ-71 (a) the manifest keys added are
/// the pin's (B-016, C-18): where <c>0.3.1</c>'s shipped manifest defines a top-level key <c>0.3.0</c>'s does not, the
/// upgrade adds a key the pin defines and the root lacks, and not that one.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class UpgradeWriterPinPolicyUnitTests
{
    [Fact]
    public async Task ARootPinnedWithLatestPatchBesideAHigherPatch_WhenUpgraded_ShouldBringItsSchemasAndTemplatesToTheVersionInUse()
    {
        // Given
        var pin = new SemanticVersion(0, 3, 0);
        var inUse = new SemanticVersion(0, 3, 1);
        ShippedFiles shipped = new ShippedFilesFixture().WithVersionCopied(pin, inUse);
        var embedded = EngineServices.Versions();
        var versions = new SchemaVersions([.. embedded.Versions, embedded.Find(pin)! with { Number = inUse }]);
        var fileSystem = new MockFileSystem();
        foreach (var file in shipped.At(pin))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes));
        }

        var manifest = JsonNode.Parse(fileSystem.File.ReadAllText("repo/.specht/manifest.json"))!.AsObject();
        manifest["pinPolicy"] = "latest-patch";
        fileSystem.File.WriteAllText("repo/.specht/manifest.json", manifest.ToJsonString());
        UpgradeWriter writer = new UpgradeWriterFixture().WithFileSystem(fileSystem).WithVersions(versions).WithShipped(shipped);

        // When
        var result = await writer.Upgrade("repo", CancellationToken.None);

        // Then
        var expected = shipped.At(inUse).Where(static file => file.Relative != ".specht/manifest.json").ToList();
        expected.Should().NotBeEmpty();
        foreach (var file in expected)
        {
            fileSystem.File.ReadAllBytes("repo/" + file.Relative).Should().Equal(file.Bytes, file.Relative);
        }

        result.Rewritten.Should().Equal(expected.Select(static file => file.Relative));
    }

    [Fact]
    public async Task ARootPinnedWithLatestPatchBesideAPatchDefiningAnotherKey_WhenUpgraded_ShouldAddOnlyTheKeysThePinDefines()
    {
        // Given
        var shippingCopy = new Dictionary<string, byte[]>(InitWriter.ShippingCopy(typeof(ShippedFiles).Assembly), StringComparer.Ordinal);
        foreach (var (name, bytes) in shippingCopy.Where(static entry => entry.Key.Contains("/v0.3.0/", StringComparison.Ordinal)).ToList())
        {
            shippingCopy[name.Replace("/v0.3.0/", "/v0.3.1/", StringComparison.Ordinal)] = bytes;
        }

        var inUseManifest = JsonNode.Parse(shippingCopy["manifest/v0.3.0/manifest.json"])!.AsObject();
        inUseManifest["schemaVersion"] = "0.3.1";
        inUseManifest["inUseOnlyKey"] = true;
        shippingCopy["manifest/v0.3.1/manifest.json"] = Encoding.UTF8.GetBytes(inUseManifest.ToJsonString());
        ShippedFiles shipped = new ShippedFilesFixture().WithShippingCopy(shippingCopy);
        var embedded = EngineServices.Versions();
        var versions = new SchemaVersions([.. embedded.Versions, embedded.Find(new SemanticVersion(0, 3, 0))! with { Number = new SemanticVersion(0, 3, 1) }]);
        var fileSystem = new MockFileSystem();
        var root = JsonNode.Parse(shippingCopy["manifest/v0.3.0/manifest.json"])!.AsObject();
        root.Remove("companionFiles").Should().BeTrue();
        root["pinPolicy"] = "latest-patch";
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(root.ToJsonString()));
        UpgradeWriter writer = new UpgradeWriterFixture().WithFileSystem(fileSystem).WithVersions(versions).WithShipped(shipped);

        // When
        await writer.Upgrade("repo", CancellationToken.None);

        // Then
        var after = JsonNode.Parse(fileSystem.File.ReadAllText("repo/.specht/manifest.json"))!.AsObject();
        after.ContainsKey("companionFiles").Should().BeTrue();
        after.ContainsKey("inUseOnlyKey").Should().BeFalse();
    }
}
