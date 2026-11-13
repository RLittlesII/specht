namespace Specht.Tests.Identifiers;

/// <summary>
/// A schema <c>0.4.0</c> manifest and the type requested for a Feature that declares the claim prefix <c>ACTIVITY</c>, whose
/// one-letter Nano ID draw starts with the other type's longer head (<c>0001-F19</c> B-076, B-077; item <c>0437</c>): a
/// scenario id under the claim head <c>ACTIVITY-S</c>, a claim id under the scenario head <c>ACTIVITY-S</c>, and a claim id in
/// the other letter case where the claim tag setting ignores case.
/// </summary>
public sealed class ManifestsReservingTheOtherHead : TheoryData<string, string>
{
    /// <summary>Initializes the scenario row, the claim row and the ignore-case row.</summary>
    public ManifestsReservingTheOtherHead()
    {
        Add(
            """
            {
              "schemaVersion": "0.4.0",
              "ids": {
                "claim": { "form": "{prefix}-S{id}" },
                "scenario": { "form": "{prefix}-{id}", "id": ".*", "mint": { "strategy": "nanoid", "nanoid": { "alphabet": "S", "size": 2 } } }
              }
            }
            """,
            "scenario");
        Add(
            """
            {
              "schemaVersion": "0.4.0",
              "ids": {
                "claim": { "form": "{prefix}-{id}", "id": ".*", "mint": { "strategy": "nanoid", "nanoid": { "alphabet": "S", "size": 2 } } },
                "scenario": { "form": "{prefix}-S{id}" }
              }
            }
            """,
            "claim");
        Add(
            """
            {
              "schemaVersion": "0.4.0",
              "claim": { "tagIgnoreCase": true },
              "ids": {
                "claim": { "form": "{prefix}-{id}", "id": ".*", "mint": { "strategy": "nanoid", "nanoid": { "alphabet": "s", "size": 2 } } },
                "scenario": { "form": "{prefix}-S{id}" }
              }
            }
            """,
            "claim");
    }
}
