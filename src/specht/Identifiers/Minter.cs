using System.Text.Json.Nodes;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Model;

namespace Specht.Identifiers;

/// <summary>
/// Mints one id for a type from that type's own settings, its <c>mint</c> entry below schema 0.4.0 and its <c>ids</c> record's
/// <c>mint</c> from it (<c>0001-F19</c> C-12, C-21), through the strategy the settings name, inside the formatting decorator,
/// the validation decorator and the collision guard (C-4). It reads the manifest it is handed, the names the tree holds and the
/// clock, and writes nothing (C-1, C-11).
/// </summary>
/// <param name="random">The random source every strategy it builds draws from.</param>
/// <param name="held">Where the guard reads the ids each type already holds in the tree.</param>
/// <param name="clock">The clock the Snowflake strategy reads.</param>
/// <implements epic="0001" feature="F19" claim="B-008" />
/// <implements epic="0001" feature="F19" claim="B-034" />
public sealed class Minter(IRandomSource random, HeldIds held, IClock clock)
{
    /// <summary>
    /// Mints one id for <paramref name="type"/>: below schema 0.4.0 from the manifest's <c>mint</c> and <c>identifiers</c>
    /// members, and from it from the type's <c>ids</c> record, whose <c>mint</c> names the settings and whose head and grammar the
    /// id takes for the claim prefix the Feature declares (A-9, C-19, C-21).
    /// </summary>
    /// <param name="manifest">The manifest: its structure, and the effective document the settings are read from.</param>
    /// <param name="type">The id type.</param>
    /// <param name="root">The repository root the guard reads held ids under.</param>
    /// <param name="feature">The folder, relative to the root, of the Feature a per-Feature id is minted for, or nothing.</param>
    /// <returns>The id.</returns>
    /// <exception cref="IdRefusedException">
    /// The settings mint no id (an <see cref="IdNotOptedInException"/> where the type is not opted in), the request names no
    /// Feature (an <see cref="IdFeatureNotNamedException"/>) or a folder that holds none (an
    /// <see cref="IdFeatureNotFoundException"/>), a candidate is refused, or the tree holds every candidate the guard draws.
    /// </exception>
    /// <implements epic="0001" feature="F19" claim="B-001" />
    /// <implements epic="0001" feature="F19" claim="B-002" />
    /// <implements epic="0001" feature="F19" claim="B-007" />
    /// <implements epic="0001" feature="F19" claim="B-008" />
    /// <implements epic="0001" feature="F19" claim="B-009" />
    /// <implements epic="0001" feature="F19" claim="B-010" />
    /// <implements epic="0001" feature="F19" claim="B-011" />
    /// <implements epic="0001" feature="F19" claim="B-012" />
    /// <implements epic="0001" feature="F19" claim="B-018" />
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
    /// <implements epic="0001" feature="F19" claim="B-061" />
    /// <implements epic="0001" feature="F19" claim="B-063" />
    /// <implements epic="0001" feature="F19" claim="B-065" />
    /// <implements epic="0001" feature="F19" claim="B-066" />
    /// <implements epic="0001" feature="F19" claim="B-067" />
    /// <implements epic="0001" feature="F19" claim="B-068" />
    /// <implements epic="0001" feature="F19" claim="B-070" />
    /// <implements epic="0001" feature="F19" claim="B-071" />
    /// <implements epic="0001" feature="F19" claim="B-072" />
    /// <implements epic="0001" feature="F19" claim="B-073" />
    /// <implements epic="0001" feature="F19" claim="B-074" />
    /// <implements epic="0001" feature="F19" claim="B-075" />
    /// <implements epic="0001" feature="F19" claim="B-076" />
    /// <implements epic="0001" feature="F19" claim="B-077" />
    public string Mint(SpechtManifestDocument manifest, string type, string root, string? feature) =>
        manifest.Structure.Ids is { } records
            ? MintFromRecord(manifest.Structure, records, manifest.Document, type, root, feature)
            : MintBelowIds(manifest.Document, type, root, manifest.Structure.Discovery, feature);

    /// <summary>The strategy a <c>mint</c> member names: its <c>strategy</c> when the member is an object and that is a string.</summary>
    /// <param name="member">The member's value.</param>
    /// <returns>The strategy name, or <see langword="null"/> when the member names none.</returns>
    /// <implements epic="0001" feature="F19" claim="B-043" />
    /// <implements epic="0001" feature="F19" claim="B-047" />
    private static string? StrategyOf(JsonNode? member) =>
        member is JsonObject entry && entry["strategy"] is JsonValue value && value.TryGetValue<string>(out var name) ? name : null;

