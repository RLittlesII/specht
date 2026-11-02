using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Acceptance;

/// <summary>
/// The id a rule is reported under at a pin (<c>0001-F7</c> B-151, B-152; ADR-0029). Step code names a rule by its key,
/// the three-digit id the rule states, and <see cref="Under(string?, string)"/> gives the four-digit id ADR-0029's map
/// pairs it with under schema 0.3.0 or later, or the key itself under 0.1.0, 0.2.0 and 0.2.1. A scenario names a rule
/// by the id it is reported under, and <see cref="Named(string?, string)"/> reads it: from 0.3.0 on a three-digit id
/// fails the step, since no rule is reported under one; below 0.3.0 a four-digit id is read as its three-digit id, as
/// ADR-0029 reads a claim that names one under those pins. A manifest with no <c>schemaVersion</c> pins 0.1.0. The map
/// is the acceptance project's own copy, since the unit tier's data is not linked here.
/// </summary>
internal static class RuleIds
{
    /// <summary>The id the rule whose key is <paramref name="key"/> is reported under at <paramref name="pin"/>.</summary>
    /// <param name="pin">The manifest's <c>schemaVersion</c>, null when it has none.</param>
    /// <param name="key">The three-digit id a rule states; an id the map does not hold is returned as given.</param>
    /// <returns>The four-digit id from 0.3.0 on, the key below it.</returns>
    public static string Under(string? pin, string key) => FourDigitAt(pin) && FourDigit.TryGetValue(key, out var id) ? id : key;

    /// <summary>The id the rule whose key is <paramref name="key"/> is reported under at the pin <paramref name="manifest"/> carries.</summary>
    /// <param name="manifest">A manifest.</param>
    /// <param name="key">The three-digit id a rule states.</param>
    /// <returns>The id at that pin.</returns>
    public static string Under(JsonObject manifest, string key) => Under(manifest["schemaVersion"]?.GetValue<string>(), key);

    /// <summary>The id the rule whose key is <paramref name="key"/> is reported under at the pin of the manifest <paramref name="tree"/> holds.</summary>
    /// <param name="tree">A tree whose manifest is written.</param>
    /// <param name="key">The three-digit id a rule states.</param>
    /// <returns>The id at that pin.</returns>
    public static string Under(SpechtTree tree, string key) => Under(Pin(tree), key);

    /// <summary>The id a scenario names, <paramref name="id"/>, read at <paramref name="pin"/>.</summary>
    /// <param name="pin">The manifest's <c>schemaVersion</c>, null when it has none.</param>
    /// <param name="id">The rule id the scenario's text names.</param>
    /// <returns><paramref name="id"/> from 0.3.0 on; below it, the three-digit id of a four-digit one.</returns>
    public static string Named(string? pin, string id)
    {
        if (!FourDigitAt(pin))
        {
            return ThreeDigit.TryGetValue(id, out var key) ? key : id;
        }

        Regex.IsMatch(id, "^SPEC[0-9]{3}$").Should().BeFalse(
            "a scenario under schema {0} names a rule by the four-digit id it is reported under, not {1}", pin, id);
        return id;
    }

    /// <summary>The id a scenario names, <paramref name="id"/>, read at the pin of the manifest <paramref name="tree"/> holds.</summary>
    /// <param name="tree">A tree whose manifest is written.</param>
    /// <param name="id">The rule id the scenario's text names.</param>
    /// <returns>The id at that pin.</returns>
    public static string Named(SpechtTree tree, string id) => Named(Pin(tree), id);

    private static string? Pin(SpechtTree tree) =>
        JsonNode.Parse(File.ReadAllText(Path.Combine(tree.Root, SpechtTree.ManifestPath)))!["schemaVersion"]?.GetValue<string>();

    private static bool FourDigitAt(string? pin) => SemanticVersion.TryParse(pin, out var version) && version.CompareTo(FourDigitFrom) >= 0;

    private static readonly SemanticVersion FourDigitFrom = new(0, 3, 0);

    private static readonly Dictionary<string, string> FourDigit = new(StringComparer.Ordinal)
    {
        ["SPEC001"] = "SPEC1001",
        ["SPEC002"] = "SPEC1002",
        ["SPEC003"] = "SPEC1003",
        ["SPEC004"] = "SPEC1004",
        ["SPEC010"] = "SPEC2001",
        ["SPEC013"] = "SPEC2002",
        ["SPEC011"] = "SPEC3001",
        ["SPEC012"] = "SPEC3002",
        ["SPEC020"] = "SPEC4001",
        ["SPEC021"] = "SPEC4002",
        ["SPEC022"] = "SPEC4007",
        ["SPEC023"] = "SPEC4008",
        ["SPEC030"] = "SPEC4003",
        ["SPEC031"] = "SPEC4004",
        ["SPEC032"] = "SPEC4009",
        ["SPEC033"] = "SPEC4010",
        ["SPEC034"] = "SPEC4005",
        ["SPEC035"] = "SPEC4006",
        ["SPEC040"] = "SPEC5001",
        ["SPEC041"] = "SPEC5002",
        ["SPEC043"] = "SPEC5003",
        ["SPEC044"] = "SPEC5004",
        ["SPEC050"] = "SPEC6001",
        ["SPEC051"] = "SPEC6002",
        ["SPEC052"] = "SPEC6003",
        ["SPEC060"] = "SPEC7001",
        ["SPEC061"] = "SPEC7002",
    };

    private static readonly Dictionary<string, string> ThreeDigit =
        FourDigit.ToDictionary(static pair => pair.Value, static pair => pair.Key, StringComparer.Ordinal);
}
