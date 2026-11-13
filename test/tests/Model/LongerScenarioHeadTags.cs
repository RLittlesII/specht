namespace Specht.Tests.Model;

/// <summary>
/// The members of a schema 0.4.0 manifest whose scenario head for a Feature with the prefix <c>ACTIVITY</c> is longer
/// than its claim head and begins with it, and a companion tag that begins with the scenario head, which is not read as a
/// claim tag (<c>0001-F5</c> B-095): as written, and differing in case while claim tags are compared ignoring it.
/// </summary>
public sealed class LongerScenarioHeadTags : TheoryData<string, string>
{
    /// <summary>Initializes the rows.</summary>
    public LongerScenarioHeadTags()
    {
        Add(
            """{ "ids": { "claim": { "form": "{prefix}-{id}", "id": ".+" }, "scenario": { "form": "{prefix}-SCENARIO-{id}" } } }""",
            "@ACTIVITY-SCENARIO-m7p2qr");
        Add(
            """
            { "claim": { "tagIgnoreCase": true },
              "ids": { "claim": { "form": "{prefix}-{id}", "id": ".+" }, "scenario": { "form": "{prefix}-SCENARIO-{id}" } } }
            """,
            "@activity-scenario-m7p2qr");
    }
}
