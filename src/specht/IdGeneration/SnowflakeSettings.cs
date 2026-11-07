using System.Text;

namespace Specht.IdGeneration;

/// <summary>Snowflake's own settings: the epoch, the field widths, and the datacenter and worker values.</summary>
/// <param name="EpochMilliseconds">Unix milliseconds the time field counts from.</param>
/// <param name="DatacenterBits">Width of the datacenter field.</param>
/// <param name="WorkerBits">Width of the worker field.</param>
/// <param name="SequenceBits">Width of the sequence field.</param>
/// <param name="Datacenter">The datacenter value.</param>
/// <param name="Worker">The worker value.</param>
/// <param name="Start">Where the sequence starts within a millisecond.</param>
public sealed record SnowflakeSettings(
    long EpochMilliseconds,
    int DatacenterBits,
    int WorkerBits,
    int SequenceBits,
    int Datacenter,
    int Worker,
    SequenceStart Start)
{
    /// <summary>The supplied generator: epoch 2026-01-01, 5 datacenter bits, 5 worker bits, 12 sequence bits, all zero.</summary>
    public static SnowflakeSettings Supplied { get; } = new(1767225600000, 5, 5, 12, 0, 0, SequenceStart.Zero);

    /// <summary>Width of the time field: 63 bits less the others, the sign bit kept clear.</summary>
    public int TimeBits => 63 - DatacenterBits - WorkerBits - SequenceBits;

    /// <summary>Derives a field value from text by FNV-1a, masked to the field's width.</summary>
    /// <param name="text">A machine name, a worktree path or a branch name.</param>
    /// <param name="bits">The field width.</param>
    /// <returns>The value.</returns>
    public static int Hash(string text, int bits)
    {
        var hash = 2166136261u;
        foreach (var b in Encoding.UTF8.GetBytes(text))
        {
            hash = unchecked((hash ^ b) * 16777619u);
        }

        return (int) (hash & ((1u << bits) - 1));
    }
}
