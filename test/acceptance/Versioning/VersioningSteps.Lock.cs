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

    [Given("the tool ships schema versions 0.1.0 and 0.2.0 and the version lock records both")]
    public void GivenBothShippedVersionsAreLocked()
    {
        PrepareLockTree(["0.1.0", "0.2.0"]);
        WriteLock(new Dictionary<string, string> { ["0.1.0"] = "earlier-digest", ["0.2.0"] = "newest-digest" });
    }

    [Given("the tool ships schema version {word} and the version lock records it")]
    public async Task GivenTheShippedVersionIsLocked(string version)
    {
        PrepareLockTree([version]);
        WriteLock(new Dictionary<string, string>
        {
            [version] = await new SchemaVersionLock(new FileSystem(), _lockRoot!).ComputeDigestAsync(version),
        });
    }

    [Given("none of its schemas, templates or rule pages has changed since")]
    public void GivenNoneOfItsFilesHasChangedSince()
    {
    }

    [Given(@"^(a frontmatter schema|the manifest|a template|a rule page) of version (\S+) has been edited since$")]
    public void GivenAFileHasBeenEditedSince(string file, string version) =>
        File.AppendAllText(
            Path.Combine(_lockRoot!, file switch
            {
                "a frontmatter schema" => $"src/tool/schema/v{version}/frontmatter/feature.schema.json",
                "the manifest" => $"src/tool/manifest/v{version}/manifest.json",
                "a template" => $"src/tool/templates/v{version}/feature.md",
                _ => $"docs/rules/v{version}/SPEC001.md",
            }),
            "edited\n");

    [When("the newest version is locked")]
    public Task WhenTheNewestVersionIsLocked() => new SchemaVersionLock(new FileSystem(), _lockRoot!).LockNewestAsync();

    [When("the shipped versions are checked against the version lock")]
    public async Task WhenTheShippedVersionsAreCheckedAgainstTheVersionLock() =>
        _lockFindings = await new SchemaVersionLock(new FileSystem(), _lockRoot!).VerifyAsync();

    [Then("version {word} matches the digest the lock records")]
    public void ThenTheVersionMatchesItsDigest(string version) =>
        _lockFindings.Should().NotContain(new SchemaVersionLockFinding(version, SchemaVersionLockFault.DigestMismatch));

    [Then("version {word} does not match the digest the lock records")]
    public void ThenTheVersionDoesNotMatchItsDigest(string version) =>
        _lockFindings.Should().Contain(new SchemaVersionLockFinding(version, SchemaVersionLockFault.DigestMismatch));

    [Then("the check fails because two versions are unlocked")]
    public void ThenTheCheckFailsBecauseTwoVersionsAreUnlocked() =>
        _lockFindings.Where(static finding => finding.Fault == SchemaVersionLockFault.MoreThanOneUnlocked)
            .Select(static finding => finding.Version)
            .Should().BeEquivalentTo(Unlocked());

    [Then("the check fails because the unlocked version is not the newest")]
    public void ThenTheCheckFailsBecauseTheUnlockedVersionIsNotTheNewest() =>
        _lockFindings.Should().Contain(new SchemaVersionLockFinding(Unlocked().Should().ContainSingle().Subject, SchemaVersionLockFault.UnlockedNotNewest));

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

    private List<string> Unlocked()
    {
        var locked = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(LockPath))!;
        return [.. Shipped.Keys.Select(static number => number.ToString()).Where(number => !locked.ContainsKey(number))];
    }

    private void PrepareLockTree(IEnumerable<string> versions)
    {
        _lockRoot = Path.Combine(".artifacts", "schema-lock-tests", Guid.NewGuid().ToString("N"));
        foreach (var version in versions)
        {
            foreach (var (path, content) in new Dictionary<string, string>
            {
                [$"src/tool/schema/v{version}/frontmatter/feature.schema.json"] =
                    $"{{\n  \"$id\": \"https://github.com/rlittlesii/specht/schema/v{version}/frontmatter/feature.schema.json\",\n"
                    + "  \"type\": \"object\"\n}\n",
                [$"src/tool/manifest/v{version}/manifest.json"] = $"{{\n  \"schemaVersion\": \"{version}\"\n}}\n",
                [$"src/tool/templates/v{version}/feature.md"] = "synthetic template\n",
                [$"docs/rules/v{version}/SPEC001.md"] =
                    "# SPEC001: Synthetic rule\n\n| Property         | Value       |\n| ---------------- | ----------- |\n"
                    + $"| Schema version   | {version,-11} |\n",
            })
            {
                var fullPath = Path.Combine(_lockRoot, path);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                File.WriteAllText(fullPath, content);
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
    private IReadOnlyList<SchemaVersionLockFinding> _lockFindings = [];
}
