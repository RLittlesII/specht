namespace Specht.Tests.Mint;

/// <summary>
/// A manifest pinned to <c>0.3.1</c> that opts no <c>epic</c>, <c>feature</c> or <c>task</c> type in, and a request for one
/// (<c>0001-F19</c> B-018, B-002; C-2): no <c>epic</c> member, a <c>feature</c> entry naming no strategy, and a <c>task</c>
/// member that is a number and not an object. Each has a grammar, and the <c>feature</c> entry a <c>nanoid</c> sub-object an
/// id could be drawn from, so only the missing opt-in refuses it. The <c>task</c> grammar names the groups the check requires
/// of it (<c>0001-F5</c> B-050).
/// </summary>
public sealed class NotMintedTypesLeftOut : TheoryData<string, string>
{
    /// <summary>Initializes the absent, no-strategy and number rows.</summary>
    public NotMintedTypesLeftOut()
    {
        Add("""{"schemaVersion":"0.3.1","identifiers":{"epic":"^z{3}$"}}""", "epic");
        Add("""{"schemaVersion":"0.3.1","identifiers":{"feature":"^z{3}$"},"mint":{"feature":{"nanoid":{"alphabet":"z","size":3}}}}""", "feature");
        Add("""{"schemaVersion":"0.3.1","identifiers":{"task":"^(?<epic>z)(?<sequence>zz)$"},"mint":{"task":7}}""", "task");
    }
}
