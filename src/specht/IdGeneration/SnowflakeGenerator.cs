using System.Globalization;

namespace Specht.IdGeneration;

/// <summary>The Snowflake strategy: time since the epoch, datacenter, worker and sequence packed into 63 bits, as decimal.</summary>
/// <param name="settings">The epoch, widths and values.</param>
/// <param name="clock">The clock.</param>
/// <param name="random">The random source a random sequence start draws from.</param>
public sealed class SnowflakeGenerator(SnowflakeSettings settings, TimeProvider clock, IRandomSource random) : IIdGenerator
{
    /// <inheritdoc />
    /// <exception cref="IdRefusedException">The clock reads earlier than the last id, or the time field is spent.</exception>
    public string Next()
    {
        var mask = (1L << settings.SequenceBits) - 1;
        var now = Now();

        if (now < _last)
        {
            throw new IdRefusedException("clock moved backwards");
        }

        if (now == _last)
        {
            _sequence = (_sequence + 1) & mask;
            if (_sequence == _first)
            {
                while (now <= _last)
                {
                    now = Now();
                }

                _first = _sequence = Start(mask);
            }
        }
        else
        {
            _first = _sequence = Start(mask);
        }

        if (now < 0 || now >= 1L << settings.TimeBits)
        {
            throw new IdRefusedException("time field out of range");
        }

        _last = now;
        var id = (now << (settings.DatacenterBits + settings.WorkerBits + settings.SequenceBits))
            | ((long) settings.Datacenter << (settings.WorkerBits + settings.SequenceBits))
            | ((long) settings.Worker << settings.SequenceBits)
            | _sequence;
        return id.ToString(CultureInfo.InvariantCulture);
    }

    private long Now() => clock.GetUtcNow().ToUnixTimeMilliseconds() - settings.EpochMilliseconds;

    private long Start(long mask) => settings.Start == SequenceStart.Zero ? 0 : random.Next((int) (mask + 1));

    private long _last = -1;
    private long _sequence;
    private long _first;
}
