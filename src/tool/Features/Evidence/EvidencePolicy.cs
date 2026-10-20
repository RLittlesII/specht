using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Specht.Tool.Features.Evidence.Contract;

namespace Specht.Tool.Features.Evidence;

internal sealed class EvidencePolicy
{
    private EvidencePolicy(JsonObject projected)
    {
        var policy = projected["policy"] as JsonObject ?? throw new EvidenceConfigurationException("Policy is missing.");
        var identifiers = projected["identifiers"] as JsonObject ?? throw new EvidenceConfigurationException("Identifiers are missing.");
        _identifiers = identifiers;
        HasEpic = identifiers.ContainsKey("epic");
        ClaimsSection = projected["claimsSection"]?.GetValue<string>() ?? throw new EvidenceConfigurationException("Claims section is missing.");
        var records = policy["records"];
        FeatureKey = records?["featureIdentityKey"]?.GetValue<string>() ?? "id";
        EpicKey = records?["featureEpicKey"]?.GetValue<string>() ?? "epic";
        FeatureLifecycleKey = records?["featureLifecycleKey"]?.GetValue<string>() ?? "spec_status";
        ClaimIdColumn = records?["claimIdColumn"]?.GetValue<string>() ?? "ID";
        ClaimStatusColumn = records?["claimStatusColumn"]?.GetValue<string>() ?? "Status";
        ApprovedMarker = projected["markers"]?["approved"]?.GetValue<string>() ?? "approved";
        WithdrawnMarker = projected["markers"]?["withdrawn"]?.GetValue<string>() ?? "Withdrawn";
        SupersededMarker = projected["markers"]?["superseded"]?.GetValue<string>() ?? "Superseded";
        Projects = (policy["projects"] as JsonArray ?? throw new EvidenceConfigurationException("Projects are missing."))
            .Select(static item => new ProjectEntry(item!["path"]!.GetValue<string>(), item["targetFrameworks"]!.AsArray()
                .Select(static framework => framework!.GetValue<string>()).ToArray())).ToArray();
        Features = (policy["features"] as JsonArray ?? throw new EvidenceConfigurationException("Features are missing."))
            .Select(item => new FeatureEntry(HasEpic ? item!["epic"]?.GetValue<string>() : null,
                item!["feature"]!.GetValue<string>())).ToArray();
        _policy = policy;
        if (Projects.Any(static project => project.Frameworks.Length == 0
                || project.Frameworks.Distinct(StringComparer.Ordinal).Count() != project.Frameworks.Length)
            || Projects.Select(static project => project.Path).Distinct(StringComparer.Ordinal).Count() != Projects.Length
            || Projects.Any(static project => project.Path.Length == 0 || project.Path.StartsWith('/')
                || project.Path.Contains('\\') || (project.Path.Length > 1 && char.IsLetter(project.Path[0]) && project.Path[1] == ':')
                || project.Path.Split('/').Any(static part => part is "" or "." or ".."))
            || Features.Any(feature => !Matches("feature", feature.Feature) || HasEpic && (feature.Epic is null || !Matches("epic", feature.Epic)))
            || Features.Distinct().Count() != Features.Length
            || !identifiers.ContainsKey("claim") || policy["requiredLinks"]?["test"]?.GetValue<bool>() == false)
        {
            throw new EvidenceConfigurationException("Analysis policy is invalid.");
        }

        RequiredKinds = new[] { "test", "implementation", "scenario" }
            .Where(kind => kind == "test" || policy["requiredLinks"]?[kind]?.GetValue<bool>() == true).ToArray();
        foreach (var exemption in policy["exemptions"] as JsonArray ?? new JsonArray())
        {
            var reason = exemption?["reason"]?.GetValue<string>();
            var links = exemption?["links"] as JsonArray;
            var target = exemption?["target"];
            if (string.IsNullOrWhiteSpace(reason) || links is null || links.Count == 0
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

    public bool Matches(string role, string value) => _identifiers[role]?.GetValue<string>() is { } grammar
        && Regex.IsMatch(value, "\\A(?:" + grammar + ")\\z", RegexOptions.CultureInvariant);

    public string? Exemption(string? epic, string feature, string claim, string kind) =>
        (_policy["exemptions"] as JsonArray ?? new JsonArray()).Where(item => item?["target"]?["kind"] is null
            && item?["target"]?["epic"]?.GetValue<string>() == epic
            && item?["target"]?["feature"]?.GetValue<string>() == feature
            && item?["target"]?["claim"]?.GetValue<string>() == claim
            && item?["links"]?.AsArray().Any(link => link?.GetValue<string>() == kind) == true)
            .Select(static item => item?["reason"]?.GetValue<string>()).FirstOrDefault();

    public sealed record ProjectEntry(string Path, string[] Frameworks);
    public sealed record FeatureEntry(string? Epic, string Feature);

    private readonly JsonObject _policy;
    private readonly JsonObject _identifiers;
}
