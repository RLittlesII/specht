using System.Text.Json.Nodes;

namespace Specht.Identifiers;

/// <summary>
/// Mints one id for a type from that type's own entry under the manifest's <c>mint</c> key (<c>0001-F19</c> C-12), through
/// the strategy the entry names. It reads the manifest it is handed and writes nothing (C-1).
/// </summary>
/// <param name="random">The random source every strategy it builds draws from.</param>
/// <implements epic="0001" feature="F19" claim="B-008" />
public sealed class Minter(IRandomSource random)
{
    /// <summary>
    /// Builds the strategy <c>mint[type]</c> names from that entry alone and draws one id from it, once every one of C-13's
    /// types <c>mint</c> opts in has a grammar in <c>identifiers</c>. A member opts its type in when it is an object whose
    /// <c>strategy</c> is a string.
    /// </summary>
    /// <param name="manifest">The manifest document, its <c>identifiers</c> filled with the pin's defaults.</param>
    /// <param name="type">The id type, a member of <c>mint</c>.</param>
    /// <returns>The id.</returns>
    /// <exception cref="IdRefusedException">
    /// One of C-13's types <c>mint</c> opts in has no grammar, the type is not opted in or its entry names no strategy (an
    /// <see cref="IdNotOptedInException"/>), its entry names a strategy that is not built, or the strategy refuses its settings.
    /// </exception>
    /// <implements epic="0001" feature="F19" claim="B-001" />
    /// <implements epic="0001" feature="F19" claim="B-002" />
    /// <implements epic="0001" feature="F19" claim="B-008" />
    /// <implements epic="0001" feature="F19" claim="B-043" />
    /// <implements epic="0001" feature="F19" claim="B-047" />
    /// <implements epic="0001" feature="F19" claim="B-048" />
    public string Mint(JsonObject manifest, string type)
    {
        var mint = manifest["mint"] as JsonObject;
        var grammars = manifest["identifiers"] as JsonObject;
        foreach (var opted in Types)
        {
            if (StrategyOf(mint?[opted]) is not null && grammars?[opted] is null)
            {
                throw new IdRefusedException($"'mint.{opted}' opts '{opted}' in, and 'identifiers' holds no grammar for it.");
            }
        }

        if (mint?[type] is not JsonObject entry)
        {
            throw new IdNotOptedInException($"The type '{type}' is not opted in under 'mint'.");
        }

        var name = StrategyOf(entry);
        IIdStrategy strategy = name switch
        {
            "nanoid" => new NanoIdStrategy(entry["nanoid"], type, random),
            null => throw new IdNotOptedInException($"'mint.{type}' names no strategy, so '{type}' is not opted in."),
            _ => throw new IdRefusedException($"'mint.{type}' names the strategy '{name}', which is not built."),
        };

        return strategy.Next();
    }

    /// <summary>The strategy a <c>mint</c> member names: its <c>strategy</c> when the member is an object and that is a string.</summary>
    /// <param name="member">The member's value.</param>
    /// <returns>The strategy name, or <see langword="null"/> when the member names none.</returns>
    /// <implements epic="0001" feature="F19" claim="B-043" />
    /// <implements epic="0001" feature="F19" claim="B-047" />
    private static string? StrategyOf(JsonNode? member) =>
        member is JsonObject entry && entry["strategy"] is JsonValue value && value.TryGetValue<string>(out var name) ? name : null;

    /// <summary>The types of the first slice (C-13), the members of <c>mint</c> whose grammar B-043 requires once they opt in.</summary>
    /// <implements epic="0001" feature="F19" claim="B-043" />
    private static readonly string[] Types = ["adr", "lesson", "decision", "claim", "constraint", "openQuestion", "scenario"];
}
