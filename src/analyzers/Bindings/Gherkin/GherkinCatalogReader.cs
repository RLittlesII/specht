using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Specht.Analysis.Policy;
using Specht.Model;

namespace Specht.Analysis.Bindings.Gherkin;

/// <summary>Builds the Feature records and findings of a native claim catalogue from delivered claim files and records.</summary>
/// <implements epic="0201" feature="F7" claim="B-003" />
public sealed class GherkinCatalogReader
{
    public BindingCatalog Read(
        AnalysisPolicy policy, IReadOnlyList<AnalysisInput> inputs, string lifecycleKey, Action<AnalysisInput, JsonNode?> validateRecord)
    {
        var gherkin = policy.Gherkin!;
        var reader = new GherkinClaimFileReader(gherkin);
        var files = gherkin.FileSets.ToDictionary(static set => set, static _ => new List<GherkinClaimFile>());
        foreach (var input in inputs.Where(static input => input.Kind == "scenario").OrderBy(static input => input.RelativePath, StringComparer.Ordinal))
        {
            var matching = gherkin.FileSets.Where(set => _paths.Matches(input.RelativePath, set.Glob)).ToArray();
            if (matching.Length == 0)
            {
                continue;
            }

            if (matching.Length > 1)
            {
                throw new GherkinInputException(input.RelativePath, "A claim file matches more than one claim file set.");
            }

            var set = matching[0];
            var file = reader.Read(input.RelativePath,
                input.Text ?? throw new GherkinInputException(input.RelativePath, "Claim file is missing or unreadable."));
            if (file.FeatureTagValues.Any(value => value != set.Feature) || file.EpicTagValues.Any(value => value != set.Epic))
            {
                throw new GherkinInputException(input.RelativePath, "Feature or epic tag names a different identity than its claim file set.");
            }

            files[set].Add(file);
        }

        if (files.Values.Any(static claimFiles => claimFiles.Count == 0))
        {
            throw new GherkinInputException(null, "A claim file set matches no supplied scenario input.");
        }

        var states = new Dictionary<GherkinClaimFileSet, (string Path, string? State)>();
        foreach (var set in gherkin.FileSets.Where(static set => set.Record is not null))
        {
            states[set] = Record(set.Record!, inputs, policy, lifecycleKey, validateRecord);
        }

        foreach (var selected in policy.Features)
        {
            if (!gherkin.FileSets.Any(set => set.Epic == selected.Epic && set.Feature == selected.Feature))
            {
                throw new GherkinInputException(null, "A selected consumer Feature has no claim file set.");
            }
        }

        var features = new List<BindingFeature>();
        var findings = new List<BindingFinding>();
        foreach (var set in gherkin.FileSets)
        {
            var claims = files[set].SelectMany(static file => file.Claims).ToArray();
            findings.AddRange(files[set].SelectMany(static file => file.Findings));
            var duplicates = claims.GroupBy(static claim => claim.Id, StringComparer.Ordinal).Where(static group => group.Count() > 1);
            foreach (var duplicate in duplicates.SelectMany(static group => group))
            {
                findings.Add(new BindingFinding("SPCT0010", duplicate.RelativePath!, duplicate.Line,
                    "Claim " + duplicate.Id + " is declared more than once in Feature " + set.Feature + "."));
            }

            var (path, state) = states.TryGetValue(set, out var record) ? record : (files[set][0].RelativePath, "current");
            features.Add(new BindingFeature(new FeatureIdentity(set.Epic, set.Feature), state, claims, path));
        }

        return new BindingCatalog(features, [], findings.OrderBy(static finding => finding.RelativePath, StringComparer.Ordinal)
            .ThenBy(static finding => finding.Line).ThenBy(static finding => finding.DiagnosticId, StringComparer.Ordinal).ToArray());
    }

    private (string Path, string? State) Record(
        string record, IReadOnlyList<AnalysisInput> inputs, AnalysisPolicy policy, string lifecycleKey, Action<AnalysisInput, JsonNode?> validateRecord)
    {
        var target = _paths.Normalize(record);
        var input = inputs.FirstOrDefault(candidate => candidate.Kind == "feature" && _paths.Normalize(candidate.RelativePath) == target)
            ?? throw new GherkinInputException(record, "Feature record is not a supplied feature input.");
        var frontmatter = new FrontmatterTextReader().Read(input.Text ?? throw new ArgumentException("A required consumer input could not be read."));
        validateRecord(input, frontmatter.Node);
        var state = frontmatter.Node?[lifecycleKey] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
        var markers = policy.Bindings.Markers;
        if (state is not null && state is not ("draft" or "in-review" or "approved" or "superseded")
            && !(markers.TryGetValue("approved", out var approved) && approved == state)
            && !(markers.TryGetValue("superseded", out var superseded) && superseded == state))
        {
            throw new GherkinInputException(input.RelativePath, "A supplied Feature has an unknown lifecycle value.");
        }

        return (input.RelativePath, state);
    }

    private readonly AnalysisPaths _paths = new();
}
