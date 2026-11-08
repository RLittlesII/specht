using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Specht.Model;
using Specht.Tool.Features.Evidence.Contract;

namespace Specht.Tool.Features.Evidence;

/// <implements epic="0201" feature="F5" claim="B-011" />
internal sealed class EvidencePolicy
{
    /// <implements epic="0201" feature="F5" claim="B-011" />
    private EvidencePolicy(JsonObject projected)
    {
        var policy = projected["policy"] as JsonObject ?? throw new EvidenceConfigurationException("Policy is missing.");
        var identifiers = projected["identifiers"] as JsonObject ?? throw new EvidenceConfigurationException("Identifiers are missing.");
        _identifiers = identifiers;
        HasEpic = identifiers.ContainsKey("epic");
        var records = policy["records"];
        if (records?["claimSource"] is { } source)
        {
            ClaimTags = NativeTags(source, projected["claim"], records, policy["scenarioScopes"], identifiers);
            ClaimFileSets = FileSets(records["claimFileSets"]);
        }

        ClaimsSection = projected["claimsSection"]?.GetValue<string>()
            ?? (ClaimTags is null ? throw new EvidenceConfigurationException("Claims section is missing.") : string.Empty);
        FeatureKey = records?["featureIdentityKey"]?.GetValue<string>() ?? "id";
        EpicKey = records?["featureEpicKey"]?.GetValue<string>() ?? "epic";
        FeatureLifecycleKey = records?["featureLifecycleKey"]?.GetValue<string>() ?? "spec_status";
        ClaimIdColumn = records?["claimIdColumn"]?.GetValue<string>() ?? "ID";
        ClaimStatusColumn = records?["claimStatusColumn"]?.GetValue<string>() ?? "Status";
        ApprovedMarker = projected["markers"]?["approved"]?.GetValue<string>() ?? "approved";
        WithdrawnMarker = projected["markers"]?["withdrawn"]?.GetValue<string>() ?? "Withdrawn";
        SupersededMarker = projected["markers"]?["superseded"]?.GetValue<string>() ?? "Superseded";
        var verified = (policy["enforcement"] as JsonArray ?? new JsonArray())
            .Where(static entry => entry?["role"]?.GetValue<string>() == "verifies")
            .Select(static entry => entry!["project"]?.GetValue<string>()).ToArray();
        Projects = (policy["projects"] as JsonArray ?? throw new EvidenceConfigurationException("Projects are missing."))
            .Select(item => new ProjectEntry(item!["path"]!.GetValue<string>(), item["targetFrameworks"]!.AsArray()
                .Select(static framework => framework!.GetValue<string>()).ToArray(),
                item["requiredCaseScope"] is not null || verified.Contains(item["path"]!.GetValue<string>(), StringComparer.Ordinal))).ToArray();
        Features = (policy["features"] as JsonArray ?? throw new EvidenceConfigurationException("Features are missing."))
            .Select(item => new FeatureEntry(HasEpic ? item!["epic"]?.GetValue<string>() : null,
                item!["feature"]!.GetValue<string>())).ToArray();
        _policy = policy;
        if (Projects.Any(static project => project.Frameworks.Length == 0
                || project.Frameworks.Distinct(StringComparer.Ordinal).Count() != project.Frameworks.Length)
            || Projects.Select(static project => project.Path).Distinct(StringComparer.Ordinal).Count() != Projects.Length
            || Projects.Any(static project => !IsRelative(project.Path))
            || Features.Any(feature => !Matches("feature", feature.Feature) || HasEpic && (feature.Epic is null || !Matches("epic", feature.Epic)))
            || Features.Distinct().Count() != Features.Length
            || ClaimFileSets.Any(set => !IsRelative(set.Glob) || set.Record is not null && !IsRelative(set.Record)
                || !Matches("feature", set.Feature) || HasEpic != (set.Epic is not null) || set.Epic is not null && !Matches("epic", set.Epic))
            || ClaimFileSets.DistinctBy(static set => (set.Epic, set.Feature)).Count() != ClaimFileSets.Length
            || !identifiers.ContainsKey("claim") || policy["requiredLinks"]?["test"]?.GetValue<bool>() == false)
        {
            throw new EvidenceConfigurationException("Analysis policy is invalid.");
        }

        RequiredKinds = new[] { "test", "implementation", "scenario" }
            .Where(kind => kind == "test" || policy["requiredLinks"]?[kind]?.GetValue<bool>() == true).ToArray();
        foreach (var exemption in policy["exemptions"] as JsonArray ?? new JsonArray())
        {
            var reason = exemption?["reason"]?.GetValue<string>();
            var target = exemption?["target"];
            if (string.IsNullOrWhiteSpace(reason) || exemption?["links"] is not JsonArray links || links.Count == 0
                || links.Any(static link => link?.GetValue<string>() is not ("test" or "implementation" or "scenario"))
                || target is null)
            {
                throw new EvidenceConfigurationException("A reasoned exemption must name supported links and a target.");
            }

            if (target["kind"] is null)
            {
                var feature = target["feature"]?.GetValue<string>();
                var claim = target["claim"]?.GetValue<string>();
                var epic = target["epic"]?.GetValue<string>();
                if (feature is null || claim is null || !Matches("feature", feature) || !Matches("claim", claim)
                    || HasEpic && (epic is null || !Matches("epic", epic)) || !HasEpic && epic is not null)
                {
                    throw new EvidenceConfigurationException("An exemption claim identity is invalid.");
                }
            }
            else if (target["kind"]?.GetValue<string>() != "declaration"
                || !Projects.Any(project => project.Path == target["project"]?.GetValue<string>())
                || string.IsNullOrWhiteSpace(target["symbolId"]?.GetValue<string>()))
            {
                throw new EvidenceConfigurationException("An exemption declaration identity is invalid.");
            }
        }
    }

