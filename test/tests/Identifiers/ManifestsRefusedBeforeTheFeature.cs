namespace Specht.Tests.Identifiers;

/// <summary>
/// A manifest, a per-Feature type and the folder <c>--feature</c> names, or none, where the request is outside B-058's and
/// B-060's scope (<c>0001-F19</c>): the type not opted in or its entry naming no strategy (B-002), and a type opted in with no
/// grammar (B-043).
/// </summary>
public sealed class ManifestsRefusedBeforeTheFeature : TheoryData<string, string, string?>
{
    /// <summary>Initializes one row per refusal, without a Feature and for a folder that holds none.</summary>
    public ManifestsRefusedBeforeTheFeature()
    {
        foreach (var feature in new[] { null, "absent" })
        {
            Add("""{"schemaVersion":"0.3.1","identifiers":{"decision":"^[ab]{2}$"}}""", "decision", feature);
            Add(
                """{"schemaVersion":"0.3.1","identifiers":{"decision":"^[ab]{2}$"},"mint":{"decision":{"nanoid":{"alphabet":"ab","size":2}}}}""",
                "decision",
                feature);
            Add("""{"schemaVersion":"0.3.1","mint":{"claim":{"strategy":"nanoid","nanoid":{"alphabet":"ab","size":2}}}}""", "claim", feature);
        }
    }
}
