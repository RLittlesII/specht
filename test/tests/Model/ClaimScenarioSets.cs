namespace Specht.Tests.Model;

/// <summary>
/// A claim file holding a Scenario, a Scenario Outline and a Rule with two scenarios, with <c>@B-001</c> on one element,
/// and the keyword lines of the scenarios carrying the claim, in document order (<c>0001-F16</c> B-025): the declaring
/// Scenario, the declaring Scenario Outline, both scenarios under a declaring Rule, and every one in the file when the
/// Feature declares it. Every element keeps its line in each row; only the tag moves.
/// </summary>
public sealed class ClaimScenarioSets : TheoryData<string, int[]>
{
    /// <summary>Initializes one row for each declaring element.</summary>
    public ClaimScenarioSets()
    {
        Add(
            """

            Feature: Arithmetic
              @B-001
              Scenario: Adds
                Given two numbers

              Scenario Outline: Adds <a>
                Given <a>

                Examples:
                  | a |
                  | 1 |

              Rule: Sums

                Scenario: Adds three
                  Given three numbers

                Scenario: Adds four
                  Given four numbers
            """,
            [4]);
        Add(
            """

            Feature: Arithmetic

              Scenario: Adds
                Given two numbers
              @B-001
              Scenario Outline: Adds <a>
                Given <a>

                Examples:
                  | a |
                  | 1 |

              Rule: Sums

                Scenario: Adds three
                  Given three numbers

                Scenario: Adds four
                  Given four numbers
            """,
            [7]);
        Add(
            """

            Feature: Arithmetic

              Scenario: Adds
                Given two numbers

              Scenario Outline: Adds <a>
                Given <a>

                Examples:
                  | a |
                  | 1 |
              @B-001
              Rule: Sums

                Scenario: Adds three
                  Given three numbers

                Scenario: Adds four
                  Given four numbers
            """,
            [16, 19]);
        Add(
            """
            @B-001
            Feature: Arithmetic

              Scenario: Adds
                Given two numbers

              Scenario Outline: Adds <a>
                Given <a>

                Examples:
                  | a |
                  | 1 |

              Rule: Sums

                Scenario: Adds three
                  Given three numbers

                Scenario: Adds four
                  Given four numbers
            """,
            [4, 7, 16, 19]);
    }
}
