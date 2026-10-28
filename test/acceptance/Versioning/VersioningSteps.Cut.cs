using System.IO.Abstractions;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using Reqnroll;
using Specht.Build;

namespace Specht.Acceptance.Versioning;

public sealed partial class VersioningSteps
{
    [Given("the tool ships schema version {word} and the version lock does not record it")]
    public void GivenTheShippedVersionIsUnlocked(string version)
    {
        PrepareLockTree([version]);
        WriteLock(new Dictionary<string, string>());
    }

    [When("version {word} is cut")]
    public async Task WhenVersionIsCut(string version)
    {
        _cutVersion = version;
        _cutBefore = ReadCutTree();
        _cutError = await Record.ExceptionAsync(() => new SchemaVersionCut(new FileSystem(), _lockRoot!).CutAsync(version));
    }

    [Then("the cut fails")]
    public void ThenTheCutFails() => _cutError.Should().BeOfType<InvalidOperationException>();

    [Then("no file is created")]
    public void ThenNoFileIsCreated() => ReadCutTree().Keys.Except(_cutBefore.Keys).Should().BeEmpty();

    [Then("a version {word} folder stands beside each folder of version {word}")]
    public void ThenAFolderStandsBesideEachFolderOfTheNewest(string cut, string newest)
    {
        RethrowCutError();
        var trees = TreesHolding(newest);
        trees.Should().NotBeEmpty();
        trees.Should().AllSatisfy(tree => Directory.Exists(Path.Combine(_lockRoot!, tree, $"v{cut}")).Should().BeTrue(tree));
    }

    [Then("each holds a file for every file of version {word}'s folder and no other")]
    public void ThenEachHoldsAFileForEveryFileOfTheNewest(string newest)
    {
        RethrowCutError();
        foreach (var tree in TreesHolding(newest))
        {
            FilesUnder($"{tree}/v{_cutVersion}").Should().BeEquivalentTo(FilesUnder($"{tree}/v{newest}"), tree);
        }
    }

    [Then("each frontmatter schema of version {word} declares the address of version {word}")]
    public void ThenEachFrontmatterSchemaDeclaresTheAddress(string cut, string addressed)
    {
        RethrowCutError();
        var schemas = FilesUnder($"src/tool/schema/v{cut}/frontmatter").Where(static name => name.EndsWith(".schema.json", StringComparison.Ordinal)).ToArray();
        schemas.Should().NotBeEmpty();
        foreach (var name in schemas)
        {
            using var schema = JsonDocument.Parse(File.ReadAllText(Path.Combine(_lockRoot!, $"src/tool/schema/v{cut}/frontmatter", name)));
            schema.RootElement.GetProperty("$id").GetString()
                .Should().Be($"https://github.com/rlittlesii/specht/schema/v{addressed}/frontmatter/{name}");
        }
    }

    [Then("each rule page of version {word} gives schema version {word}")]
    public void ThenEachRulePageGivesTheSchemaVersion(string cut, string given)
    {
        RethrowCutError();
        var pages = Directory.GetFiles(Path.Combine(_lockRoot!, $"docs/rules/v{cut}"), "*.md");
        pages.Should().NotBeEmpty();
        foreach (var page in pages)
        {
            File.ReadAllLines(page)
                .Where(static line => line.StartsWith("| Schema version ", StringComparison.Ordinal))
                .Select(static line => line.Split('|')[2].Trim())
                .Should().Equal(given);
        }
    }

    [Then("each file of version {word} equals version {word}'s file of the same name, the stated version aside")]
    public void ThenEachFileEqualsTheNewestsFileBesideTheStatedVersion(string cut, string newest)
    {
        RethrowCutError();
        foreach (var tree in TreesHolding(newest))
        {
            foreach (var name in FilesUnder($"{tree}/v{newest}"))
            {
                var source = File.ReadAllText(Path.Combine(_lockRoot!, tree, $"v{newest}", name));
                var stated = tree switch
                {
                    "src/tool/schema" => source.Replace(
                        $"/schema/v{newest}/frontmatter/", $"/schema/v{cut}/frontmatter/", StringComparison.Ordinal),
                    "docs/rules" => source.Replace(
                        $"| Schema version   | {newest,-11} |", $"| Schema version   | {cut,-11} |", StringComparison.Ordinal),
                    _ => source,
                };
                File.ReadAllBytes(Path.Combine(_lockRoot!, tree, $"v{cut}", name)).Should().Equal(Encoding.UTF8.GetBytes(stated), $"{tree}/{name}");
            }
        }
    }

    [Then("each template of version {word} is byte-identical to version {word}'s")]
    public void ThenEachTemplateIsByteIdenticalToTheNewests(string cut, string newest)
    {
        RethrowCutError();
        var templates = FilesUnder($"src/tool/templates/v{newest}");
        templates.Should().NotBeEmpty();
        foreach (var name in templates)
        {
            File.ReadAllBytes(Path.Combine(_lockRoot!, $"src/tool/templates/v{cut}", name))
                .Should().Equal(File.ReadAllBytes(Path.Combine(_lockRoot!, $"src/tool/templates/v{newest}", name)), name);
        }
    }

    [Then("no file outside version {word}'s folders is created, changed or deleted")]
    public void ThenNoFileOutsideTheCutFoldersChanged(string cut)
    {
        RethrowCutError();
        var folders = CutTrees.Select(tree => $"{tree}/v{cut}/").ToArray();
        ReadCutTree()
            .Where(file => !folders.Any(folder => file.Key.StartsWith(folder, StringComparison.Ordinal)))
            .ToDictionary(static file => file.Key, static file => file.Value)
            .Should().BeEquivalentTo(_cutBefore);
    }

    [Then("the version lock does not record version {word}")]
    public void ThenTheVersionLockDoesNotRecord(string version)
    {
        RethrowCutError();
        JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(LockPath))!.Should().NotContainKey(version);
    }

    private void RethrowCutError()
    {
        if (_cutError is not null)
        {
            ExceptionDispatchInfo.Throw(_cutError);
        }
    }

    private string[] TreesHolding(string version) =>
        CutTrees.Where(tree => Directory.Exists(Path.Combine(_lockRoot!, tree, $"v{version}"))).ToArray();

    private string[] FilesUnder(string folder)
    {
        var fullPath = Path.Combine(_lockRoot!, folder);
        return Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(fullPath, path).Replace('\\', '/'))
            .ToArray();
    }

    private Dictionary<string, byte[]> ReadCutTree() => Directory
        .GetFiles(_lockRoot!, "*", SearchOption.AllDirectories)
        .ToDictionary(path => Path.GetRelativePath(_lockRoot!, path).Replace('\\', '/'), File.ReadAllBytes);

    private static readonly string[] CutTrees = ["src/tool/schema", "src/tool/manifest", "src/tool/templates", "docs/rules"];

    private string? _cutVersion;
    private Exception? _cutError;
    private Dictionary<string, byte[]> _cutBefore = new();
}
