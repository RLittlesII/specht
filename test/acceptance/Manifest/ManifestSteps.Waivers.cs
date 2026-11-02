using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Manifest;

/// <summary>Own-off defaults, overrides and evaluated counts through the shipped waiver rule (0001-F5 B-061, B-062).</summary>
public sealed partial class ManifestSteps
{
    [Given("the manifest sets no level for the waiver rule")]
    public void GivenNoWaiverOverride() => Named("rules").Remove(RuleIds.Under(Manifest, "SPEC034"));

    [Given("the manifest sets the waiver rule to warning")]
    public void GivenWaiverWarning() => Named("rules")[RuleIds.Under(Manifest, "SPEC034")] = "warning";

    [Given("the root holds a specification with a waived row that names no approver")]
    public void GivenUnsignedWaiver() => Tree.WriteFeature("0001", "F1", sections: SpechtTree.SectionsWith(
        "9. Traceability Matrix", "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| --- | --- | --- | --- |\n"
            + "| B-001 | A waiver | | Waived |\n"));

    [Then("the waiver rule reports nothing")]
    public void ThenNoWaiverFinding()
    {
        _rejection.Should().BeNull();
        _report!.Violations.Should().NotContain(violation => violation.RuleId == RuleIds.Under(Manifest, "SPEC034"));
    }

    [Then("the waiver rule reports that row at warning severity")]
    public void ThenWaiverWarning()
    {
        _rejection.Should().BeNull();
        var violation = _report!.Violations.Where(violation => violation.RuleId == RuleIds.Under(Manifest, "SPEC034")).Should().ContainSingle().Subject;
        violation.Identifier.Should().Be("B-001");
        violation.Severity.Should().Be(Specht.Model.SpechtSeverity.Warning);
    }

    [Given("the default manifest of schema version {string}")]
    public void GivenPinnedDefaultForWaivers(string pin) =>
        _waiverDefaults = EngineServices.Versions().Versions.Single(version => version.Number.ToString() == pin).Manifest;

    [When("its rule settings are read")]
    public void WhenDefaultRuleSettingsAreRead() => _structure = EngineServices.Parser().Parse(_waiverDefaults!);

    [Then("the waiver rule and the retired-claim rule are set to off")]
    public void ThenDefaultWaiverLevels()
    {
        Structure.Rules["SPEC034"].Should().Be(SpechtRuleLevel.Off);
        Structure.Rules["SPEC035"].Should().Be(SpechtRuleLevel.Off);
    }

    [Then("the count of rules evaluated excludes the waiver rule")]
    public void ThenWaiverIsNotCounted()
    {
        _rejection.Should().BeNull();
        var version = EngineServices.Versions().Versions.Single(static version => version.Number.ToString() == "0.2.0");
        _report!.RulesEvaluated.Should().Be(version.RuleIds.Count - 2);
    }

    private string? _waiverDefaults;
}
