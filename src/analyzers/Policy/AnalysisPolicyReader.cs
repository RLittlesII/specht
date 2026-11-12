using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Specht.Analysis.Bindings;
using Specht.Analysis.Bindings.Gherkin;

namespace Specht.Analysis.Policy;

/// <summary>Validates projected analysis meanings using the supplied consumer grammars.</summary>
public sealed class AnalysisPolicyReader
{
    public AnalysisPolicy Read(JsonObject projection)
    {
        var identifiers = Named(projection["identifiers"]);
        if (!identifiers.ContainsKey("feature") || !identifiers.ContainsKey("claim") || identifiers.Values.Any(string.IsNullOrWhiteSpace))
        {
            throw Reject("Identifiers must supply nonempty feature and claim grammars.");
        }

        foreach (var grammar in identifiers.Values)
        {
            _ = new Regex(grammar, RegexOptions.CultureInvariant);
        }

        var epics = identifiers.ContainsKey("epic");
        var policy = projection["policy"] as JsonObject ?? throw Reject("Policy is missing.");
        var projects = Array(policy["projects"], "Policy projects");
        var paths = projects.Select(project => _paths.Normalize(Text(project!, "path", "Policy projects"))).ToArray();
        if (paths.Distinct(StringComparer.Ordinal).Count() != paths.Length)
        {
            throw Reject("Policy projects path must be distinct.");
        }

        foreach (var project in projects)
        {
            var frameworks = Strings(project!["targetFrameworks"], "Policy projects targetFrameworks");
            if (frameworks.Length == 0 || frameworks.Any(string.IsNullOrWhiteSpace)
                || frameworks.Distinct(StringComparer.Ordinal).Count() != frameworks.Length)
            {
                throw Reject("Policy projects targetFrameworks must be nonempty and distinct.");
            }
        }

        foreach (var feature in Array(policy["features"], "Policy features"))
        {
            Owner(feature!, identifiers, epics);
        }

        if (policy["requiredLinks"]?["test"]?.GetValue<bool>() == false)
        {
            throw Reject("Policy requiredLinks test must not be false.");
        }

        foreach (var key in new[] { "implementation", "scenario" })
        {
            _ = policy["requiredLinks"]?[key]?.GetValue<bool>();
        }

        foreach (var scope in policy["enforcement"]?.AsArray() ?? new JsonArray())
        {
            Project(scope!, paths, "Enforcement");
            var role = Text(scope!, "role", "Enforcement");
            if (role is not ("verifies" or "implements" or "follows"))
            {
                throw Reject("Enforcement role must be verifies, implements or follows.");
            }

            var kinds = Strings(scope!["declarationKinds"], "Enforcement declarationKinds", true);
            if (kinds.Length == 0)
            {
                throw Reject("Enforcement declarationKinds must name at least one declaration kind.");
            }

            if (kinds.Any(static kind => !Kinds.Contains(kind, StringComparer.Ordinal)))
            {
                throw Reject("Enforcement declarationKinds names an unsupported declaration kind.");
            }

            if (role == "verifies" && kinds.Any(static kind => kind != "testMethod"))
            {
                throw Reject("Enforcement declarationKinds may name only testMethod when role is verifies.");
            }

            var includes = Strings(scope!["include"], "Enforcement include", true);
            if (includes.Length == 0)
            {
                throw Reject("Enforcement include must select at least one source glob.");
            }

            foreach (var glob in includes.Concat(Strings(scope!["exclude"], "Enforcement exclude", true)))
            {
                _ = _paths.Normalize(glob);
            }
        }

        foreach (var exemption in policy["exemptions"]?.AsArray() ?? new JsonArray())
        {
            _ = Text(exemption!, "reason", "Exemptions");
            var links = Strings(exemption!["links"], "Exemptions links", true);
            if (links.Length == 0 || links.Any(static link => link is not ("test" or "implementation" or "scenario")))
            {
                throw Reject("Exemptions links must name test, implementation or scenario.");
            }

            var target = exemption["target"]!;
            if (target["kind"] is null)
            {
                Owner(target, identifiers, epics);
                Identifier("claim", Text(target, "claim", "Exemptions target"), identifiers, "Exemptions target claim");
            }
            else
            {
                if (Text(target, "kind", "Exemptions target") != "declaration")
                {
                    throw Reject("Exemptions target kind must be declaration when present.");
                }

                Project(target, paths, "Exemptions target");
                _ = Text(target, "symbolId", "Exemptions target");
            }
        }

        foreach (var profile in policy["records"]?["adrProfiles"]?.AsArray() ?? new JsonArray())
        {
            _ = _paths.Normalize(Text(profile!, "glob", "Records adrProfiles"));
            if (Text(profile!, "owner", "Records adrProfiles") is not ("repository" or "containingFeature"))
            {
                throw Reject("Records adrProfiles owner must be repository or containingFeature.");
            }

            Selector(profile!["identity"], false, "Records adrProfiles identity");
            Selector(profile["lifecycle"], true, "Records adrProfiles lifecycle");
        }

        foreach (var role in new[] { "feature", "epic" })
        {
            if (policy["scenarioScopes"]?[role + "Tag"] is { } form)
            {
                var marker = "{" + role + "}";
                var text = form.GetValue<string>();
                var index = text.IndexOf(marker, StringComparison.Ordinal);
                if (index < 0 || text.IndexOf(marker, index + marker.Length, StringComparison.Ordinal) >= 0
                    || text.Replace(marker, string.Empty).IndexOfAny(new[] { '{', '}' }) >= 0)
                {
                    throw Reject("ScenarioScopes " + role + "Tag must contain exactly its own placeholder.");
                }
            }
        }

        var markers = Named(projection["markers"]);
        var omissions = projection["omissions"];
        var gherkin = new GherkinClaimPolicyReader().Read(projection["claim"], policy["records"] as JsonObject,
            policy["scenarioScopes"] as JsonObject, identifiers, epics);
        var bindings = new BindingPolicy(projection["schemaVersion"]?.GetValue<string>() ?? string.Empty,
            identifiers, markers, epics,
            gherkin is null ? Text(projection, "claimsSection", "Policy") : projection["claimsSection"]?.GetValue<string>() ?? string.Empty,
            new LifecycleOmissions(
                omissions?["feature"]?.GetValue<bool>() == true, omissions?["claim"]?.GetValue<bool>() == true,
                omissions?["adr"]?.GetValue<bool>() == true));
        return new AnalysisPolicy(policy, bindings) { Gherkin = gherkin };
    }