    public ProjectEntry[] Projects { get; }
    public FeatureEntry[] Features { get; }
    public string[] RequiredKinds { get; }
    public bool HasEpic { get; }
    public string ClaimsSection { get; }
    public string FeatureKey { get; }
    public string EpicKey { get; }
    public string FeatureLifecycleKey { get; }
    public string ClaimIdColumn { get; }
    public string ClaimStatusColumn { get; }
    public string ApprovedMarker { get; }
    public string WithdrawnMarker { get; }
    public string SupersededMarker { get; }
    public GherkinClaimTags? ClaimTags { get; }
    public ClaimFileSet[] ClaimFileSets { get; } = [];

    /// <implements epic="0201" feature="F5" claim="B-011" />
    public static EvidencePolicy Read(JsonObject configuration)
    {
        var analysis = configuration["codeAnalysis"] as JsonObject ?? throw new EvidenceConfigurationException("codeAnalysis is missing.");
        var mappings = analysis["mappings"] as JsonObject ?? throw new EvidenceConfigurationException("Explicit mappings are missing.");
        var fields = mappings.ToDictionary(static entry => entry.Key,
            static entry => entry.Value?.GetValue<string>() ?? throw new EvidenceConfigurationException("A mapping is empty."),
            StringComparer.Ordinal);
        var projected = new AnalysisConfigurationReader().Read(configuration, fields);

        projected["policy"] ??= analysis.DeepClone();
        return new EvidencePolicy(projected);
    }

    /// <implements epic="0201" feature="F5" claim="B-011" />
    public bool Matches(string role, string value) => _identifiers[role]?.GetValue<string>() is { } grammar
        && Regex.IsMatch(value, "\\A(?:" + grammar + ")\\z", RegexOptions.CultureInvariant);

    /// <implements epic="0201" feature="F5" claim="B-011" />
    public string? Exemption(string? epic, string feature, string claim, string kind) =>
        (_policy["exemptions"] as JsonArray ?? new JsonArray()).Where(item => item?["target"]?["kind"] is null
            && item?["target"]?["epic"]?.GetValue<string>() == epic
            && item?["target"]?["feature"]?.GetValue<string>() == feature
            && item?["target"]?["claim"]?.GetValue<string>() == claim
            && item?["links"]?.AsArray().Any(link => link?.GetValue<string>() == kind) == true)
            .Select(static item => item?["reason"]?.GetValue<string>()).FirstOrDefault();

