using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Engine.Baseline;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Versioning;

public sealed partial class VersioningSteps
{
    [Given("a tree the check runs on under a shipped schema version below 1.0.0")]
    public void GivenATreeTheCheckRunsOnUnderAShippedVersionBelowOneZeroZero() => BaselineTree.Write(Tree);

    [Given("the tool ships a version higher than it in its patch alone")]
    public void GivenTheToolShipsAVersionHigherThanItInItsPatchAlone()
    {
        var below = EngineServices.Versions().Versions.Select(static version => version.Number).Where(static number => number.Major < 1).ToList();
        _patchNeighbours =
        [
            .. below.SelectMany(lower => below
                .Where(higher => higher.Major == lower.Major && higher.Minor == lower.Minor && higher.Patch > lower.Patch)
                .Select(higher => (lower.ToString(), higher.ToString()))),
        ];
        _patchNeighbours.Should().NotBeEmpty();
    }

    [When("the tree is checked under each of the two versions")]
    public void WhenTheTreeIsCheckedUnderEachOfTheTwoVersions()
    {
        foreach (var (lower, higher) in _patchNeighbours)
        {
            foreach (var version in new[] { lower, higher })
            {
                Tree.Pin(version);
                var (stdout, stderr, exitCode) = AcceptanceTool.Launch(Tree.Root, "--root", ".", "--json");
                exitCode.Should().NotBe(2, stderr).And.NotBe(3, stderr);
                var document = JsonNode.Parse(stdout)!.AsObject();
                document.Remove("schemaVersion");
                _patchReports[version] = document.ToJsonString();
            }
        }
    }

    [Then("the two reports are the same, the schema version each names aside")]
    public void ThenTheTwoReportsAreTheSameTheSchemaVersionEachNamesAside()
    {
        foreach (var (lower, higher) in _patchNeighbours)
        {
            _patchReports[higher].Should().Be(_patchReports[lower], "{0} differs from {1} in its patch alone", higher, lower);
        }
    }

    [Given("a Feature declares the claim prefix {string} and the claim {string}")]
    public void GivenAFeatureDeclaresTheClaimPrefixAndTheClaim(string prefix, string claim) =>
        Tree.WriteCoLocatedFeature(
            "src/ci",
            "0001",
            "F1",
            new Dictionary<string, string> { ["claim_prefix"] = $"\"{prefix}\"" },
            SpechtTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + $"| {claim} | It does the thing. | brd | Active |\n"));

    [When(@"^the tree is checked under (\S+) and under (\S+)$")]
    public void WhenTheTreeIsCheckedUnderEachPin(string lower, string higher)
    {
        foreach (var version in new[] { lower, higher })
        {
            Tree.Pin(version);
            _pinnedChecks[version] = AcceptanceTool.Launch(Tree.Root, "--root", ".", "--json");
        }
    }

    [Then(@"^under (\S+) SPEC1002 reports the claim prefix declaration and SPEC4003 reports nothing$")]
    public void ThenUnderTheLowerPinSpec1002ReportsTheDeclaration(string version)
    {
        var violations = PrefixedFeatureViolations(version);
        violations.Should().Contain(static violation =>
            (string?) violation["ruleId"] == "SPEC1002" && (string?) violation["identifier"] == "claim_prefix");
        violations.Should().NotContain(static violation => (string?) violation["ruleId"] == "SPEC4003");
    }

    [Then(@"^under (\S+) SPEC4003 reports claim ""(.*)"" for lacking the prefix ""(.*)"" and SPEC1002 reports nothing$")]
    public void ThenUnderTheHigherPinSpec4003ReportsTheClaim(string version, string claim, string prefix)
    {
        var violations = PrefixedFeatureViolations(version);
        violations.Should().NotContain(static violation => (string?) violation["ruleId"] == "SPEC1002");
        var finding = violations.Where(static violation => (string?) violation["ruleId"] == "SPEC4003").Should().ContainSingle().Subject;
        ((string?) finding["identifier"]).Should().Be(claim);
        ((string?) finding["message"]).Should().Contain($"'{claim}'").And.Contain($"'{prefix}'");
    }

    [Then("neither check exits with code 2 or 3")]
    public void ThenNeitherCheckExitsWithCodeTwoOrThree()
    {
        _pinnedChecks.Should().HaveCount(2);
        foreach (var (_, stderr, exitCode) in _pinnedChecks.Values)
        {
            exitCode.Should().BeOneOf([0, 1], stderr);
        }
    }

    private List<JsonNode> PrefixedFeatureViolations(string version)
    {
        var (stdout, stderr, _) = _pinnedChecks[version];
        stdout.Should().NotBeEmpty("the check under {0} wrote its document: {1}", version, stderr);
        return
        [
            .. JsonNode.Parse(stdout)!["violations"]!.AsArray()
                .Select(static violation => violation!)
                .Where(static violation => (string?) violation["file"] == "src/ci/.spec/README.md"),
        ];
    }

    private readonly Dictionary<string, (string Stdout, string Stderr, int ExitCode)> _pinnedChecks = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _patchReports = new(StringComparer.Ordinal);
    private List<(string Lower, string Higher)> _patchNeighbours = [];
}
