using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Tool.Features;
using Specht.Versioning;
using Spectre.Console.Cli.Testing;

namespace Specht.Tests.Upgrade;

/// <summary>
/// <c>specht upgrade</c> through Spectre's command tester under a floating pin policy (<c>0001-F7</c> B-017 as amended,
/// B-146, C-18): over a set and shipped files holding the embedded <c>0.3.0</c> and a <c>0.3.1</c> copied from it with every
/// file changed, a root holding <c>0.3.0</c>'s files and pinning <c>0.3.0</c> with <c>latest-patch</c> exits <c>0</c>, and
/// its version line and one line per rewritten file name the version the check uses, <c>0.3.1</c>, and never the pin.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class UpgradeCommandPinPolicyIntegrationTests
{
    [Fact]
    public void ARootPinnedWithLatestPatchBesideAHigherPatch_WhenUpgraded_ShouldExitZeroAndNameTheVersionInUseAndEachFileRewritten()
    {
        // Given
        var pin = new SemanticVersion(0, 3, 0);
        var inUse = new SemanticVersion(0, 3, 1);
        var root = Path.GetFullPath("repo");
        ShippedFiles shipped = new ShippedFilesFixture().WithVersionCopied(pin, inUse);
        var embedded = EngineServices.Versions();
        var versions = new SchemaVersions([.. embedded.Versions, embedded.Find(pin)! with { Number = inUse }]);
        var fileSystem = new MockFileSystem();
        foreach (var file in shipped.At(pin))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes));
        }

        var manifestPath = Path.Combine(root, ".specht/manifest.json");
        var manifest = JsonNode.Parse(fileSystem.File.ReadAllText(manifestPath))!.AsObject();
        manifest["pinPolicy"] = "latest-patch";
        fileSystem.File.WriteAllText(manifestPath, manifest.ToJsonString());
        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem).WithVersions(versions).WithShipped(shipped);

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        var rewritten = shipped.At(inUse)
            .Where(static file => file.Relative != ".specht/manifest.json")
            .Select(static file => $"rewritten {file.Relative}");
        result.Output.Split('\n').Should().Equal([".specht/ rewritten to schemaVersion 0.3.1", .. rewritten]);
    }
}
