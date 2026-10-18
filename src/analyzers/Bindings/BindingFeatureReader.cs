using System;
using System.Linq;
using System.Text.Json.Nodes;
using Markdig;
using Specht.Model;

namespace Specht.Analysis.Bindings;

/// <summary>Projects supplied Feature text through the engine's shared text-reading semantics.</summary>
public sealed class BindingFeatureReader
{
    public BindingFeature Read(string text, string relativePath, BindingPolicy policy)
    {
        new BindingProvenanceGuard().Validate(relativePath);
        var frontmatter = new FrontmatterTextReader().Read(text);
        var section = new MarkdownTextReader().Read(text, _pipeline)
            .FirstOrDefault(section => string.Equals(section.Title, policy.ClaimsSection, StringComparison.Ordinal));
        var claims = section is null ? [] : section.Rows.Select((row, index) =>
            new BindingClaim(row.ElementAtOrDefault(0)?.Trim() ?? string.Empty, row.ElementAtOrDefault(3)?.Trim(), section.RowLines[index])).ToArray();
        return new BindingFeature(
            new FeatureIdentity(policy.DeclaresEpicGrammar ? Text(frontmatter.Node, "epic") : null, Text(frontmatter.Node, "id")),
            Text(frontmatter.Node, "spec_status"), claims, relativePath);
    }

    private static string? Text(JsonObject? mapping, string key) =>
        mapping?[key] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
}
