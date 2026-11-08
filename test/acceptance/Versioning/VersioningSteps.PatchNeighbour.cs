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

    private readonly Dictionary<string, string> _patchReports = new(StringComparer.Ordinal);
    private List<(string Lower, string Higher)> _patchNeighbours = [];
}
