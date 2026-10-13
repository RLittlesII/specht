using System.IO.Abstractions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using Reqnroll;
using Specht.Build;

namespace Specht.Acceptance.Versioning;

public sealed partial class VersioningSteps
{
    [Given("the version lock records version {word} only")]
    public void GivenTheVersionLockRecordsVersionOnly(string version)
    {
        PrepareLockTree(Shipped.Keys.Select(static number => number.ToString()));
        WriteLock(new Dictionary<string, string> { [version] = "previous-digest" });
    }

    [Given("the tool ships schema versions 0.1.0 and 0.1.1 and the version lock records both")]
    public void GivenBothShippedVersionsAreLocked()
    {
        PrepareLockTree(["0.1.0", "0.1.1"]);
        WriteLock(new Dictionary<string, string> { ["0.1.0"] = "earlier-digest", ["0.1.1"] = "newest-digest" });
    }

    [Given("the tool ships schema version {word} and the version lock records it")]
    public void GivenTheShippedVersionIsLocked(string version)
    {
        PrepareLockTree([version]);
        WriteLock(new Dictionary<string, string> { [version] = "original-digest" });
    }

    [Given("a template of version {word} has been edited since")]
    public void GivenATemplateHasBeenEditedSince(string version) =>
        File.WriteAllText(Path.Combine(_lockRoot!, $"src/tool/templates/v{version}/feature.md"), "changed template");

    [When("the newest version is locked")]
    public Task WhenTheNewestVersionIsLocked() => new SchemaVersionLock(new FileSystem(), _lockRoot!).LockNewestAsync();

    [Then("the version lock records version {word} with the digest of its files")]
    public async Task ThenTheVersionLockRecordsTheDigest(string version)
    {
        var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(await File.ReadAllTextAsync(LockPath))!;
        entries.Should().HaveCount(2);
        entries["0.1.0"].Should().Be("previous-digest");
        entries[version].Should().Be(await new SchemaVersionLock(new FileSystem(), _lockRoot!).ComputeDigestAsync(version));
    }

    [Then("the version lock is as it was")]
    [Then("the version lock still records the digest version 0.1.0 was locked with")]
    public async Task ThenTheVersionLockIsUnchanged() =>
        (await File.ReadAllBytesAsync(LockPath)).Should().Equal(_lockBefore);

    [AfterScenario]
    public void DeleteLockTree()
    {
        if (_lockRoot is not null)
        {
            Directory.Delete(_lockRoot, recursive: true);
        }
    }

    private string LockPath => Path.Combine(_lockRoot!, SchemaVersionLock.RelativePath);

    private void PrepareLockTree(IEnumerable<string> versions)
    {
        _lockRoot = Path.Combine(".artifacts", "schema-lock-tests", Guid.NewGuid().ToString("N"));
        foreach (var version in versions)
        {
            foreach (var path in new[]
                     {
                         $"src/tool/schema/v{version}/frontmatter/feature.schema.json",
                         $"src/tool/manifest/v{version}/manifest.json",
                         $"src/tool/templates/v{version}/feature.md",
                         $"docs/rules/v{version}/SPEC001.md",
                     })
            {
                var fullPath = Path.Combine(_lockRoot, path);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                File.WriteAllText(fullPath, $"synthetic {path}\n");
            }
        }
    }

    private void WriteLock(Dictionary<string, string> entries)
    {
        _lockBefore = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entries));
        File.WriteAllBytes(LockPath, _lockBefore);
    }

    private string? _lockRoot;
    private byte[] _lockBefore = [];
}
