namespace Specht.Tests.Model;

/// <summary>Claim file texts with the keyword line and the trimmed title of each Scenario the reader lists.</summary>
public sealed class ClaimScenarioTitleSets : TheoryData<string, int[], string[]>
{
    /// <summary>Initializes one row for each keyword family.</summary>
    public ClaimScenarioTitleSets()
    {
        Add(
            """
            Feature: Arithmetic

              @B-001
              Scenario:   Adds two numbers   
                Given two numbers

              Scenario Outline: Adds <a>  
                Given <a>

                Examples:
                  | a |
                  | 1 |

              Rule: Sums

                Scenario:Adds three
                  Given three numbers
            """,
            [4, 7, 16],
            ["Adds two numbers", "Adds <a>", "Adds three"]);
        Add(
            """
            Feature: Arithmetic

              Example: Adds
                Given two numbers

              Scenario Template: Adds <a>
                Given <a>

                Scenarios:
                  | a |
                  | 1 |

              Rule: Sums

                Example: Adds four
                  Given four numbers
            """,
            [3, 6, 15],
            ["Adds", "Adds <a>", "Adds four"]);
    }
}
