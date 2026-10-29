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

    [When("their numbers are compared")]
    public async Task WhenTheirNumbersAreCompared() =>
        _numbering = await new SchemaVersionNumbering(new FileSystem(), tool.Repository).VerifyAsync();

    [Then("the higher one's major is greater than the lower one's")]
    public void ThenTheHigherOnesMajorIsGreater() =>
        _numbering.Should().NotContain(static finding => finding.Fault == SchemaVersionNumberingFault.NewRuleWithoutHigherMajor);

    [Then("the higher one's minor is greater than the lower one's")]
    public void ThenTheHigherOnesMinorIsGreater() =>
        _numbering.Should().NotContain(static finding => finding.Fault == SchemaVersionNumberingFault.NewRuleWithoutHigherMinor);

    [Then("they differ in their patch alone")]
    public void ThenTheyDifferInTheirPatchAlone() =>
        _numbering.Should().NotContain(static finding => finding.Fault == SchemaVersionNumberingFault.TemplateOrRulePageChangeNotAPatch);

    private async Task<Dictionary<string, string>> ReadLockAsync() =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(
            await File.ReadAllTextAsync(Path.Combine(tool.Repository, SchemaVersionLock.RelativePath)))!;

    private List<string> _published = [];
    private List<string> _embedded = [];
    private string? _version;
    private IReadOnlyList<SchemaVersionLockFinding> _findings = [];
    private IReadOnlyList<SchemaVersionNumberingFinding> _numbering = [];
}
