using System.Globalization;
using System.Text.Json.Nodes;

namespace Specht.Identifiers;

/// <summary>
/// The Snowflake strategy: the milliseconds from an epoch to the clock's reading above a datacenter, a worker and a sequence
/// field, written in decimal digits, every setting read from a type's <c>snowflake</c> sub-object alone (ADR-0033;
/// <c>0001-F19</c> C-3, C-16).
/// </summary>
/// <implements epic="0001" feature="F19" claim="B-007" />
/// <implements epic="0001" feature="F19" claim="B-023" />
/// <implements epic="0001" feature="F19" claim="B-024" />
/// <implements epic="0001" feature="F19" claim="B-025" />
/// <implements epic="0001" feature="F19" claim="B-026" />
/// <implements epic="0001" feature="F19" claim="B-027" />
/// <implements epic="0001" feature="F19" claim="B-028" />
/// <implements epic="0001" feature="F19" claim="B-029" />
public sealed class SnowflakeStrategy : IIdStrategy
{
    /// <summary>Initializes a new instance of the <see cref="SnowflakeStrategy"/> class from the sub-object, filling every absent setting.</summary>
    /// <param name="settings">The type's <c>snowflake</c> sub-object, or nothing when the entry names none.</param>
    /// <param name="type">The id type, which a refusal names.</param>
    /// <param name="random">The random source each unnamed field is drawn from.</param>
    /// <param name="clock">The clock each draw reads.</param>
    /// <param name="key">The setting the entry is read from, which a refusal names: <c>mint.&lt;type&gt;</c> when nothing is named.</param>
    /// <exception cref="IdRefusedException">
    /// The sub-object is not an object, or names an epoch, a width or a field value no number can be drawn from.
    /// </exception>
    /// <implements epic="0001" feature="F19" claim="B-007" />
    /// <implements epic="0001" feature="F19" claim="B-022" />
    /// <implements epic="0001" feature="F19" claim="B-023" />
    /// <implements epic="0001" feature="F19" claim="B-027" />
    /// <implements epic="0001" feature="F19" claim="B-028" />
    public SnowflakeStrategy(JsonNode? settings, string type, IRandomSource random, IClock clock, string? key = null)
    {
        key ??= $"mint.{type}";
        if (settings is not null and not JsonObject)
        {
            throw new IdRefusedException($"'{key}.snowflake' is not an object.");
        }

        _epoch = settings?["epoch"] switch
        {
            null => DefaultEpoch,
            JsonValue value when value.TryGetValue<string>(out var text)
                                 && DateTimeOffset.TryParseExact(
                                     text, EpochFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var epoch)
                => epoch.ToUnixTimeMilliseconds(),
            _ => throw new IdRefusedException($"'{key}.snowflake.epoch' is not a date written {EpochFormat}."),
        };

        _datacenterBits = Width(settings, "datacenterBits", DefaultDatacenterBits, key);
        _workerBits = Width(settings, "workerBits", DefaultWorkerBits, key);
        _sequenceBits = Width(settings, "sequenceBits", DefaultSequenceBits, key);
        if (_datacenterBits + _workerBits + _sequenceBits > MaxFieldBits)
        {
            throw new IdRefusedException($"'{key}.snowflake' names widths that sum to more than {MaxFieldBits} bits, leaving no bit for the time.");
        }

        _namedDatacenter = Field(settings, "datacenter", _datacenterBits, key);
        _namedWorker = Field(settings, "worker", _workerBits, key);
        _sequenceStart = Field(settings, "sequenceStart", _sequenceBits, key);
        _type = type;
        _key = key;
        _random = random;
        _clock = clock;
    }

