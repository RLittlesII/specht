using Specht.Identifiers;

namespace Specht.Tests.Identifiers;

/// <summary>
/// A clock that answers with the readings it was given, in order, and then the last one again, so a test states every time
/// a request reads and no test reads the time of the machine it runs on.
/// </summary>
internal sealed class FixedClock(params DateTimeOffset[] readings) : IClock
{
    /// <summary>Gets the time of ADR-0033's check value, noon on 2026-11-07, universal time.</summary>
    public static DateTimeOffset CheckTime { get; } = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    /// <inheritdoc />
    public DateTimeOffset GetUtcNow() => readings[Math.Min(_read++, readings.Length - 1)];

    private int _read;
}
