using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Specht.Analysis.TestEvidence;

public sealed class TestEvidenceArtifactReader
{
    public TestEvidenceReadResult Read(string json, TestEvidenceAnalysisInputs? currentInputs = null)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw Invalid("artifact must be an object.");
            }

            var version = Integer(root, "formatVersion");
            if (String(root, "format") != "specht.test-evidence" || version is not (1 or 2 or 3))
            {
                return TestEvidenceReadResult.Unsupported();
            }

            var members = new[] { "format", "formatVersion", "project", "configuration", "assembly",
                "sourceFingerprint", "policyFingerprint", "adapter", "discovery", "bindings", "results", "inputs", "scenarios" };
            Object(root, "artifact", version == 1 ? members : members.Append("catalog").ToArray());

            var project = Object(Required(root, "project"), "project", "path", "targetFramework");
            var configuration = Object(Required(root, "configuration"), "configuration", "name", "fingerprint");
            var assembly = Object(Required(root, "assembly"), "assembly", "name", "sha256", "path");
            var adapter = Object(Required(root, "adapter"), "adapter", "id", "version", "contractVersion");
            var discovery = ReadDiscovery(Required(root, "discovery"), version);
            var bindings = ReadBindings(Required(root, "bindings"));
            var results = ReadResults(Required(root, "results"), discovery.Cases);
            var inputs = ReadInputs(Required(root, "inputs"));
            var sourceFingerprint = Digest(String(root, "sourceFingerprint"), "sourceFingerprint");
            var policyFingerprint = Digest(String(root, "policyFingerprint"), "policyFingerprint");
            var fingerprint = new TestEvidenceFingerprint();
            if (fingerprint.ComputeDigests(inputs.Where(static input => input.Kind is "source" or "generated-source"))
                    != sourceFingerprint
                || fingerprint.ComputeDigests(inputs.Where(static input => input.Kind is "manifest" or "schema" or "record"))
                    != policyFingerprint)
            {
                throw Invalid("Declared fingerprints do not match their input identities.");
            }
            var scenarios = ReadScenarios(root, discovery.Cases);
            var catalog = root.TryGetProperty("catalog", out var catalogElement) ? ReadCatalog(catalogElement) : null;

            var artifact = new TestEvidenceArtifact(
                Path(String(project, "path")), NonEmpty(String(project, "targetFramework"), "project.targetFramework"),
                NonEmpty(String(configuration, "name"), "configuration.name"), Digest(String(configuration, "fingerprint"), "configuration.fingerprint"),
                NonEmpty(String(assembly, "name"), "assembly.name"), Digest(String(assembly, "sha256"), "assembly.sha256"),
                OptionalPath(assembly, "path"), sourceFingerprint, policyFingerprint,
                NonEmpty(String(adapter, "id"), "adapter.id"), NonEmpty(String(adapter, "version"), "adapter.version"),
                NonEmpty(String(adapter, "contractVersion"), "adapter.contractVersion"), discovery, bindings, results, inputs, scenarios, catalog);

            var isCurrent = currentInputs is null || (artifact.ProjectPath == currentInputs.ProjectPath
                && artifact.TargetFramework == currentInputs.TargetFramework
                && artifact.SourceFingerprint == currentInputs.SourceFingerprint
                && artifact.AssemblySha256 == currentInputs.AssemblySha256
                && artifact.ConfigurationFingerprint == currentInputs.ConfigurationFingerprint
                && artifact.PolicyFingerprint == currentInputs.PolicyFingerprint);
            return TestEvidenceReadResult.Valid(artifact, isCurrent);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or ContractException
            or FormatException or OverflowException or ArgumentException)
        {
            return TestEvidenceReadResult.Invalid();
        }
    }

    private static IReadOnlyList<TestEvidenceCatalogFeature> ReadCatalog(JsonElement element)
    {
        var value = Object(element, "catalog", "features");
        return Array(value, "features").Select(static feature =>
        {
            var item = Object(feature, "catalog feature", "epic", "feature", "state", "path", "claims");
            return new TestEvidenceCatalogFeature(NullableString(item, "epic"), NonEmpty(String(item, "feature"), "catalog.feature"),
                NonEmpty(String(item, "state"), "catalog.state"), Path(String(item, "path")),
                Array(item, "claims").Select(ReadCatalogClaim).ToArray());
        }).ToArray();
    }

    private static TestEvidenceCatalogClaim ReadCatalogClaim(JsonElement element)
    {
        var item = Object(element, "catalog claim", "id", "state", "element", "path", "line", "cases");
        var kind = String(item, "element");
        if (!CatalogElements.Contains(kind))
        {
            throw Invalid("Catalog claim element is not recognized.");
        }

        return new TestEvidenceCatalogClaim(NonEmpty(String(item, "id"), "catalog.claim.id"),
            NonEmpty(String(item, "state"), "catalog.claim.state"), kind, Path(String(item, "path")),
            Positive(Integer(item, "line"), "catalog.claim.line"), Array(item, "cases").Select(static testCase =>
            {
                var value = Object(testCase, "catalog case", "line", "planned");
                return new TestEvidenceCatalogCase(Positive(Integer(value, "line"), "catalog.case.line"), Boolean(value, "planned"));
            }).ToArray());
    }

    private static TestEvidenceDiscovery ReadDiscovery(JsonElement element, int version)
    {
        var value = Object(element, "discovery", "state", "sessionId", "methods", "cases");
        var stateText = String(value, "state");
        if (!Enum.TryParse<TestEvidenceDiscoveryState>(stateText, true, out var state)
            || !string.Equals(state.ToString().ToLowerInvariant(), stateText, StringComparison.Ordinal))
        {
            throw Invalid("discovery.state is not recognized.");
        }

        var session = NullableString(value, "sessionId");
        if (state is TestEvidenceDiscoveryState.Complete or TestEvidenceDiscoveryState.Deferred)
        {
            NonEmpty(session, "discovery.sessionId");
        }
        else if (session is not null)
        {
            throw Invalid("Unavailable discovery must have a null sessionId.");
        }

        var methods = Array(value, "methods").Select(ReadMethod).ToArray();
        var methodIds = new HashSet<string>(methods.Select(static method => method.SymbolId), StringComparer.Ordinal);
        if (methodIds.Count != methods.Length)
        {
            throw Invalid("Method symbol IDs must be unique.");
        }

        var cases = Array(value, "cases").Select(item => ReadCase(item, version)).ToArray();
        if (cases.Select(static item => item.CaseId).Distinct(StringComparer.Ordinal).Count() != cases.Length
            || cases.Any(item => !methodIds.Contains(item.MethodSymbolId)))
        {
            throw Invalid("Case IDs must be unique and each case must name a discovered method.");
        }

        if ((state is TestEvidenceDiscoveryState.Unavailable or TestEvidenceDiscoveryState.UnsupportedContract) && cases.Length != 0)
        {
            throw Invalid("Unavailable discovery cannot claim discovered cases.");
        }

        return new TestEvidenceDiscovery(state, session, methods, cases);
    }

    private static TestEvidenceMethod ReadMethod(JsonElement element)
    {
        var value = Object(element, "method", "symbolId", "declaringType", "name", "genericArity", "parameterTypes", "location");
        var parameters = Array(value, "parameterTypes").Select(static parameter =>
        {
            if (parameter.ValueKind != JsonValueKind.String)
            {
                throw Invalid("Method parameter types must be strings.");
            }

            return parameter.GetString()!;
        }).ToArray();
        var location = Required(value, "location");
        TestEvidenceLocation? source = null;
        if (location.ValueKind != JsonValueKind.Null)
        {
            var item = Object(location, "location", "path", "line", "column");
            source = new TestEvidenceLocation(Path(String(item, "path")), Positive(Integer(item, "line"), "location.line"),
                Positive(Integer(item, "column"), "location.column"));
        }

        return new TestEvidenceMethod(NonEmpty(String(value, "symbolId"), "method.symbolId"),
            NonEmpty(String(value, "declaringType"), "method.declaringType"), NonEmpty(String(value, "name"), "method.name"),
            NonNegative(Integer(value, "genericArity"), "method.genericArity"), parameters, source);
    }

    private static TestEvidenceCase ReadCase(JsonElement element, int version)
    {
        var value = version == 3
            ? Object(element, "case", "caseId", "methodSymbolId", "rowId", "displayName", "categories", "owned", "selected")
            : Object(element, "case", "caseId", "methodSymbolId", "rowId", "displayName");
        var testCase = new TestEvidenceCase(NonEmpty(String(value, "caseId"), "case.caseId"),
            NonEmpty(String(value, "methodSymbolId"), "case.methodSymbolId"), NullableString(value, "rowId"), NullableString(value, "displayName"));
        if (version != 3)
        {
            return testCase;
        }

        var categories = Array(value, "categories").Select(static category => category.ValueKind == JsonValueKind.String
            ? NonEmpty(category.GetString(), "case.categories") : throw Invalid("Case categories must be strings.")).ToArray();
        if (!categories.SequenceEqual(categories.Distinct(StringComparer.Ordinal).OrderBy(static category => category, StringComparer.Ordinal)))
        {
            throw Invalid("Case categories must be distinct and in ordinal order.");
        }

        return testCase with { Categories = categories, Owned = Boolean(value, "owned"), Selected = Boolean(value, "selected") };
    }

    private static IReadOnlyList<TestEvidenceBinding> ReadBindings(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            throw Invalid("bindings must be an array.");
        }

        return element.EnumerateArray().Select(item =>
        {
            var value = Object(item, "binding", "symbolId", "role", "attributes", "shapeValid", "status", "isEligible", "target", "matches", "location");
            var attributes = Required(value, "attributes");
            if (attributes.ValueKind != JsonValueKind.Object
                || attributes.EnumerateObject().Select(static property => property.Name).Distinct(StringComparer.Ordinal).Count()
                != attributes.EnumerateObject().Count()
                || attributes.EnumerateObject().Any(static property => property.Value.ValueKind != JsonValueKind.String))
            {
                throw Invalid("Binding attributes must be strings.");
            }

            var rawAttributes = attributes.EnumerateObject().ToDictionary(static property => property.Name,
                static property => property.Value.GetString()!, StringComparer.Ordinal);
            var role = NonEmpty(String(value, "role"), "binding.role");
            if (!Roles.Contains(role))
            {
                throw Invalid("Binding role is not recognized.");
            }

            var status = NullableString(value, "status");
            if (status is not null && !BindingStatuses.Contains(status))
            {
                throw Invalid("Binding status is not recognized.");
            }

            var matches = Array(value, "matches").Select(match => ReadTarget(match, "binding match")).ToArray();
            var target = Required(value, "target");
            var resolvedTarget = target.ValueKind == JsonValueKind.Null ? null : ReadTarget(target, "binding target");
            var sourceLocation = ReadLocation(Required(value, "location"), "binding location");

            var shapeValid = Boolean(value, "shapeValid");
            var eligible = Required(value, "isEligible");
            var isEligible = eligible.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => (bool?)null,
                _ => throw Invalid("Binding eligibility must be boolean or null."),
            };
            if (shapeValid != (status is not null)
                || (status == "resolved") != (resolvedTarget is not null)
                || ((status == "resolved") != (eligible.ValueKind is JsonValueKind.True or JsonValueKind.False)))
            {
                throw Invalid("Binding shape, resolution, target and eligibility fields disagree.");
            }

            return new TestEvidenceBinding(NonEmpty(String(value, "symbolId"), "binding.symbolId"),
                role, rawAttributes, shapeValid, status, isEligible, resolvedTarget, matches, sourceLocation);
        }).ToArray();
    }

    private static TestEvidenceResults ReadResults(JsonElement element, IReadOnlyList<TestEvidenceCase> discoveredCases)
    {
        var state = String(element, "state");
        if (state == "absent")
        {
            Object(element, "results", "state");
            if (discoveredCases.Any(static item => item.Selected == true))
            {
                throw Invalid("A case cannot be selected when results are absent.");
            }

            return new TestEvidenceResults(state, null, System.Array.Empty<TestEvidenceOutcome>());
        }

        var value = Object(element, "results", "state", "runId", "cases");
        if (state != "available")
        {
            throw Invalid("results.state is not recognized.");
        }

        var caseIds = new HashSet<string>(discoveredCases.Where(static item => item.Selected != false).Select(static item => item.CaseId),
            StringComparer.Ordinal);
        var outcomes = Array(value, "cases").Select(item =>
        {
            var result = Object(item, "result", "caseId", "outcome", "detail");
            var caseId = NonEmpty(String(result, "caseId"), "results.caseId");
            var outcome = String(result, "outcome");
            if (!Outcomes.Contains(outcome) || !caseIds.Contains(caseId))
            {
                throw Invalid("Each result must name a discovered case and a supported outcome.");
            }

            return new TestEvidenceOutcome(caseId, outcome, NullableString(result, "detail"));
        }).ToArray();
        var uniqueOutcomes = new List<TestEvidenceOutcome>();
        foreach (var group in outcomes.GroupBy(static result => result.CaseId, StringComparer.Ordinal))
        {
            var distinct = group.Distinct().ToArray();
            if (distinct.Length > 1)
            {
                throw Invalid("Conflicting results for one case and run are invalid.");
            }

            uniqueOutcomes.Add(distinct[0]);
        }

        return new TestEvidenceResults(state, NonEmpty(String(value, "runId"), "results.runId"), uniqueOutcomes);
    }

    private static IReadOnlyList<TestEvidenceInput> ReadInputs(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            throw Invalid("inputs must be an array.");
        }

        var inputs = element.EnumerateArray().Select(item =>
        {
            var value = Object(item, "input", "path", "sha256", "kind");
            var path = Path(String(value, "path"));
            var hash = Digest(String(value, "sha256"), "inputs.sha256");
            var kind = String(value, "kind");
            if (!InputKinds.Contains(kind))
            {
                throw Invalid("Input kind is not recognized.");
            }

            return new TestEvidenceInput(path, hash, kind);
        }).ToArray();
        var ordered = inputs.OrderBy(static item => item.Path, StringComparer.Ordinal).ThenBy(static item => item.Kind, StringComparer.Ordinal).ToArray();
        if (!inputs.SequenceEqual(ordered) || inputs.Select(static item => (item.Path, item.Kind)).Distinct().Count() != inputs.Length)
        {
            throw Invalid("Inputs must be uniquely sorted by path and kind.");
        }

        return inputs;
    }

    private static IReadOnlyList<JsonElement> ReadScenarios(JsonElement root, IReadOnlyList<TestEvidenceCase> discoveredCases)
    {
        if (!root.TryGetProperty("scenarios", out var scenarios))
        {
            return System.Array.Empty<JsonElement>();
        }

        if (scenarios.ValueKind != JsonValueKind.Array)
        {
            throw Invalid("scenarios must be an array.");
        }

        var records = scenarios.EnumerateArray().ToArray();
        var caseIds = new HashSet<string>(discoveredCases.Select(static item => item.CaseId), StringComparer.Ordinal);
        var identities = new HashSet<string>(StringComparer.Ordinal);
        foreach (var scenario in records)
        {
            var value = Object(scenario, "scenario", "path", "scenarioId", "line", "scope", "claims", "cases");
            var path = Path(String(value, "path"));
            var scenarioId = NullableString(value, "scenarioId");
            if (scenarioId is not null)
            {
                NonEmpty(scenarioId, "scenario.scenarioId");
            }

            var line = Positive(Integer(value, "line"), "scenario.line");
            var identity = scenarioId is null ? path + "\0line:" + line : path + "\0id:" + scenarioId;
            if (!identities.Add(identity))
            {
                throw Invalid("Scenario identities must be unique within an artifact.");
            }

            var scope = Object(Required(value, "scope"), "scenario.scope", "epic", "feature");
            var epic = NullableString(scope, "epic");
            var feature = NonEmpty(String(scope, "feature"), "scenario.scope.feature");
            var claims = Array(value, "claims").Select(claim => ReadScenarioClaim(claim, epic, feature)).ToArray();
            if (claims.Select(static claim => claim.Id).Distinct(StringComparer.Ordinal).Count() != claims.Length)
            {
                throw Invalid("Scenario claim IDs must be unique.");
            }

            var links = Array(value, "cases").Select(link => ReadScenarioCase(link, caseIds, discoveredCases, scenarioId)).ToArray();
            if (links.Length == 0)
            {
                throw Invalid("Each scenario must retain at least one case link or unresolved link.");
            }
        }

        return records.Select(static scenario => scenario.Clone()).ToArray();
    }

    private static (string Id, string Status) ReadScenarioClaim(JsonElement element, string? epic, string feature)
    {
        var value = Object(element, "scenario claim", "id", "status", "isEligible", "target", "matches");
        var id = NonEmpty(String(value, "id"), "scenario.claim.id");
        var status = String(value, "status");
        if (!BindingStatuses.Contains(status))
        {
            throw Invalid("Scenario claim status is not recognized.");
        }

        var targetElement = Required(value, "target");
        var target = targetElement.ValueKind == JsonValueKind.Null ? null : ReadTarget(targetElement, "scenario claim target");
        var matches = Array(value, "matches").Select(match => ReadTarget(match, "scenario claim match")).ToArray();
        var eligibleElement = Required(value, "isEligible");
        var eligible = eligibleElement.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => (bool?)null,
            _ => throw Invalid("Scenario claim eligibility must be boolean or null."),
        };
        if ((status == "resolved") != (target is not null)
            || (status == "resolved") != (eligible is not null)
            || target is not null && (target.Kind != "claim" || target.Id != id || target.Epic != epic || target.Feature != feature)
            || matches.Any(match => match.Kind != "claim" || match.Id != id || match.Epic != epic || match.Feature != feature))
        {
            throw Invalid("Scenario claim status, scope, target and matches disagree.");
        }

        return (id, status);
    }

    private static (string? RowId, string? CaseId, string Status) ReadScenarioCase(
        JsonElement element,
        HashSet<string> caseIds,
        IReadOnlyList<TestEvidenceCase> discoveredCases,
        string? scenarioId)
    {
        var value = Object(element, "scenario case link", "rowId", "caseId", "status", "state");
        var rowId = NullableString(value, "rowId");
        var caseId = NullableString(value, "caseId");
        var status = String(value, "status");
        if (String(value, "state") is not ("planned" or "active"))
        {
            throw Invalid("Scenario case link state must be planned or active.");
        }

        if (rowId is not null)
        {
            NonEmpty(rowId, "scenario case rowId");
        }

        if (status == "unresolved")
        {
            if (caseId is not null)
            {
                throw Invalid("Unresolved scenario case links must have a null caseId.");
            }

            return (rowId, null, status);
        }

        if (status != "resolved" || scenarioId is null || caseId is null || !caseIds.Contains(caseId))
        {
            throw Invalid("Resolved scenario case links must name a discovered case and scenario ID.");
        }

        var testCase = discoveredCases.Single(item => item.CaseId == caseId);
        if (testCase.RowId != rowId)
        {
            throw Invalid("Scenario row IDs must match the linked discovered case.");
        }

        return (rowId, caseId, status);
    }

    private static JsonElement Object(JsonElement value, string description, params string[] allowed)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw Invalid(description + " must be an object.");
        }

        var names = new HashSet<string>(allowed, StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
        {
            if (!names.Contains(property.Name) || !seen.Add(property.Name))
            {
                throw Invalid(description + " has an unknown or duplicate property.");
            }
        }

        return value;
    }

    private static JsonElement Required(JsonElement value, string name) => value.TryGetProperty(name, out var result)
        ? result : throw Invalid("Required property '" + name + "' is missing.");

    private static string String(JsonElement value, string name)
    {
        var item = Required(value, name);
        if (item.ValueKind != JsonValueKind.String)
        {
            throw Invalid("Property '" + name + "' must be a string.");
        }

        return item.GetString()!;
    }

    private static string? NullableString(JsonElement value, string name)
    {
        var item = Required(value, name);
        return item.ValueKind == JsonValueKind.Null ? null : item.ValueKind == JsonValueKind.String
            ? item.GetString() : throw Invalid("Property '" + name + "' must be a string or null.");
    }

    private static int Integer(JsonElement value, string name)
    {
        var item = Required(value, name);
        return item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out var number)
            ? number : throw Invalid("Property '" + name + "' must be an integer.");
    }

    private static bool Boolean(JsonElement value, string name)
    {
        var item = Required(value, name);
        return item.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw Invalid("Property '" + name + "' must be a boolean."),
        };
    }

    private static TestEvidenceTarget ReadTarget(JsonElement target, string description)
    {
        var value = Object(target, description, "kind", "epic", "feature", "id", "state", "featureState", "relativePath", "line");
        return new TestEvidenceTarget(NonEmpty(String(value, "kind"), description + ".kind"),
            NullableString(value, "epic"), NullableString(value, "feature"), NonEmpty(String(value, "id"), description + ".id"),
            NullableString(value, "state"), NullableString(value, "featureState"), Path(String(value, "relativePath")),
            Positive(Integer(value, "line"), description + ".line"));
    }

    private static TestEvidenceLocation? ReadLocation(JsonElement location, string description)
    {
        if (location.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var value = Object(location, description, "path", "line", "column");
        return new TestEvidenceLocation(Path(String(value, "path")), Positive(Integer(value, "line"), description + ".line"),
            Positive(Integer(value, "column"), description + ".column"));
    }

    private static IEnumerable<JsonElement> Array(JsonElement value, string name)
    {
        var item = Required(value, name);
        return item.ValueKind == JsonValueKind.Array ? item.EnumerateArray().ToArray()
            : throw Invalid("Property '" + name + "' must be an array.");
    }

    private static string NonEmpty(string? value, string name) => !string.IsNullOrEmpty(value)
        ? value! : throw Invalid(name + " must be nonempty.");

    private static int Positive(int value, string name) => value > 0 ? value : throw Invalid(name + " must be positive.");

    private static int NonNegative(int value, string name) => value >= 0 ? value : throw Invalid(name + " cannot be negative.");

    private static string Digest(string value, string name) => value.Length == 64 && value.All(static character =>
        character is >= '0' and <= '9' or >= 'a' and <= 'f') ? value : throw Invalid(name + " must be a lowercase SHA-256 digest.");

    private static string Path(string value)
    {
        if (string.IsNullOrEmpty(value) || value[0] == '/' || value.Contains('\\')
            || (value.Length > 1 && char.IsLetter(value[0]) && value[1] == ':')
            || value.Split('/').Any(static component => component is "" or "." or ".."))
        {
            throw Invalid("Paths must be normalized repository-relative paths.");
        }

        return value;
    }

    private static string? OptionalPath(JsonElement value, string name)
    {
        if (!value.TryGetProperty(name, out var path))
        {
            return null;
        }

        return path.ValueKind == JsonValueKind.Null ? null : path.ValueKind == JsonValueKind.String
            ? Path(path.GetString()!) : throw Invalid("Optional path must be a string or null.");
    }

    private static ContractException Invalid(string message) => new(message);

    private sealed class ContractException(string message) : Exception(message);

    private static readonly HashSet<string> Outcomes = new(StringComparer.Ordinal)
        { "passed", "failed", "skipped", "notRun", "aborted" };

    private static readonly HashSet<string> BindingStatuses = new(StringComparer.Ordinal)
        { "incomplete", "invalid", "unresolved", "ambiguous", "resolved" };

    private static readonly HashSet<string> Roles = new(StringComparer.Ordinal)
        { "spec", "implements", "verifies", "follows" };

    private static readonly HashSet<string> CatalogElements = new(StringComparer.Ordinal)
        { "feature", "rule", "scenario", "scenarioOutline" };

    private static readonly HashSet<string> InputKinds = new(StringComparer.Ordinal)
        { "source", "generated-source", "manifest", "schema", "record" };

}

