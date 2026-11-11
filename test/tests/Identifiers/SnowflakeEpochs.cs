namespace Specht.Tests.Identifiers;

/// <summary>
/// Snowflake sub-objects that name an epoch (<c>0001-F19</c> B-007), a reading of the clock, the number of bits below the
/// time field and the milliseconds from that epoch to that reading: the scenario's 2026-01-01 and noon on 2026-10-23, an
/// epoch other than the default under the default widths, and one under widths of its own read at a time with milliseconds.
/// Every field is named, so nothing is drawn.
/// </summary>
public sealed class SnowflakeEpochs : TheoryData<string, string, int, long>
{
    /// <summary>Initializes the scenario's row, a 2024 epoch, and a 2025 epoch under widths of 3, 4 and 6 bits.</summary>
    public SnowflakeEpochs()
    {
        Add("""{ "epoch": "2026-01-01", "datacenter": 0, "worker": 0, "sequenceStart": 0 }""", "2026-10-23T12:00:00Z", 22, 25531200000);
        Add("""{ "epoch": "2024-02-29", "datacenter": 1, "worker": 2, "sequenceStart": 3 }""", "2026-10-23T12:00:00Z", 22, 83592000000);
        Add(
            """{ "epoch": "2025-07-04", "datacenterBits": 3, "workerBits": 4, "sequenceBits": 6, "datacenter": 7, "worker": 15, "sequenceStart": 63 }""",
            "2026-10-23T12:00:00.123Z",
            13,
            41169600123);
    }
}
