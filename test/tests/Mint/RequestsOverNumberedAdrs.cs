namespace Specht.Tests.Mint;

/// <summary>
/// The <c>mint</c> key of a manifest an ADR number is requested of, over a tree whose ADRs are already numbered
/// (<c>0001-F19</c> B-013; C-1): ADR numbers opted in to Nano ID, ADR numbers opted in to Snowflake, and no type opted in,
/// the scenario's three rows.
/// </summary>
public sealed class RequestsOverNumberedAdrs : TheoryData<string?>
{
    /// <summary>Initializes the Nano ID, Snowflake and no-type rows.</summary>
    public RequestsOverNumberedAdrs()
    {
        Add("""{ "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }""");
        Add("""{ "adr": { "strategy": "snowflake" } }""");
        Add(default(string));
    }
}