    /// <summary>A record's <c>mint</c> member, when the record and the member are objects.</summary>
    /// <implements epic="0001" feature="F19" claim="B-075" />
    private static JsonObject? MintOf(JsonObject? ids, string type) =>
        ids?[type] is JsonObject record && record["mint"] is JsonObject entry ? entry : null;

    /// <summary>
    /// The head a candidate must not start with (B-076, B-077): the scenario head for a claim, and the claim head for a scenario,
    /// where that head is the longer; nothing otherwise.
    /// </summary>
    /// <implements epic="0001" feature="F19" claim="B-076" />
    /// <implements epic="0001" feature="F19" claim="B-077" />
    private static string? Reserved(string type, SpechtClaimGrammar heads) => type switch
    {
        "claim" when heads.LongerScenarioHead => heads.ScenarioHead,
        "scenario" when heads.Head is { } head && heads.ScenarioHead is { } scenario && head.Length > scenario.Length => head,
        _ => null,
    };

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
    /// One of C-13's types <c>mint</c> opts in has no grammar, <c>mint</c> opts in <c>epic</c>, <c>feature</c> or
    /// <c>task</c>, the type is not opted in or its entry names no strategy (an
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
    /// <implements epic="0001" feature="F19" claim="B-018" />
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
    /// <implements epic="0001" feature="F19" claim="B-061" />
    private string MintBelowIds(JsonObject manifest, string type, string root, SpechtDiscoveryInputs discovery, string? feature)
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

        foreach (var refused in NotMinted)
        {
            if (StrategyOf(mint?[refused]) is not null)
            {
                throw new IdRefusedException($"'mint.{refused}' opts '{refused}' in, and no '{refused}' id is minted under the pinned schema version.");
            }
        }

        if (mint?[type] is not JsonObject entry)
        {
            throw new IdNotOptedInException($"The type '{type}' is not opted in under 'mint'.");
        }

