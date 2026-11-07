using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Specht.IdGeneration;

/// <summary>Composes a type's strategy and decorators from the manifest's <c>mint</c> key; a type it does not name gets nothing.</summary>
public static class IdMinter
{
    /// <summary>The id types a manifest can opt in.</summary>
    public static IReadOnlyList<string> Types { get; } =
        ["epic", "feature", "task", "claim", "constraint", "openQuestion", "decision", "adr", "lesson", "scenario"];

    /// <summary>Builds formatting, then validation, then the collision guard around the type's strategy.</summary>
    /// <param name="manifest">The manifest document.</param>
    /// <param name="type">The id type.</param>
    /// <param name="identity">Text a derived worker id hashes: the machine name or the root's path, by the setting.</param>
    /// <param name="clock">The clock.</param>
    /// <param name="random">The random source.</param>
    /// <param name="held">Whether the tree holds an id of the type.</param>
    /// <returns>The outermost decorator.</returns>
    /// <exception cref="IdRefusedException">The type is not opted in, has no grammar, or its settings are invalid.</exception>
    public static IIdGenerator Create(
        JsonObject manifest,
        string type,
        Func<string, string> identity,
        TimeProvider clock,
        IRandomSource random,
        Func<string, bool> held)
    {
        if (manifest["mint"] is not JsonObject mint || mint[type] is not JsonObject settings)
        {
            throw new IdRefusedException($"{type} is not opted in");
        }

        if (manifest["identifiers"]?[type]?.GetValue<string>() is not { } grammar)
        {
            throw new IdRefusedException($"{type} has no grammar in identifiers");
        }

        IIdGenerator strategy = settings["strategy"]?.GetValue<string>() switch
        {
            "nanoid" => new NanoIdGenerator(NanoId(settings["nanoid"] as JsonObject), random),
            "snowflake" => new SnowflakeGenerator(Snowflake(settings["snowflake"] as JsonObject, identity, random), clock, random),
            var other => throw new IdRefusedException($"{type} names no known strategy: {other}"),
        };

        var encoding = settings["encoding"]?.GetValue<string>() switch
        {
            null or "none" => IdEncoding.None,
            "base32" => IdEncoding.Base32,
            var other => throw new IdRefusedException($"unknown encoding: {other}"),
        };

        var formatted = new FormattingDecorator(strategy, settings["prefix"]?.GetValue<string>() ?? string.Empty, encoding);
        var validated = new ValidationDecorator(formatted, type, new Regex(grammar, RegexOptions.CultureInvariant));
        return new CollisionGuardDecorator(validated, held, settings["attempts"]?.GetValue<int>() ?? 8);
    }

    private static NanoIdSettings NanoId(JsonObject? settings)
    {
        if (settings?["alphabet"]?.GetValue<string>() is not { } alphabet || settings["size"]?.GetValue<int>() is not { } size)
        {
            throw new IdRefusedException("nanoid needs an alphabet and a size");
        }

        var nanoId = new NanoIdSettings(alphabet, size);
        nanoId.Validate();
        return nanoId;
    }

    private static SnowflakeSettings Snowflake(JsonObject? settings, Func<string, string> identity, IRandomSource random)
    {
        var supplied = SnowflakeSettings.Supplied;
        var epoch = settings?["epoch"]?.GetValue<string>() is { } date
            ? DateTimeOffset.Parse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUnixTimeMilliseconds()
            : supplied.EpochMilliseconds;
        var datacenterBits = settings?["datacenterBits"]?.GetValue<int>() ?? supplied.DatacenterBits;
        var workerBits = settings?["workerBits"]?.GetValue<int>() ?? supplied.WorkerBits;
        var sequenceBits = settings?["sequenceBits"]?.GetValue<int>() ?? supplied.SequenceBits;
        var start = settings?["sequenceStart"]?.GetValue<string>() == "random" ? SequenceStart.Random : SequenceStart.Zero;

        if (datacenterBits < 0 || workerBits < 0 || sequenceBits < 0 || datacenterBits + workerBits + sequenceBits > 30)
        {
            throw new IdRefusedException("snowflake fields must leave at least 33 bits of time");
        }

        return new SnowflakeSettings(
            epoch,
            datacenterBits,
            workerBits,
            sequenceBits,
            Field(settings?["datacenter"], datacenterBits, identity, random),
            Field(settings?["worker"], workerBits, identity, random),
            start);
    }

    private static int Field(JsonNode? value, int bits, Func<string, string> identity, IRandomSource random)
    {
        if (value is null)
        {
            return 0;
        }

        if (value.GetValueKind() == System.Text.Json.JsonValueKind.Number)
        {
            var number = value.GetValue<int>();
            return number >= 0 && number < 1 << bits ? number : throw new IdRefusedException("snowflake field value out of range");
        }

        return value.GetValue<string>() switch
        {
            "random" => random.Next(1 << bits),
            var source => SnowflakeSettings.Hash(identity(source), bits),
        };
    }
}
