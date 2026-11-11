namespace Specht.Tests.Identifiers;

/// <summary>
/// Snowflake sub-objects that name no epoch (<c>0001-F19</c> B-022), as JSON or absent, a reading of the clock and the
/// milliseconds from 2026-01-01 at midnight, universal time, to it: no sub-object at ADR-0033's check time, an empty one a
/// millisecond after the epoch, and one naming a datacenter and a worker at a time written with an offset.
/// </summary>
public sealed class SnowflakeSettingsNamingNoEpoch : TheoryData<string?, string, long>
{
    /// <summary>Initializes the absent, the empty and the field-naming rows.</summary>
    public SnowflakeSettingsNamingNoEpoch()
    {
        Add(null, "2026-11-07T12:00:00Z", 26827200000);
        Add("{}", "2026-01-01T00:00:00.001Z", 1);
        Add("""{ "datacenter": 3, "worker": 9 }""", "2026-03-01T06:30:00-02:00", 5128200000);
    }
}
