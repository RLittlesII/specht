using System.IO.Abstractions;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using Reqnroll;
using Specht.Build;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Acceptance.Versioning;

/// <summary>
/// Steps for <c>src/tool/.spec/versioning.feature</c> (0001-F7) that read this repository's own shipped versions, item
/// 0044: B-004, B-013, B-015, B-040, B-066 and B-088. What a published package shipped is read from the committed version lock,
/// since every published version is locked (A-4); the shipped files are the repository's shipping trees the tool embeds.
/// Item 0388 binds B-066's patch scenarios over a synthetic shipping tree on disk instead: 0.1.0 and 0.1.1, the higher's
/// manifest defining a key the lower's does not; the opt-in premise gives the new rule page an <c>opt_in</c> frontmatter line
/// naming that key, the mark 0001-F7 section 7 defines. The minor scenario's rule premise holds over the repository when
/// some shipped step below 1.0.0 adds a rule page without that mark.
/// </summary>
[Binding]
[Scope(Feature = "Schema versioning")]
public sealed class ShippedVersionSteps(PackedTool tool)
{
    [Given("the schema versions 1.0.0 and higher published in packages so far")]
    public async Task GivenTheVersionsFromOneZeroZeroPublishedSoFar() =>
        _published = [.. (await ReadLockAsync()).Keys.Where(static number => System.Version.Parse(number) >= new System.Version(1, 0, 0))];

    [When("a later tool's embedded versions are enumerated")]
    public void WhenALaterToolsEmbeddedVersionsAreEnumerated() =>
        _embedded = [.. EngineServices.Versions().Versions.Select(static version => version.Number.ToString())];

    [Then("every one of those versions is present")]
    public void ThenEveryOneOfThoseVersionsIsPresent() => _published.Except(_embedded).Should().BeEmpty();

    [Given("the embedded schemas, manifest, templates and rule pages for version {word} as the first published package shipped them")]
    [Given("the rules version {word} held in the first published package")]
    public async Task GivenWhatTheFirstPublishedPackageShipped(string version)
    {
        (await ReadLockAsync()).Should().ContainKey(version);
        _version = version;
    }

    [Given("a later tool that ships version {word}")]
    public void GivenALaterToolThatShipsVersion(string version)
    {
        SemanticVersion.TryParse(version, out var number).Should().BeTrue();
        EngineServices.Versions().Find(number).Should().NotBeNull();
    }

    [When("that tool's embedded files for version {word} are read")]
    [When("that tool's rules for version {word} are enumerated")]
    public async Task WhenThatToolsVersionIsRead(string version)
    {
        version.Should().Be(_version);
        _findings = await new SchemaVersionLock(new FileSystem(), tool.Repository).VerifyAsync();
    }

    [Then("they are byte-identical")]
    public void ThenTheyAreByteIdentical() =>
        _findings.Should().NotContain(new SchemaVersionLockFinding(_version!, SchemaVersionLockFault.DigestMismatch));

    [Then("they are the same rules")]
    public void ThenTheyAreTheSameRules()
    {
        _findings.Should().NotContain(new SchemaVersionLockFinding(_version!, SchemaVersionLockFault.DigestMismatch));
        SemanticVersion.TryParse(_version, out var number).Should().BeTrue();
        EngineServices.Versions().Find(number)!.RuleIds.Should().BeEquivalentTo(
            Directory.GetFiles(Path.Combine(tool.Repository, "docs", "rules", $"v{_version}"), "SPEC*.md")
                .Select(static page => Path.GetFileNameWithoutExtension(page)));
    }

    [Given("two schema versions the tool ships, one higher than the other by version precedence")]
    [Given("two neighbouring schema versions the tool ships")]
    public void GivenTwoShippedVersions() =>
        Directory.GetDirectories(Path.Combine(tool.Repository, "src", "tool", "schema"), "v*").Should().HaveCountGreaterThanOrEqualTo(2);

    [Given("the lower one is version 1.0.0 or higher")]
    [Given("both are below version 1.0.0")]
    [Given("the higher one holds a rule the lower one does not")]
    public void GivenThePairsPremise()
    {
    }

    [Given("^the two differ only in (a template|the text of a rule page), the version each states aside$")]
    public void GivenTheTwoDifferOnlyIn(string what)
    {
    }

    [Given("two schema versions the tool ships, both below version 1.0.0, the higher one greater in its patch alone")]
    public void GivenTwoShippedVersionsBelowOneZeroZeroDifferingInTheirPatchAlone()
    {
        _numberingRoot = Path.Combine(".artifacts", "schema-numbering-tests", Guid.NewGuid().ToString("N"));
        foreach (var version in new[] { PatchLower, PatchHigher })
        {
            Write($"src/tool/schema/v{version}/frontmatter/feature.schema.json",
                $"{{\n  \"$id\": \"https://github.com/rlittlesii/specht/schema/v{version}/frontmatter/feature.schema.json\"\n}}\n");
            Write($"src/tool/templates/v{version}/feature.md", "synthetic template\n");
            Write($"docs/rules/v{version}/SPEC001.md", $"# SPEC001: Synthetic rule\n\n| Schema version | {version} |\n");
        }

        Write($"src/tool/manifest/v{PatchLower}/manifest.json", $"{{\n  \"schemaVersion\": \"{PatchLower}\"\n}}\n");
        Write(
            $"src/tool/manifest/v{PatchHigher}/manifest.json",
            $"{{\n  \"schemaVersion\": \"{PatchHigher}\",\n  \"optional\": {{ \"source\": \"none\" }}\n}}\n");
    }

