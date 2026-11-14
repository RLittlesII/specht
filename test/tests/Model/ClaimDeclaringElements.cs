using Specht.Model;

namespace Specht.Tests.Model;

/// <summary>
/// The members of a schema 0.4.0 manifest that selects Gherkin claims, a claim file whose one claim tag sits on a Feature,
/// a Scenario, a Scenario Outline with three Examples rows or a Rule holding two untagged scenarios, and the claim it
/// declares as authored, with the keyword line and kind of the element that declares it (<c>0001-F16</c> B-004, B-006,
/// B-007; C-1, C-4): under the shipped tag form, under a declared one and while claim tags ignore case.
/// </summary>
public sealed class ClaimDeclaringElements : TheoryData<string, string, string, int, GherkinClaimElement>
{
    /// <summary>Initializes one row for each declaring element and each tag reading.</summary>
    public ClaimDeclaringElements()
    {
        Add(
            "{}",
            "@B-001\nFeature: Arithmetic\n\n  Scenario: Adds\n    Given two numbers\n",
            "B-001",
            2,
            GherkinClaimElement.Feature);
        Add(
            "{}",
            "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n",
            "B-001",
            4,
            GherkinClaimElement.Scenario);
        Add(
            "{}",
            """
            Feature: Arithmetic

              @B-003
              Scenario Outline: Adds <a>
                Given <a>

                Examples:
                  | a |
                  | 1 |
                  | 2 |
                  | 3 |
            """,
            "B-003",
            4,
            GherkinClaimElement.ScenarioOutline);
        Add(
            "{}",
            """
            Feature: Arithmetic

              @B-004
              Rule: Sums

                Scenario: Adds two
                  Given two numbers

                Scenario: Adds three
                  Given three numbers
            """,
            "B-004",
            4,
            GherkinClaimElement.Rule);
        Add(
            """{ "claim": { "tag": "@claim:{claim}" } }""",
            "Feature: Arithmetic\n\n  @claim:B-001\n  Scenario: Adds\n    Given two numbers\n",
            "B-001",
            4,
            GherkinClaimElement.Scenario);
        Add(
            """{ "claim": { "tagIgnoreCase": true } }""",
            "Feature: Arithmetic\n\n  @b-001\n  Scenario: Adds\n    Given two numbers\n",
            "b-001",
            4,
            GherkinClaimElement.Scenario);
    }
}
