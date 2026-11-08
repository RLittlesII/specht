using System.Text.Json.Nodes;

namespace Specht.IdGeneration;

/// <summary>
/// Mints one id for a type from that type's own entry under the manifest's <c>mint</c> key (<c>0001-F19</c> C-12), through
/// the strategy the entry names. It reads the manifest it is handed and writes nothing (C-1).
/// </summary>
/// <param name="random">The random source every strategy it builds draws from.</param>
/// <implements epic="0001" feature="F19" claim="B-008" />
public sealed class IdMinter(IRandomSource random)
{
    /// <summary>Builds the strategy <c>mint[type]</c> names from that entry alone and draws one id from it.</summary>
    /// <param name="manifest">The manifest document.</param>
    /// <param name="type">The id type, a member of <c>mint</c>.</param>
    /// <returns>The id.</returns>
    /// <exception cref="IdRefusedException">The type is not opted in, or its entry names no built strategy or settings no id can be drawn from.</exception>
    /// <implements epic="0001" feature="F19" claim="B-005" />
    /// <implements epic="0001" feature="F19" claim="B-006" />
    /// <implements epic="0001" feature="F19" claim="B-008" />
    public string Mint(JsonObject manifest, string type)
    {
        if (manifest["mint"] is not JsonObject mint || mint[type] is not JsonObject entry)
        {
            throw new IdRefusedException($"The type '{type}' is not opted in under 'mint'.");
        }

        IIdStrategy strategy = Text(entry["strategy"]) switch
        {
            "nanoid" => new NanoIdStrategy(NanoId(entry["nanoid"], type), random),
            null => throw new IdRefusedException($"'mint.{type}' names no strategy."),
            var other => throw new IdRefusedException($"'mint.{type}' names the strategy '{other}', which is not built."),
        };

        return strategy.Next();
    }

    /// <summary>Reads Nano ID's settings from the <c>nanoid</c> sub-object alone, filling an absent alphabet and size.</summary>
    /// <implements epic="0001" feature="F19" claim="B-019" />
    /// <implements epic="0001" feature="F19" claim="B-020" />
    private static NanoIdSettings NanoId(JsonNode? node, string type)
    {
        if (node is not null and not JsonObject)
        {
            throw new IdRefusedException($"'mint.{type}.nanoid' is not an object.");
        }

        var alphabet = node?["alphabet"] switch
        {
            null => NanoIdSettings.Base32,
            var value when Text(value) is { Length: > 0 } text => text,
            _ => throw new IdRefusedException($"'mint.{type}.nanoid.alphabet' is not a non-empty string."),
        };

        var size = node?["size"] switch
        {
            null => DefaultSize(type) ?? throw new IdRefusedException($"'mint.{type}.nanoid' names no size, and '{type}' has no default."),
            JsonValue value when value.TryGetValue<int>(out var number) && number > 0 => number,
            _ => throw new IdRefusedException($"'mint.{type}.nanoid.size' is not a positive integer."),
        };

        return new NanoIdSettings(alphabet, size);
    }

    /// <summary>The size of a type that names none: 8 for the repository-wide <c>adr</c> and <c>lesson</c> (ADR-0033).</summary>
    /// <implements epic="0001" feature="F19" claim="B-020" />
    private static int? DefaultSize(string type) => type is "adr" or "lesson" ? 8 : null;

    /// <summary>The node's string value, or nothing when it is not a string.</summary>
    /// <implements epic="0001" feature="F19" claim="B-008" />
    private static string? Text(JsonNode? node) => node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
}
