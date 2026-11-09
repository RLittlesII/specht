namespace Specht.Tests.Identifiers;

/// <summary>
/// An <c>adr</c> entry under <c>mint</c> that names no strategy (<c>0001-F19</c> B-047, C-2, C-12), one row per row of the
/// scenario: no <c>strategy</c> member, and a number in its place. Each holds a <c>nanoid</c> sub-object an id could be drawn
/// from, so a default strategy would mint one.
/// </summary>
public sealed class EntriesNamingNoStrategy : TheoryData<string>
{
    /// <summary>Initializes the absent and number rows.</summary>
    public EntriesNamingNoStrategy()
    {
        Add("""{ "nanoid": { "alphabet": "ab", "size": 2 } }""");
        Add("""{ "strategy": 7, "nanoid": { "alphabet": "ab", "size": 2 } }""");
    }
}
