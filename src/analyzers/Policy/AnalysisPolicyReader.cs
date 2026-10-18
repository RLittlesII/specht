using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Policy;

/// <summary>Validates projected analysis meanings using the supplied consumer grammars.</summary>
public sealed class AnalysisPolicyReader
{
    public AnalysisPolicy Read(JsonObject projection)
    {
        var identifiers = Named(projection["identifiers"]!);
        if (!identifiers.ContainsKey("feature") || !identifiers.ContainsKey("claim") || identifiers.Values.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Analysis requires the supplied Feature and claim grammars.");
        }

        foreach (var grammar in identifiers.Values)
        {
            _ = new Regex(grammar, RegexOptions.CultureInvariant);
        }

        var epics = identifiers.ContainsKey("epic");
        var policy = projection["policy"] as JsonObject ?? throw new ArgumentException("Analysis policy is missing.");
        var projects = Array(policy["projects"]);
        var paths = projects.Select(project => _paths.Normalize(Text(project!, "path"))).ToArray();
        if (paths.Distinct(StringComparer.Ordinal).Count() != paths.Length)
        {
            throw new ArgumentException("Analysis projects must be distinct.");
        }

        foreach (var project in projects)
        {
            var frameworks = Strings(project!["targetFrameworks"]!);
            if (frameworks.Length == 0 || frameworks.Any(string.IsNullOrWhiteSpace)
                || frameworks.Distinct(StringComparer.Ordinal).Count() != frameworks.Length)
            {
                throw new ArgumentException("Analysis projects require distinct nonempty target frameworks.");
            }
        }

        foreach (var feature in Array(policy["features"]))
        {
            Owner(feature!, identifiers, epics);
        }

        if (policy["requiredLinks"]?["test"]?.GetValue<bool>() == false)
        {
            throw new ArgumentException("Selected non-exempt claims require test links.");
        }

        foreach (var key in new[] { "implementation", "scenario" })
        {
            _ = policy["requiredLinks"]?[key]?.GetValue<bool>();
        }

        foreach (var scope in policy["enforcement"]?.AsArray() ?? new JsonArray())
        {
            Project(scope!, paths);
            var role = Text(scope!, "role");
            var kinds = Strings(scope!["declarationKinds"]!);
            if (role is not ("verifies" or "implements" or "follows") || kinds.Length == 0
                || kinds.Any(static kind => !Kinds.Contains(kind, StringComparer.Ordinal))
                || role == "verifies" && kinds.Any(static kind => kind != "testMethod"))
            {
                throw new ArgumentException("Analysis enforcement must select supported roles and declarations.");
            }

            var includes = Strings(scope["include"]!);
            if (includes.Length == 0)
            {
                throw new ArgumentException("An authoring scope requires explicit source selection.");
            }

            foreach (var glob in includes.Concat(Strings(scope["exclude"]!)))
            {
                _ = _paths.Normalize(glob);
            }
        }

        foreach (var exemption in policy["exemptions"]?.AsArray() ?? new JsonArray())
        {
            _ = Text(exemption!, "reason");
            var links = Strings(exemption!["links"]!);
            if (links.Length == 0 || links.Any(static link => link is not ("test" or "implementation" or "scenario")))
            {
                throw new ArgumentException("An exemption must name its required link kinds.");
            }

            var target = exemption["target"]!;
            if (target["kind"] is null)
            {
                Owner(target, identifiers, epics);
                Identifier("claim", Text(target, "claim"), identifiers);
            }
            else
            {
                if (Text(target, "kind") != "declaration")
                {
                    throw new ArgumentException("An exemption must select an explicit claim or declaration.");
                }

                Project(target, paths);
                _ = Text(target, "symbolId");
            }
        }

        foreach (var profile in policy["records"]?["adrProfiles"]?.AsArray() ?? new JsonArray())
        {
            _ = _paths.Normalize(Text(profile!, "glob"));
            if (Text(profile!, "owner") is not ("repository" or "containingFeature"))
            {
                throw new ArgumentException("ADR profiles require explicit ownership.");
            }

            Selector(profile!["identity"]!, false);
            Selector(profile["lifecycle"]!, true);
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
                    throw new ArgumentException("Scenario encodings must contain exactly their own placeholder.");
                }
            }
        }

        var markers = Named(projection["markers"]);
        var omissions = projection["omissions"];
        var bindings = new BindingPolicy(projection["schemaVersion"]?.GetValue<string>() ?? string.Empty,
            identifiers, markers, epics, Text(projection, "claimsSection"), new LifecycleOmissions(
                omissions?["feature"]?.GetValue<bool>() == true, omissions?["claim"]?.GetValue<bool>() == true,
                omissions?["adr"]?.GetValue<bool>() == true));
        return new AnalysisPolicy(policy, bindings);
    }

    private static Dictionary<string, string> Named(JsonNode? node) =>
        (node as JsonObject ?? throw new ArgumentException("Consumer grammars are missing.")).ToDictionary(
        static entry => entry.Key,
            static entry => entry.Value?.GetValue<string>() ?? throw new ArgumentException("A consumer grammar is empty."), StringComparer.Ordinal);

    private static string Text(JsonNode node, string key) => node?[key]?.GetValue<string>() is { } value && !string.IsNullOrWhiteSpace(value)
        ? value : throw new ArgumentException("An explicitly mapped analysis field is absent or empty.");

    private static JsonArray Array(JsonNode? node) => node as JsonArray ?? throw new ArgumentException("An analysis selection array is missing.");

    private static string[] Strings(JsonNode? node) => Array(node).Select(static value => value?.GetValue<string>()
        ?? throw new ArgumentException("An analysis selection value is missing.")).ToArray();

    private static void Owner(JsonNode target, IReadOnlyDictionary<string, string> identifiers, bool epics)
    {
        Identifier("feature", Text(target, "feature"), identifiers);
        if (epics != (target["epic"] is not null))
        {
            throw new ArgumentException("Feature selection must follow the consumer's epic tier.");
        }

        if (epics)
        {
            Identifier("epic", Text(target, "epic"), identifiers);
        }
    }

    private static void Identifier(string role, string value, IReadOnlyDictionary<string, string> identifiers)
    {
        if (!identifiers.TryGetValue(role, out var grammar)
            || !new Regex("\\A(?:" + grammar + ")\\z", RegexOptions.CultureInvariant).IsMatch(value))
        {
            throw new ArgumentException("Analysis identity violates its supplied consumer grammar.");
        }
    }

    private static void Selector(JsonNode selector, bool lifecycle)
    {
        if (selector is not JsonObject)
        {
            throw new ArgumentException("A record selector is missing.");
        }

        if (selector["frontmatterKey"] is not null)
        {
            _ = Text(selector, "frontmatterKey");
            if (selector.AsObject().Count != 1)
            {
                throw new ArgumentException("Record selectors must select exactly one encoding.");
            }
        }
        else if (lifecycle)
        {
            _ = Text(selector, "section");
            if (selector["firstWord"]?.GetValue<bool>() != true)
            {
                throw new ArgumentException("A lifecycle section selector requires firstWord.");
            }
        }
        else if (selector["headingLevel"]?.GetValue<int>() is not (>= 1 and <= 6))
        {
            throw new ArgumentException("An identity heading selector requires a valid heading level.");
        }
        else
        {
            _ = Text(selector, "separator");
        }
    }

    private void Project(JsonNode node, string[] paths)
    {
        if (!paths.Contains(_paths.Normalize(Text(node, "project")), StringComparer.Ordinal))
        {
            throw new ArgumentException("Analysis enforcement references an undeclared project.");
        }
    }

    private static readonly string[] Kinds =
    {
        "type", "class", "record", "struct", "interface", "enum", "delegate", "constructor", "method", "testMethod",
        "property", "indexer", "field", "event", "operator", "conversion", "finalizer", "enumMember",
    };
    private readonly AnalysisPaths _paths = new();
}