    /// <implements epic="0201" feature="F5" claim="B-036" />
    public sealed record ProjectEntry(string Path, string[] Frameworks, bool RequiresExport);
    /// <implements epic="0201" feature="F5" claim="B-011" />
    public sealed record FeatureEntry(string? Epic, string Feature);
    /// <implements epic="0201" feature="F5" claim="B-028" />
    public sealed record ClaimFileSet(string Glob, string Feature, string? Epic, string? Record);

    /// <implements epic="0201" feature="F5" claim="B-028" />
    private static GherkinClaimTags NativeTags(JsonNode source, JsonNode? claim, JsonNode records, JsonNode? scopes, JsonObject identifiers)
    {
        if (source is not JsonValue value || !value.TryGetValue<string>(out var name) || name != "gherkin")
        {
            throw new EvidenceConfigurationException("codeAnalysis.records.claimSource must be \"gherkin\".");
        }

        var tag = claim?["tag"] is JsonValue form && form.TryGetValue<string>(out var text) ? text
            : throw new EvidenceConfigurationException("Native claim input requires claim.tag, mapped as /claim.");
        var ignoreCase = claim!["tagIgnoreCase"] is not { } node ? false
            : node is JsonValue flag && flag.TryGetValue<bool>(out var folded) ? folded
            : throw new EvidenceConfigurationException("claim.tagIgnoreCase must be a Boolean.");
        try
        {
            return new GherkinClaimTags(
                new GherkinClaimTagForm(tag, "claim", ignoreCase, identifiers["claim"]?.GetValue<string>()),
                Scope(scopes, "feature"), Scope(scopes, "epic"),
                Tags(records["plannedTags"], ["@ignore"]), Tags(records["withdrawnTags"], []), Tags(records["supersededTags"], []));
        }
        catch (GherkinClaimTextException failure)
        {
            throw new EvidenceConfigurationException(failure.Message);
        }
    }

    /// <implements epic="0201" feature="F5" claim="B-028" />
    private static GherkinClaimTagForm? Scope(JsonNode? scopes, string role) =>
        scopes?[role + "Tag"] is JsonValue node && node.TryGetValue<string>(out var form) ? new GherkinClaimTagForm(form, role, false, null) : null;

    /// <implements epic="0201" feature="F5" claim="B-028" />
    private static string[] Tags(JsonNode? node, string[] fallback) => node is null ? fallback
        : (node as JsonArray ?? throw TagLists()).Select(static entry => entry is JsonValue value && value.TryGetValue<string>(out var tag)
            && tag.Length > 1 && tag[0] == '@' && !tag.Any(char.IsWhiteSpace) ? tag : throw TagLists()).ToArray();

    /// <implements epic="0201" feature="F5" claim="B-028" />
    private static EvidenceConfigurationException TagLists() => new("Claim tag lists must hold @-led tags without whitespace.");

    /// <implements epic="0201" feature="F5" claim="B-035" />
    private static ClaimFileSet[] FileSets(JsonNode? node) => node is JsonArray { Count: > 0 } sets
        ? sets.Select(static entry => new ClaimFileSet(Text(entry, "glob"), Text(entry, "feature"),
            entry?["epic"] is null ? null : Text(entry, "epic"), entry?["record"] is null ? null : Text(entry, "record"))).ToArray()
        : throw new EvidenceConfigurationException("codeAnalysis.records.claimFileSets is missing or invalid.");

    /// <implements epic="0201" feature="F5" claim="B-035" />
    private static string Text(JsonNode? entry, string key) => entry?[key] is JsonValue value && value.TryGetValue<string>(out var text)
        && !string.IsNullOrWhiteSpace(text) ? text : throw new EvidenceConfigurationException("codeAnalysis.records.claimFileSets is missing or invalid.");

    /// <implements epic="0201" feature="F5" claim="B-011" />
    private static bool IsRelative(string path) => path.Length > 0 && !path.StartsWith('/') && !path.Contains('\\')
        && !(path.Length > 1 && char.IsLetter(path[0]) && path[1] == ':') && !path.Split('/').Any(static part => part is "" or "." or "..");

    private readonly JsonObject _policy;
    private readonly JsonObject _identifiers;
}