    [Given("the higher one holds a rule that reports on a tree whose manifest sets no key the lower one does not define")]
    public void GivenTheHigherOneHoldsARuleATreeCanMeetWithoutTheNewKey() =>
        Write($"docs/rules/v{PatchHigher}/SPEC002.md", $"# SPEC002: Synthetic rule\n\n| Schema version | {PatchHigher} |\n");

    [Given("every rule the higher one holds and the lower one does not reports only when a manifest sets an optional key the lower one does not define")]
    public void GivenEveryNewRuleIsReachedOnlyUnderAnOptionalKeyTheLowerDoesNotDefine() =>
        Write(
            $"docs/rules/v{PatchHigher}/SPEC002.md",
            $"---\ntitle: \"SPEC002: Synthetic rule\"\nopt_in: optional.source\n---\n\n# SPEC002: Synthetic rule\n\n| Schema version | {PatchHigher} |\n");

    [Given("that rule reports on a tree whose manifest sets no key the lower one does not define")]
    public void GivenThatRuleReportsOnATreeWhoseManifestSetsNoNewKey()
    {
        var rules = Path.Combine(tool.Repository, "docs", "rules");
        var versions = Directory.GetDirectories(rules, "v*")
            .Select(static folder => System.Version.Parse(Path.GetFileName(folder)[1..]))
            .Where(static version => version.Major < 1)
            .Order()
            .ToList();
        HashSet<string> Pages(System.Version version) =>
            [.. Directory.GetFiles(Path.Combine(rules, $"v{version.ToString(3)}"), "*.md").Select(static page => Path.GetFileName(page))];
        bool OptIn(System.Version version, string page) =>
            File.ReadLines(Path.Combine(rules, $"v{version.ToString(3)}", page))
                .Skip(1)
                .TakeWhile(static line => line != "---")
                .Any(static line => line.StartsWith("opt_in:", StringComparison.Ordinal));
        versions.Zip(versions.Skip(1))
            .SelectMany(pair => Pages(pair.Second).Except(Pages(pair.First)).Where(page => !OptIn(pair.Second, page)))
            .Should()
            .NotBeEmpty("a shipped version below 1.0.0 adds a rule page with no opt_in mark");
    }

    [When("their numbers are compared")]
    public async Task WhenTheirNumbersAreCompared() =>
        _numbering = await new SchemaVersionNumbering(new FileSystem(), _numberingRoot ?? tool.Repository).VerifyAsync();

    [Then("the numbers are accepted")]
    public void ThenTheNumbersAreAccepted() => _numbering.Should().BeEmpty();

    [Then("the numbers are refused")]
    public void ThenTheNumbersAreRefused() =>
        _numbering.Should().Contain(new SchemaVersionNumberingFinding(PatchLower, PatchHigher, SchemaVersionNumberingFault.NewRuleWithoutHigherMinor));

    [Then("the higher one's major is greater than the lower one's")]
    public void ThenTheHigherOnesMajorIsGreater() =>
        _numbering.Should().NotContain(static finding => finding.Fault == SchemaVersionNumberingFault.NewRuleWithoutHigherMajor);

    [Then("the higher one's minor is greater than the lower one's")]
    public void ThenTheHigherOnesMinorIsGreater() =>
        _numbering.Should().NotContain(static finding => finding.Fault == SchemaVersionNumberingFault.NewRuleWithoutHigherMinor);

    [Then("they differ in their patch alone")]
    public void ThenTheyDifferInTheirPatchAlone() =>
        _numbering.Should().NotContain(static finding => finding.Fault == SchemaVersionNumberingFault.TemplateOrRulePageChangeNotAPatch);

    [AfterScenario]
    public void DeleteNumberingTree()
    {
        if (_numberingRoot is not null)
        {
            Directory.Delete(_numberingRoot, recursive: true);
        }
    }

    private void Write(string path, string content)
    {
        var fullPath = Path.Combine(_numberingRoot!, path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
    }

    private async Task<Dictionary<string, string>> ReadLockAsync() =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(
            await File.ReadAllTextAsync(Path.Combine(tool.Repository, SchemaVersionLock.RelativePath)))!;

    private const string PatchLower = "0.1.0";

    private const string PatchHigher = "0.1.1";

    private string? _numberingRoot;
    private List<string> _published = [];
    private List<string> _embedded = [];
    private string? _version;
    private IReadOnlyList<SchemaVersionLockFinding> _findings = [];
    private IReadOnlyList<SchemaVersionNumberingFinding> _numbering = [];
}
