namespace Specht.Tests.Identifiers;

/// <summary>
/// Snowflake numbers written in base32 (<c>0001-F19</c> B-030, B-031, B-056; C-15): the encoding named, <c>base32</c> or
/// none, a Snowflake sub-object naming every field, a reading of the clock, and the number in thirteen characters over
/// <c>0123456789abcdefghjkmnpqrstvwxyz</c>. The rows are ADR-0033's check value under both, zero, 31 and 32 at the low
/// characters, a number with every field set, and the largest number a layout holds.
/// </summary>
public sealed class SnowflakeNumbersInBase32 : TheoryData<string?, string, string, string>
{
    /// <summary>Initializes the check value, zero, 31, 32, a mixed number and the largest one.</summary>
    public SnowflakeNumbersInBase32()
    {
        Add("\"base32\"", Zeros, "2026-11-07T12:00:00Z", "033y1mp000000");
        Add(null, Zeros, "2026-11-07T12:00:00Z", "033y1mp000000");
        Add("\"base32\"", Zeros, "2026-01-01T00:00:00Z", "0000000000000");
        Add("\"base32\"", """{ "datacenter": 0, "worker": 0, "sequenceStart": 31 }""", "2026-01-01T00:00:00Z", "000000000000z");
        Add(null, """{ "datacenter": 0, "worker": 0, "sequenceStart": 32 }""", "2026-01-01T00:00:00Z", "0000000000010");
        Add("\"base32\"", """{ "datacenter": 3, "worker": 9, "sequenceStart": 2709 }""", "2026-10-23T12:00:00Z", "02z3hsj00d6mn");
        Add(null, Largest, "2026-01-01T00:00:00.001Z", "7zzzzzzzzzzzz");
    }

    private const string Zeros = """{ "datacenter": 0, "worker": 0, "sequenceStart": 0 }""";

    private const string Largest =
        """{ "datacenterBits": 30, "workerBits": 30, "sequenceBits": 2, "datacenter": 1073741823, "worker": 1073741823, "sequenceStart": 3 }""";
}
