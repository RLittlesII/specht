namespace Specht.Tests.Model;

/// <summary>
/// The members of a schema 0.4.0 manifest, the claims a Feature with the prefix <c>ACTIVITY</c> declares and those
/// <c>SPEC4003</c> reports because their text after the claim head begins with the rest of a longer scenario head
/// (<c>0001-F5</c> B-101; A-8): as written, differing in case while claim tags are compared ignoring it and while they are
/// not, and a scenario head that does not begin with the claim head.
/// </summary>
public sealed class ScenarioHeadClaims : TheoryData<string, string[], string[]>
{
    /// <summary>Initializes the rows.</summary>
    public ScenarioHeadClaims()
    {
        Add(
            """{ "ids": { "claim": { "form": "{prefix}-{id}", "id": ".+" }, "scenario": { "form": "{prefix}-SCENARIO-{id}" } } }""",
            ["ACTIVITY-k3x9ab", "ACTIVITY-SCENARIO-m7p2qr"],
            ["ACTIVITY-SCENARIO-m7p2qr"]);
        Add(
            """
            { "claim": { "tagIgnoreCase": true },
              "ids": { "claim": { "form": "{prefix}-{id}", "id": ".+" }, "scenario": { "form": "{prefix}-SCENARIO-{id}" } } }
            """,
            ["ACTIVITY-k3x9ab", "ACTIVITY-scenario-m7p2qr"],
            ["ACTIVITY-scenario-m7p2qr"]);
        Add(
            """
            { "claim": { "tagIgnoreCase": false },
              "ids": { "claim": { "form": "{prefix}-{id}", "id": ".+" }, "scenario": { "form": "{prefix}-SCENARIO-{id}" } } }
            """,
            ["ACTIVITY-scenario-m7p2qr"],
            []);
        Add("""{ "ids": { "claim": { "form": "{prefix}-{id}", "id": ".+" } } }""", ["ACTIVITY-SCENARIO-m7p2qr"], []);
    }
}
