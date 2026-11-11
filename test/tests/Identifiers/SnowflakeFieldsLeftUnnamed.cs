namespace Specht.Tests.Identifiers;

/// <summary>
/// Snowflake sub-objects that leave one field unnamed and name the other two (<c>0001-F19</c> B-024 to B-026; C-16), the
/// bits below that field, its width, and the draws two requests make for it: the datacenter, the worker and the sequence
/// start, each with the scenario's draw first.
/// </summary>
public sealed class SnowflakeFieldsLeftUnnamed : TheoryData<string, int, int, int, int>
{
    /// <summary>Initializes the datacenter, worker and sequence start rows.</summary>
    public SnowflakeFieldsLeftUnnamed()
    {
        Add("""{ "worker": 9, "sequenceStart": 0 }""", 17, 5, 17, 4);
        Add("""{ "datacenter": 3, "sequenceStart": 0 }""", 12, 5, 22, 4);
        Add("""{ "datacenter": 3, "worker": 9 }""", 0, 12, 3001, 4);
    }
}
