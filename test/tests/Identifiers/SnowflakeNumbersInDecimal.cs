namespace Specht.Tests.Identifiers;

/// <summary>
/// Snowflake numbers written in decimal digits under the encoding <c>none</c> (<c>0001-F19</c> B-032; C-15): a Snowflake
/// sub-object naming every field, a reading of the clock, and the number. The rows are ADR-0033's check value, zero, a
/// number with every field set, and the largest number a layout holds.
/// </summary>
public sealed class SnowflakeNumbersInDecimal : TheoryData<string, string, string>
{
    /// <summary>Initializes the check value, zero, a mixed number and the largest one.</summary>
    public SnowflakeNumbersInDecimal()
    {
        Add("""{ "datacenter": 0, "worker": 0, "sequenceStart": 0 }""", "2026-11-07T12:00:00Z", "112521432268800000");
        Add("""{ "datacenter": 0, "worker": 0, "sequenceStart": 0 }""", "2026-01-01T00:00:00Z", "0");
        Add("""{ "datacenter": 3, "worker": 9, "sequenceStart": 2709 }""", "2026-10-23T12:00:00Z", "107085614285232789");
        Add(
            """{ "datacenterBits": 30, "workerBits": 30, "sequenceBits": 2, "datacenter": 1073741823, "worker": 1073741823, "sequenceStart": 3 }""",
            "2026-01-01T00:00:00.001Z",
            "9223372036854775807");
    }
}
