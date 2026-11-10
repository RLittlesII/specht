namespace Specht.Tests.Mint;

/// <summary>
/// A manifest whose <c>adr</c> entry under <c>mint</c> names no strategy, for a request of an <c>adr</c> id (<c>0001-F19</c>
/// B-002, B-047), one row per row B-002's scenario takes from B-047's: no <c>strategy</c> member, and a number in its place.
/// Each has a grammar and a <c>nanoid</c> sub-object an id could be drawn from, so only the missing strategy refuses it.
/// </summary>
public sealed class EntriesNamingNoStrategyRequests : TheoryData<string>
{
    /// <summary>Initializes the absent and number rows.</summary>
    public EntriesNamingNoStrategyRequests()
    {
        Add("""{"schemaVersion":"0.3.1","identifiers":{"adr":"^z{3}$"},"mint":{"adr":{"nanoid":{"alphabet":"z","size":3}}}}""");
        Add("""{"schemaVersion":"0.3.1","identifiers":{"adr":"^z{3}$"},"mint":{"adr":{"strategy":7,"nanoid":{"alphabet":"z","size":3}}}}""");
    }
}
