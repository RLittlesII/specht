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
    /// Builds the strategy <c>mint[type]</c> names from that entry alone and draws one id from it, once every type <c>mint</c>
    /// names has a grammar in <c>identifiers</c>.
    /// </summary>
    /// <param name="manifest">The manifest document, its <c>identifiers</c> filled with the pin's defaults.</param>
    /// <param name="type">The id type, a member of <c>mint</c>.</param>
    /// <returns>The id.</returns>
    /// <exception cref="IdRefusedException">
    /// A type <c>mint</c> names has no grammar, the type is not opted in, its entry names no built strategy, or the strategy
    /// refuses its settings.
    /// </exception>
    /// <implements epic="0001" feature="F19" claim="B-001" />
    /// <implements epic="0001" feature="F19" claim="B-008" />
    /// <implements epic="0001" feature="F19" claim="B-043" />
    /// <implements epic="0001" feature="F19" claim="B-047" />
    /// <implements epic="0001" feature="F19" claim="B-048" />
    public string Mint(JsonObject manifest, string type)
    {
        var mint = manifest["mint"] as JsonObject;
        var grammars = manifest["identifiers"] as JsonObject;
        foreach (var (opted, _) in mint ?? new JsonObject())
        {
            if (grammars?[opted] is null)
            {
                throw new IdRefusedException($"'mint.{opted}' opts '{opted}' in, and 'identifiers' holds no grammar for it.");
            }
        }

        if (mint?[type] is not JsonObject entry)
        {
            throw new IdRefusedException($"The type '{type}' is not opted in under 'mint'.");
        }

        var name = entry["strategy"] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
        IIdStrategy strategy = name switch
        {
            "nanoid" => new NanoIdStrategy(entry["nanoid"], type, random),
            null => throw new IdRefusedException($"'mint.{type}' names no strategy."),
            _ => throw new IdRefusedException($"'mint.{type}' names the strategy '{name}', which is not built."),
        };

        return strategy.Next();
    }
}