    /// <inheritdoc />
    /// <exception cref="IdRefusedException">
    /// The clock reads earlier than at the draw before it, or the milliseconds from the epoch do not fit the time field.
    /// </exception>
    /// <implements epic="0001" feature="F19" claim="B-007" />
    /// <implements epic="0001" feature="F19" claim="B-022" />
    /// <implements epic="0001" feature="F19" claim="B-023" />
    /// <implements epic="0001" feature="F19" claim="B-024" />
    /// <implements epic="0001" feature="F19" claim="B-025" />
    /// <implements epic="0001" feature="F19" claim="B-026" />
    /// <implements epic="0001" feature="F19" claim="B-029" />
    public string Next()
    {
        long elapsed;
        if (_previous is { } previous)
        {
            elapsed = Elapsed();
            if (elapsed < previous)
            {
                throw new IdRefusedException($"The clock read earlier at a later '{_type}' draw than at the draw before it.");
            }

            _sequence = (_sequence + 1) & ((1 << _sequenceBits) - 1);
        }
        else
        {
            _datacenter = _namedDatacenter ?? _random.Next(1 << _datacenterBits);
            _worker = _namedWorker ?? _random.Next(1 << _workerBits);
            _sequence = _sequenceStart ?? _random.Next(1 << _sequenceBits);
            elapsed = Elapsed();
        }

        var fieldBits = _datacenterBits + _workerBits + _sequenceBits;
        if (elapsed < 0 || elapsed >= 1L << (SignificantBits - fieldBits))
        {
            throw new IdRefusedException($"The milliseconds from '{_key}.snowflake.epoch' to the clock's reading do not fit the time field.");
        }

        _previous = elapsed;
        var number = (elapsed << fieldBits)
                     | ((long) _datacenter << (_workerBits + _sequenceBits))
                     | ((long) _worker << _sequenceBits)
                     | (long) _sequence;
        return number.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>The width a member names, or its default when it names none.</summary>
    /// <param name="settings">The sub-object, or nothing.</param>
    /// <param name="member">The width's member.</param>
    /// <param name="width">The default width.</param>
    /// <param name="key">The setting the entry is read from, which a refusal names.</param>
    /// <returns>The width.</returns>
    /// <exception cref="IdRefusedException">The member is not an integer from 0 to <see cref="MaxWidth"/>.</exception>
    /// <implements epic="0001" feature="F19" claim="B-023" />
    private static int Width(JsonNode? settings, string member, int width, string key) => settings?[member] switch
    {
        null => width,
        JsonValue value when value.TryGetValue<int>(out var number) && number is >= 0 and <= MaxWidth => number,
        _ => throw new IdRefusedException($"'{key}.snowflake.{member}' is not an integer from 0 to {MaxWidth}."),
    };

    /// <summary>The value a field's member names, or nothing when it names none or names <c>random</c> (ADR-0033, OQ-1).</summary>
    /// <param name="settings">The sub-object, or nothing.</param>
    /// <param name="member">The field's member.</param>
    /// <param name="width">The field's width.</param>
    /// <param name="key">The setting the entry is read from, which a refusal names.</param>
    /// <returns>The value, or <see langword="null"/> when it is drawn.</returns>
    /// <exception cref="IdRefusedException">The member is not <c>random</c> and not an integer the field's width holds.</exception>
    /// <implements epic="0001" feature="F19" claim="B-024" />
    /// <implements epic="0001" feature="F19" claim="B-025" />
    /// <implements epic="0001" feature="F19" claim="B-026" />
    /// <implements epic="0001" feature="F19" claim="B-027" />
    /// <implements epic="0001" feature="F19" claim="B-028" />
    private static int? Field(JsonNode? settings, string member, int width, string key) => settings?[member] switch
    {
        null => null,
        JsonValue value when value.TryGetValue<string>(out var text) && text == RandomValue => null,
        JsonValue value when value.TryGetValue<int>(out var number) && number >= 0 && number < 1 << width => number,
        _ => throw new IdRefusedException($"'{key}.snowflake.{member}' is neither 'random' nor an integer from 0 to below {1 << width}."),
    };

    /// <summary>The milliseconds from the epoch to the clock's reading.</summary>
    /// <returns>The milliseconds, negative when the clock reads before the epoch.</returns>
    /// <implements epic="0001" feature="F19" claim="B-007" />
    /// <implements epic="0001" feature="F19" claim="B-029" />
    private long Elapsed() => _clock.GetUtcNow().ToUnixTimeMilliseconds() - _epoch;

    private const string RandomValue = "random";
    private const string EpochFormat = "yyyy-MM-dd";
    private const int DefaultDatacenterBits = 5;
    private const int DefaultWorkerBits = 5;
    private const int DefaultSequenceBits = 12;
    private const int MaxWidth = 30;
    private const int SignificantBits = 63;
    private const int MaxFieldBits = SignificantBits - 1;

    /// <summary>The epoch of a type that names none: 2026-01-01 at midnight, universal time, in Unix milliseconds (ADR-0033).</summary>
    /// <implements epic="0001" feature="F19" claim="B-022" />
    private static readonly long DefaultEpoch = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();

    private readonly long _epoch;
    private readonly int _datacenterBits;
    private readonly int _workerBits;
    private readonly int _sequenceBits;
    private readonly int? _namedDatacenter;
    private readonly int? _namedWorker;
    private readonly int? _sequenceStart;
    private readonly string _type;
    private readonly string _key;
    private readonly IRandomSource _random;
    private readonly IClock _clock;
    private long? _previous;
    private int _datacenter;
    private int _worker;
    private int _sequence;
}
