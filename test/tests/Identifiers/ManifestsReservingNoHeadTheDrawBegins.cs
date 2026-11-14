namespace Specht.Tests.Identifiers;

/// <summary>
/// A schema <c>0.4.0</c> manifest, the type requested for a Feature that declares the claim prefix <c>ACTIVITY</c>, and the id
/// its one-letter Nano ID draw is minted as, where the draw does not start with the other type's head (<c>0001-F19</c> B-076,
/// B-077; item <c>0437</c>): a claim id beside the longer scenario head <c>ACTIVITY-SCENARIO-</c>, a claim id in the other
/// letter case where the claim tag setting is case-sensitive, and a scenario id whose own head is the longer one.
/// </summary>
public sealed class ManifestsReservingNoHeadTheDrawBegins : TheoryData<string, string, string>
{
    /// <summary>Initializes the longer-head row, the letter-case row and the scenario row.</summary>
    public ManifestsReservingNoHeadTheDrawBegins()
    {
        Add(
            """
            {
              "schemaVersion": "0.4.0",
              "ids": {
                "claim": { "form": "{prefix}-{id}", "id": ".*", "mint": { "strategy": "nanoid", "nanoid": { "alphabet": "S", "size": 2 } } },
                "scenario": { "form": "{prefix}-SCENARIO-{id}" }
              }
            }
            """,
            "claim",
            "ACTIVITY-SS");
        Add(
            """
            {
              "schemaVersion": "0.4.0",
              "claim": { "tagIgnoreCase": false },
              "ids": {
                "claim": { "form": "{prefix}-{id}", "id": ".*", "mint": { "strategy": "nanoid", "nanoid": { "alphabet": "s", "size": 2 } } },
                "scenario": { "form": "{prefix}-S{id}" }
              }
            }
            """,
            "claim",
            "ACTIVITY-ss");
        Add(
            """
            {
              "schemaVersion": "0.4.0",
              "ids": {
                "claim": { "form": "{prefix}-{id}" },
                "scenario": { "form": "{prefix}-S{id}", "id": ".*", "mint": { "strategy": "nanoid", "nanoid": { "alphabet": "S", "size": 2 } } }
              }
            }
            """,
            "scenario",
            "ACTIVITY-SSS");
    }
}
