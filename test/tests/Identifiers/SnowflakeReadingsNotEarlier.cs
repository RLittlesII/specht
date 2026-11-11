namespace Specht.Tests.Identifiers;

/// <summary>
/// How many milliseconds after its first reading a Snowflake type's clock reads at the guard's next draw (<c>0001-F19</c>
/// B-010): the same millisecond, and one later, the two readings that are not earlier than the first (B-029).
/// </summary>
public sealed class SnowflakeReadingsNotEarlier : TheoryData<int>
{
    /// <summary>Initializes the row reading the same millisecond and the row reading one later.</summary>
    public SnowflakeReadingsNotEarlier()
    {
        Add(0);
        Add(1);
    }
}
