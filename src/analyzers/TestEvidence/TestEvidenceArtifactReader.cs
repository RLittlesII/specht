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

            if (String(root, "format") != "specht.test-evidence" || Integer(root, "formatVersion") != 1)
            {
                return TestEvidenceReadResult.Unsupported();
            }

            Object(root, "artifact", "format", "formatVersion", "project", "configuration", "assembly",
                "sourceFingerprint", "policyFingerprint", "adapter", "discovery", "bindings", "results", "inputs", "scenarios");

            var project = Object(Required(root, "project"), "project", "path", "targetFramework");
            var configuration = Object(Required(root, "configuration"), "configuration", "name", "fingerprint");
            var assembly = Object(Required(root, "assembly"), "assembly", "name", "sha256", "path");
            var adapter = Object(Required(root, "adapter"), "adapter", "id", "version", "contractVersion");
            var discovery = ReadDiscovery(Required(root, "discovery"));
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
            var scenarios = ReadScenarios(root);

            var artifact = new TestEvidenceArtifact(
                Path(String(project, "path")), NonEmpty(String(project, "targetFramework"), "project.targetFramework"),
                NonEmpty(String(configuration, "name"), "configuration.name"), Digest(String(configuration, "fingerprint"), "configuration.fingerprint"),
                NonEmpty(String(assembly, "name"), "assembly.name"), Digest(String(assembly, "sha256"), "assembly.sha256"),
                OptionalPath(assembly, "path"), sourceFingerprint, policyFingerprint,
                NonEmpty(String(adapter, "id"), "adapter.id"), NonEmpty(String(adapter, "version"), "adapter.version"),
                NonEmpty(String(adapter, "contractVersion"), "adapter.contractVersion"), discovery, bindings, results, inputs, scenarios);

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

    private static TestEvidenceDiscovery ReadDiscovery(JsonElement element)
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

        var cases = Array(value, "cases").Select(ReadCase).ToArray();
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

    private static TestEvidenceCase ReadCase(JsonElement element)
    {
        var value = Object(element, "case", "caseId", "methodSymbolId", "rowId", "displayName");
        return new TestEvidenceCase(NonEmpty(String(value, "caseId"), "case.caseId"),
            NonEmpty(String(value, "methodSymbolId"), "case.methodSymbolId"), NullableString(value, "rowId"), NullableString(value, "displayName"));
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
            return new TestEvidenceResults(state, null, System.Array.Empty<TestEvidenceOutcome>());
        }

        var value = Object(element, "results", "state", "runId", "cases");
        if (state != "available")
        {
            throw Invalid("results.state is not recognized.");
        }

        var caseIds = new HashSet<string>(discoveredCases.Select(static item => item.CaseId), StringComparer.Ordinal);
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

    private static IReadOnlyList<JsonElement> ReadScenarios(JsonElement root)
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
        if (records.Any(static scenario => scenario.ValueKind != JsonValueKind.Object))
        {
            throw Invalid("Each scenario record must be an object.");
        }

        return records.Select(static scenario => scenario.Clone()).ToArray();
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

    private static readonly HashSet<string> InputKinds = new(StringComparer.Ordinal)
        { "source", "generated-source", "manifest", "schema", "record" };

}

