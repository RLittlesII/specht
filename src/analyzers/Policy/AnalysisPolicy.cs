using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Policy;

/// <summary>The consumer's immutable selection and reasoned authoring policy.</summary>
public sealed class AnalysisPolicy
{
    public AnalysisPolicy(JsonObject policy, BindingPolicy bindings)
    {
        _policy = policy.DeepClone().AsObject();
        Bindings = bindings;
        Projects = Array.AsReadOnly(_policy["projects"]!.AsArray().Select(project => new AnalysisProject(
            _paths.Normalize(project!["path"]!.GetValue<string>()),
            Array.AsReadOnly(project["targetFrameworks"]!.AsArray().Select(static tfm => tfm!.GetValue<string>()).ToArray()))).ToArray());
        Features = Array.AsReadOnly(_policy["features"]!.AsArray().Select(static feature =>
            new FeatureIdentity(feature!["epic"]?.GetValue<string>(), feature["feature"]!.GetValue<string>())).ToArray());
    }

    public BindingPolicy Bindings { get; }
    public IReadOnlyList<AnalysisProject> Projects { get; }
    public IReadOnlyList<FeatureIdentity> Features { get; }

    public DeclarationPolicy Select(AnalysisDeclaration declaration)
    {
        var project = _paths.Normalize(declaration.Project);
        var validate = Projects.Any(entry => entry.Path == project && entry.TargetFrameworks.Contains(declaration.TargetFramework, StringComparer.Ordinal));
        var exemptions = (_policy["exemptions"]?.AsArray() ?? new JsonArray())
            .Where(exemption => exemption!["target"]!["kind"]?.GetValue<string>() == "declaration"
                && _paths.Normalize(exemption["target"]!["project"]!.GetValue<string>()) == project
                && exemption["target"]!["symbolId"]!.GetValue<string>() == declaration.SymbolId).ToArray();
        var exemptLinks = exemptions.SelectMany(static exemption => Strings(exemption!["links"]!)).ToArray();
        var roles = !validate || declaration.Generated ? Array.Empty<string>()
            : (_policy["enforcement"]?.AsArray() ?? new JsonArray())
                .Where(scope => _paths.Normalize(scope!["project"]!.GetValue<string>()) == project
                    && Strings(scope["include"]!).Any(glob => _paths.Matches(declaration.RelativePath, glob))
                    && !Strings(scope["exclude"]!).Any(glob => _paths.Matches(declaration.RelativePath, glob))
                    && Strings(scope["declarationKinds"]!).Any(kind => kind == declaration.Kind
                        || kind == "testMethod" && declaration.Kind == "method" && declaration.RecognizedTest
                        || kind == "type" && TypeKinds.Contains(declaration.Kind, StringComparer.Ordinal)))
                .Select(static scope => scope!["role"]!.GetValue<string>())
                .Where(role => (role != "verifies" || declaration.RecognizedTest) && !exemptLinks.Contains(Link(role), StringComparer.Ordinal))
                .Distinct(StringComparer.Ordinal).OrderBy(static role => role, StringComparer.Ordinal).ToArray();
        return new DeclarationPolicy(validate, Array.AsReadOnly(roles),
            Array.AsReadOnly(exemptions.Select(static exemption => exemption!["reason"]!.GetValue<string>()).ToArray()));
    }

    public IReadOnlyList<string> RequiredLinks(ClaimReference claim)
    {
        if (!Features.Any(feature => feature.Epic == claim.Epic && feature.Feature == claim.Feature))
        {
            return Array.Empty<string>();
        }

        var links = new List<string> { "test" };
        foreach (var link in new[] { "implementation", "scenario" })
        {
            if (_policy["requiredLinks"]?[link]?.GetValue<bool>() == true)
            {
                links.Add(link);
            }
        }

        var exempt = ClaimExemptions(claim).SelectMany(static exemption => Strings(exemption["links"]!)).ToArray();
        return Array.AsReadOnly(links.Where(link => !exempt.Contains(link, StringComparer.Ordinal)).ToArray());
    }

    public IReadOnlyList<string> ExemptionReasons(ClaimReference claim) =>
        Array.AsReadOnly(ClaimExemptions(claim).Select(static exemption => exemption["reason"]!.GetValue<string>()).ToArray());

    private static IEnumerable<string> Strings(JsonNode node) => node.AsArray().Select(static entry => entry!.GetValue<string>());
    private static string Link(string role) => role == "verifies" ? "test" : role == "implements" ? "implementation" : role;

    private IEnumerable<JsonNode> ClaimExemptions(ClaimReference claim) =>
        (_policy["exemptions"]?.AsArray() ?? new JsonArray()).Where(exemption => exemption!["target"]!["kind"] is null
            && exemption["target"]!["epic"]?.GetValue<string>() == claim.Epic
            && exemption["target"]!["feature"]!.GetValue<string>() == claim.Feature
            && exemption["target"]!["claim"]!.GetValue<string>() == claim.Claim).Select(static exemption => exemption!);

    private static readonly string[] TypeKinds = { "class", "record", "struct", "interface", "enum", "delegate" };
    private readonly JsonObject _policy;
    private readonly AnalysisPaths _paths = new();
}
