namespace Specht.Tests.Manifest;

/// <summary>
/// Each shipped pin below schema 0.4.0 that reads <c>claim</c>, and a <c>claim</c> object holding only members schema
/// 0.4.0 adds (<c>0001-F16</c> B-028, decision 0002).
/// </summary>
public sealed class ClaimMembersBelowZeroFourZero : TheoryData<string, string>
{
    /// <summary>Initializes every pin with a valid Gherkin selection, an invalid one, and the remaining members.</summary>
    public ClaimMembersBelowZeroFourZero()
    {
        foreach (var pin in new[] { "0.2.0", "0.2.1", "0.3.0", "0.3.1" })
        {
            Add(pin, """
                { "source": "gherkin",
                  "files": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "arithmetic" } ] }
                """);
            Add(pin, """{ "source": "gherkin" }""");
            Add(pin, """
                { "plannedTags": [ "@wip" ], "withdrawnTags": [ "@withdrawn" ], "supersededTags": [ "@superseded" ],
                  "lifecycleKey": "status", "scopes": { "featureTag": "@feature:{feature}", "epicTag": "@epic:{epic}" } }
                """);
        }
    }
}
