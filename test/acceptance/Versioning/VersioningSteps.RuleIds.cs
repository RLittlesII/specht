using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Versioning;

/// <summary>
/// The id a rule reports under each pin (0001-F7 B-151, B-152; ADR-0029). "The root holds a specification with a claim
/// that has no traceability row" writes the tree's Feature with claims B-001 and B-002 and a § 9 row for B-001 alone, so
/// the claim-row rule reports B-002; "the check runs" launches the built tool with <c>--json</c>, and the steps read the
/// rule id the report gives that violation and the rule ids of every other violation in it.
/// </summary>
public sealed partial class VersioningSteps
{
    [Given("the root holds a specification with a claim that has no traceability row")]
    public void GivenTheRootHoldsASpecificationWithAClaimThatHasNoTraceabilityRow() =>
        Tree.WriteFeature("0001", "F1", sections: SpechtTree.SectionsWith("3. Acceptance Criteria", TwoClaims));

    [Then(@"^that violation is reported under (SPEC[0-9]+)$")]
    public void ThenThatViolationIsReportedUnder(string ruleId) =>
        Violations
            .Where(static violation => violation["file"]!.GetValue<string>() == FeaturePath
                                       && violation["identifier"]?.GetValue<string>() == "B-002")
            .Select(static violation => violation["ruleId"]!.GetValue<string>())
            .Should()
            .Equal(ruleId);

    [Then(@"^no violation is reported under a (three|four)-digit rule id$")]
    public void ThenNoViolationIsReportedUnderARuleIdOfThatWidth(string width) =>
        Violations
            .Select(static violation => violation["ruleId"]!.GetValue<string>())
            .Should()
            .NotContain(id => Regex.IsMatch(id, width == "three" ? "^SPEC[0-9]{3}$" : "^SPEC[0-9]{4}$"));
}
