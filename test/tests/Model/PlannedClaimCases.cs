namespace Specht.Tests.Model;

/// <summary>
/// The members of a schema 0.4.0 manifest that selects Gherkin claims, a claim file, the claim it declares, the lines of
/// that claim's cases and the lines of those planned (<c>0001-F16</c> B-013; C-4): a scenario carrying <c>@ignore</c>
/// under the shipped <c>plannedTags</c> default, the same tag when the manifest lists another, the tag it lists, and an
/// outline whose one Examples block of two carries <c>@ignore</c>.
/// </summary>
public sealed class PlannedClaimCases : TheoryData<string, string, string, int[], int[]>
{
    /// <summary>Initializes one row for each planned tag reading.</summary>
    public PlannedClaimCases()
    {
        Add("{}", "Feature: Arithmetic\n\n  @B-001 @ignore\n  Scenario: Adds\n    Given two numbers\n", "B-001", [4], [4]);
        Add(
            """{ "claim": { "plannedTags": ["@wip"] } }""",
            "Feature: Arithmetic\n\n  @B-001 @ignore\n  Scenario: Adds\n    Given two numbers\n",
            "B-001",
            [4],
            []);
        Add(
            """{ "claim": { "plannedTags": ["@wip"] } }""",
            "Feature: Arithmetic\n\n  @B-001 @wip\n  Scenario: Adds\n    Given two numbers\n",
            "B-001",
            [4],
            [4]);
        Add(
            "{}",
            """
            Feature: Arithmetic

              @B-003
              Scenario Outline: Adds <a>
                Given <a>

                @ignore
                Examples: Later
                  | a |
                  | 1 |
                  | 2 |

                Examples: Now
                  | a |
                  | 3 |
            """,
            "B-003",
            [10, 11, 15],
            [10, 11]);
    }
}