    private static AnalysisPolicyException Reject(string message) => new(message);

    private static Dictionary<string, string> Named(JsonNode? node) =>
        (node as JsonObject ?? throw Reject("Identifiers must be an object.")).ToDictionary(
            static entry => entry.Key,
            static entry => entry.Value?.GetValue<string>() ?? throw Reject("Identifiers entries must be nonempty text."), StringComparer.Ordinal);

    private static string Text(JsonNode node, string key, string container) => node?[key]?.GetValue<string>() is { } value && !string.IsNullOrWhiteSpace(value)
        ? value : throw Reject(container + " " + key + " must be present and nonempty.");

    private static JsonArray Array(JsonNode? node, string field) => node as JsonArray ?? throw Reject(field + " must be an array.");

    private static string[] Strings(JsonNode? node, string field, bool absentIsEmpty = false) =>
        node is null && absentIsEmpty ? System.Array.Empty<string>()
        : Array(node, field).Select(value => value?.GetValue<string>() ?? throw Reject(field + " entries must be text.")).ToArray();

    private static void Owner(JsonNode target, IReadOnlyDictionary<string, string> identifiers, bool epics)
    {
        Identifier("feature", Text(target, "feature", "Policy features"), identifiers, "Policy features feature");
        if (epics != (target["epic"] is not null))
        {
            throw Reject("Policy features epic must be present exactly when identifiers supply an epic grammar.");
        }

        if (epics)
        {
            Identifier("epic", Text(target, "epic", "Policy features"), identifiers, "Policy features epic");
        }
    }

    private static void Identifier(string role, string value, IReadOnlyDictionary<string, string> identifiers, string field)
    {
        if (!identifiers.TryGetValue(role, out var grammar)
            || !new Regex("\\A(?:" + grammar + ")\\z", RegexOptions.CultureInvariant).IsMatch(value))
        {
            throw Reject(field + " must match its identifiers grammar.");
        }
    }

    private static void Selector(JsonNode? selector, bool lifecycle, string field)
    {
        if (selector is not JsonObject)
        {
            throw Reject(field + " must be an object.");
        }

        if (selector["frontmatterKey"] is not null)
        {
            _ = Text(selector, "frontmatterKey", field);
            if (selector.AsObject().Count != 1)
            {
                throw Reject(field + " must select exactly one encoding.");
            }
        }
        else if (lifecycle)
        {
            _ = Text(selector, "section", field);
            if (selector["firstWord"]?.GetValue<bool>() != true)
            {
                throw Reject(field + " firstWord must be true.");
            }
        }
        else if (selector["headingLevel"]?.GetValue<int>() is not (>= 1 and <= 6))
        {
            throw Reject(field + " headingLevel must be from 1 to 6.");
        }
        else
        {
            _ = Text(selector, "separator", field);
        }
    }

    private void Project(JsonNode node, string[] paths, string container)
    {
        if (!paths.Contains(_paths.Normalize(Text(node, "project", container)), StringComparer.Ordinal))
        {
            throw Reject(container + " project must be a declared project.");
        }
    }

    private static readonly string[] Kinds =
    {
        "type", "class", "record", "struct", "interface", "enum", "delegate", "constructor", "method", "testMethod",
        "property", "indexer", "field", "event", "operator", "conversion", "finalizer", "enumMember",
    };

    private readonly AnalysisPaths _paths = new();
}
