using System;
using System.Linq;
using System.Text.Json.Nodes;
using Markdig;
using Specht.Model;

namespace Specht.Analysis.Bindings;

/// <summary>Projects supplied Feature text through the engine's shared text-reading semantics.</summary>
public sealed class BindingFeatureReader
{
    public BindingFeature Read(string text, string relativePath, BindingPolicy policy) =>
        ReadMapped(text, relativePath, policy, null, "spec_status", "id", "epic", null);

    public BindingFeature ReadMapped(
        string text, string relativePath, BindingPolicy policy, string? statusColumn, string lifecycleKey,
        string identityKey, string epicKey, string? claimIdColumn)
    {
        new BindingProvenanceGuard().Validate(relativePath);
        var frontmatter = new FrontmatterTextReader().Read(text);
        var section = new MarkdownTextReader().Read(text, _pipeline)
            .FirstOrDefault(section => string.Equals(section.Title, policy.ClaimsSection, StringComparison.Ordinal));
        var idColumn = claimIdColumn is null ? 0 : section?.Headers.ToList().IndexOf(claimIdColumn) ?? -1;
        var stateColumn = statusColumn is null ? 3 : section?.Headers.ToList().IndexOf(statusColumn) ?? -1;
        var claims = section is null ? [] : section.Rows.Select((row, index) =>
            new BindingClaim(row.ElementAtOrDefault(idColumn >= 0 ? idColumn : 0)?.Trim() ?? string.Empty,
                stateColumn >= 0 ? row.ElementAtOrDefault(stateColumn)?.Trim() : null, section.RowLines[index])).ToArray();
        return new BindingFeature(
            new FeatureIdentity(policy.DeclaresEpicGrammar ? Text(frontmatter.Node, epicKey) : null, Text(frontmatter.Node, identityKey)),
            Text(frontmatter.Node, lifecycleKey), claims, relativePath);
    }

    private static string? Text(JsonObject? mapping, string key) =>
        mapping?[key] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
}