        var key = $"mint.{type}";
        var name = StrategyOf(entry) ?? throw new IdNotOptedInException($"'{key}' names no strategy, so '{type}' is not opted in.");
        var location = FeatureOf(type, root, discovery, feature);
        var strategy = StrategyNamed(name, entry, type, key);
        var formatted = new FormattingDecorator(strategy, entry["prefix"], entry["encoding"], name, type, key);
        var validated = new ValidationDecorator(
            formatted, grammars?[type]?.GetValue<string>(), $"identifiers.{type}", type, null, StringComparison.Ordinal);
        return new CollisionGuardDecorator(validated, entry["attempts"], type, held.Of(type, root, discovery, location), key).Next();
    }

    /// <summary>
    /// Mints from the type's <c>ids</c> record once no record's <c>mint</c> names a <c>prefix</c> (B-075): the strategy its
    /// <c>mint</c> names, its head for the Feature's claim prefix ahead of each draw, its grammar and the other type's longer
    /// head as validation, and the guard over the headed candidate (B-063, B-065 to B-068, B-070 to B-077).
    /// </summary>
    /// <param name="structure">The manifest's structure, which holds the records, the Feature keys and the claim tag case setting.</param>
    /// <param name="records">The seven effective records.</param>
    /// <param name="manifest">The effective document, whose records' <c>mint</c> members hold the settings.</param>
    /// <param name="type">The id type.</param>
    /// <param name="root">The repository root.</param>
    /// <param name="feature">The folder of the Feature a per-Feature id is minted for, or nothing.</param>
    /// <returns>The id.</returns>
    /// <exception cref="IdRefusedException">The request mints no id.</exception>
    /// <implements epic="0001" feature="F19" claim="B-063" />
    /// <implements epic="0001" feature="F19" claim="B-065" />
    /// <implements epic="0001" feature="F19" claim="B-066" />
    /// <implements epic="0001" feature="F19" claim="B-067" />
    /// <implements epic="0001" feature="F19" claim="B-068" />
    /// <implements epic="0001" feature="F19" claim="B-070" />
    /// <implements epic="0001" feature="F19" claim="B-071" />
    /// <implements epic="0001" feature="F19" claim="B-072" />
    /// <implements epic="0001" feature="F19" claim="B-073" />
    /// <implements epic="0001" feature="F19" claim="B-074" />
    /// <implements epic="0001" feature="F19" claim="B-075" />
    /// <implements epic="0001" feature="F19" claim="B-076" />
    /// <implements epic="0001" feature="F19" claim="B-077" />
    private string MintFromRecord(
        SpechtStructure structure, IReadOnlyDictionary<string, SpechtIdRecord> records, JsonObject manifest, string type, string root, string? feature)
    {
        var ids = manifest["ids"] as JsonObject;
        foreach (var recorded in Types)
        {
            if (MintOf(ids, recorded) is { } optIn && optIn.ContainsKey("prefix"))
            {
                throw new IdRefusedException(
                    $"'ids.{recorded}.mint.prefix' names a prefix, and a '{recorded}' id takes its head from the record's 'plain' or 'form'.");
            }
        }

        var key = $"ids.{type}.mint";
        var entry = MintOf(ids, type) ?? throw new IdNotOptedInException($"The type '{type}' is not opted in under '{key}'.");
        var name = StrategyOf(entry) ?? throw new IdNotOptedInException($"'{key}' names no strategy, so '{type}' is not opted in.");
        var location = FeatureOf(type, root, structure.Discovery, feature);
        var strategy = StrategyNamed(name, entry, type, key);
        var heads = structure.ClaimGrammar(location is null ? null : held.ClaimPrefixOf(root, location, structure.FeatureKeys.ClaimPrefix));
        var record = records[type];
        var formatted = new FormattingDecorator(strategy, JsonValue.Create(record.Head(heads.Prefix)), entry["encoding"], name, type, key);
        var validated = new ValidationDecorator(
            formatted, record.Grammar(heads.Prefix), $"ids.{type}.id", type, Reserved(type, heads), structure.TagComparison);
        return new CollisionGuardDecorator(validated, entry["attempts"], type, held.Of(type, root, structure.Discovery, location), key).Next();
    }

    /// <summary>The Feature a per-Feature type is minted for, as the check lists it; nothing for a repository-wide type.</summary>
    /// <exception cref="IdFeatureNotNamedException">The type is per-Feature and the request names no Feature.</exception>
    /// <exception cref="IdFeatureNotFoundException">The folder named holds no Feature.</exception>
    /// <implements epic="0001" feature="F19" claim="B-057" />
    /// <implements epic="0001" feature="F19" claim="B-058" />
    /// <implements epic="0001" feature="F19" claim="B-059" />
    /// <implements epic="0001" feature="F19" claim="B-060" />
    private SpechtLocation? FeatureOf(string type, string root, SpechtDiscoveryInputs discovery, string? feature) =>
        !IdTypes.PerFeature.Contains(type) ? null
        : feature is null ? throw new IdFeatureNotNamedException($"A '{type}' id is minted for a Feature, and the request names none.")
        : held.FeatureAt(root, discovery, feature)
            ?? throw new IdFeatureNotFoundException($"The folder named for the '{type}' id holds no Feature's '.spec/'.");

    /// <summary>The strategy an entry names, built from that entry's settings alone.</summary>
    /// <exception cref="IdRefusedException">The name is neither <c>nanoid</c> nor <c>snowflake</c>, or the strategy refuses its settings.</exception>
    /// <implements epic="0001" feature="F19" claim="B-005" />
    /// <implements epic="0001" feature="F19" claim="B-007" />
    /// <implements epic="0001" feature="F19" claim="B-010" />
    private IIdStrategy StrategyNamed(string name, JsonObject entry, string type, string key) => name switch
    {
        "nanoid" => new NanoIdStrategy(entry["nanoid"], type, random, key),
        "snowflake" => new SnowflakeStrategy(entry["snowflake"], type, random, clock, key),
        _ => throw new IdRefusedException($"'{key}' names the strategy '{name}', which is neither 'nanoid' nor 'snowflake'."),
    };

    /// <summary>The types of the first slice (C-13), the members of <c>mint</c> whose grammar B-043 requires once they opt in.</summary>
    /// <implements epic="0001" feature="F19" claim="B-043" />
    private static readonly string[] Types = [.. IdTypes.RepositoryWide, .. IdTypes.PerFeature];

    /// <summary>The types C-13 leaves out, the members of <c>mint</c> that refuse every request once they opt in (B-061).</summary>
    /// <implements epic="0001" feature="F19" claim="B-018" />
    /// <implements epic="0001" feature="F19" claim="B-061" />
    private static readonly string[] NotMinted = ["epic", "feature", "task"];
}
