using System.Text.Json.Nodes;
using Specht.Discovery;

namespace Specht.Identifiers;

/// <summary>
/// Mints one id for a type from that type's own entry under the manifest's <c>mint</c> key (<c>0001-F19</c> C-12), through
/// the strategy the entry names, inside the formatting decorator, the validation decorator and the collision guard (C-4). It
/// reads the manifest it is handed, the names the tree holds and the clock, and writes nothing (C-1, C-11).
/// </summary>
/// <param name="random">The random source every strategy it builds draws from.</param>
/// <param name="held">Where the guard reads the ids each type already holds in the tree.</param>
/// <param name="clock">The clock the Snowflake strategy reads.</param>
/// <implements epic="0001" feature="F19" claim="B-008" />
/// <implements epic="0001" feature="F19" claim="B-034" />
public sealed class Minter(IRandomSource random, HeldIds held, IClock clock)
{
    /// <summary>
    /// Builds the strategy <c>mint[type]</c> names from that entry alone and draws one id from it, once every one of C-13's
    /// types <c>mint</c> opts in has a grammar in <c>identifiers</c>. A member opts its type in when it is an object whose
    /// <c>strategy</c> is a string. The formatting decorator starts each candidate with the entry's prefix, the validation
    /// decorator refuses one the type's <c>identifiers</c> grammar does not match, and the guard draws again while the tree under
    /// <paramref name="root"/> holds the formatted candidate.
    /// </summary>
    /// <param name="manifest">The manifest document, its <c>identifiers</c> filled with the pin's defaults.</param>
    /// <param name="type">The id type, a member of <c>mint</c>.</param>
    /// <param name="root">The repository root the guard reads held ids under.</param>
    /// <param name="discovery">The manifest's discovery inputs, under which the guard lists the places it reads.</param>
    /// <param name="feature">The folder, relative to the root, of the Feature a per-Feature id is minted for, or nothing.</param>
    /// <returns>The id.</returns>
    /// <exception cref="IdRefusedException">
    /// One of C-13's types <c>mint</c> opts in has no grammar, the type is not opted in or its entry names no strategy (an
    /// <see cref="IdNotOptedInException"/>), its entry names a strategy other than <c>nanoid</c> or <c>snowflake</c>, the
    /// type is per-Feature and the request names no Feature (an <see cref="IdFeatureNotNamedException"/>) or a folder that
    /// holds none (an <see cref="IdFeatureNotFoundException"/>), the strategy refuses its settings or its clock reads earlier
    /// at a later draw, formatting refuses its <c>prefix</c> or
    /// <c>encoding</c>, the type's <c>identifiers</c> grammar does not match a candidate drawn, the guard refuses its
    /// <c>attempts</c>, or the tree holds every candidate the guard draws.
    /// </exception>
    /// <implements epic="0001" feature="F19" claim="B-001" />
    /// <implements epic="0001" feature="F19" claim="B-002" />
    /// <implements epic="0001" feature="F19" claim="B-007" />
    /// <implements epic="0001" feature="F19" claim="B-008" />
    /// <implements epic="0001" feature="F19" claim="B-009" />
    /// <implements epic="0001" feature="F19" claim="B-010" />
    /// <implements epic="0001" feature="F19" claim="B-011" />
    /// <implements epic="0001" feature="F19" claim="B-012" />
    /// <implements epic="0001" feature="F19" claim="B-029" />
    /// <implements epic="0001" feature="F19" claim="B-033" />
    /// <implements epic="0001" feature="F19" claim="B-034" />
    /// <implements epic="0001" feature="F19" claim="B-035" />
    /// <implements epic="0001" feature="F19" claim="B-036" />
    /// <implements epic="0001" feature="F19" claim="B-037" />
    /// <implements epic="0001" feature="F19" claim="B-038" />
    /// <implements epic="0001" feature="F19" claim="B-039" />
    /// <implements epic="0001" feature="F19" claim="B-040" />
    /// <implements epic="0001" feature="F19" claim="B-041" />
    /// <implements epic="0001" feature="F19" claim="B-042" />
    /// <implements epic="0001" feature="F19" claim="B-043" />
    /// <implements epic="0001" feature="F19" claim="B-047" />
    /// <implements epic="0001" feature="F19" claim="B-048" />
    /// <implements epic="0001" feature="F19" claim="B-053" />
    /// <implements epic="0001" feature="F19" claim="B-054" />
    /// <implements epic="0001" feature="F19" claim="B-055" />
    /// <implements epic="0001" feature="F19" claim="B-056" />
    /// <implements epic="0001" feature="F19" claim="B-057" />
    /// <implements epic="0001" feature="F19" claim="B-058" />
    /// <implements epic="0001" feature="F19" claim="B-059" />
    /// <implements epic="0001" feature="F19" claim="B-060" />
    public string Mint(JsonObject manifest, string type, string root, SpechtDiscoveryInputs discovery, string? feature)
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

        var name = StrategyOf(entry) ?? throw new IdNotOptedInException($"'mint.{type}' names no strategy, so '{type}' is not opted in.");
        var location = !IdTypes.PerFeature.Contains(type) ? null
            : feature is null ? throw new IdFeatureNotNamedException($"A '{type}' id is minted for a Feature, and the request names none.")
            : held.FeatureAt(root, discovery, feature)
                ?? throw new IdFeatureNotFoundException($"The folder named for the '{type}' id holds no Feature's '.spec/'.");
        IIdStrategy strategy = name switch
        {
            "nanoid" => new NanoIdStrategy(entry["nanoid"], type, random),
            "snowflake" => new SnowflakeStrategy(entry["snowflake"], type, random, clock),
            _ => throw new IdRefusedException($"'mint.{type}' names the strategy '{name}', which is neither 'nanoid' nor 'snowflake'."),
        };

        var formatted = new FormattingDecorator(strategy, entry["prefix"], entry["encoding"], name, type);
        var validated = new ValidationDecorator(formatted, grammars?[type], type);
        return new CollisionGuardDecorator(validated, entry["attempts"], type, held.Of(type, root, discovery, location)).Next();
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
    private static readonly string[] Types = [.. IdTypes.RepositoryWide, .. IdTypes.PerFeature];
}
