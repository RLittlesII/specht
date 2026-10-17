using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Manifest;

/// <summary>Sign-off defaults and their override boundary (0001-F5 B-065 to B-067).</summary>
public sealed partial class ManifestSteps
{
    [Given("the manifest declares no table headers")]
    public void GivenTheManifestDeclaresNoTableHeaders() => Manifest.Remove("tables");

    [Given("the manifest declares only its matrix table headers")]
    public void GivenOnlyMatrixHeaders() =>
        Manifest["tables"] = new JsonObject { ["matrix"] = new JsonArray("ID", "Case", "Proof", "State") };

    [Given("the manifest declares sign-off headers {string}")]
    public void GivenSignOffHeaders(string headers) =>
        Manifest["tables"] = new JsonObject { ["signOff"] = new JsonArray(headers.Split(" | ").Select(static header => JsonValue.Create(header)).ToArray()) };

    [Given("the root holds a specification whose sign-off table has a {string} column in second place")]
    public void GivenSignOffWithWrongStatusHeader(string header) => Tree.WriteFeature(
        "0001",
        "F1",
        sections: SpecTree.SectionsWith(
            "12. Sign-off",
            $"## 12. Sign-off\n\n| Section | {header} | Reviewer | Note |\n| --- | --- | --- | --- |\n| 1-5 | 🟡 | spec-reviewer | Draft |\n"));

    [Then("the table-header rule expects {string} for the sign-off section")]
    public void ThenSignOffHeadersAreChecked(string headers)
    {
        _rejection.Should().BeNull();
        var violation = _report!.Violations.Where(static violation => violation.RuleId == "SPEC013").Should().ContainSingle().Subject;
        violation.Identifier.Should().Be("12. Sign-off");
        violation.Message.Should().Contain(headers);
    }

    [Then("the sign-off table headers are {string}")]
    public void ThenSignOffHeaders(string headers) => Structure.Tables["signOff"].Should().Equal(headers.Split(" | "));

    [Then("the matrix table headers keep the declared names")]
    public void ThenMatrixHeadersStayExplicit() => Structure.Tables["matrix"].Should().Equal("ID", "Case", "Proof", "State");

    [Then("the table map contains no sign-off entry")]
    public void ThenNoSignOffTable() => Structure.Tables.Should().NotContainKey("signOff");
}
