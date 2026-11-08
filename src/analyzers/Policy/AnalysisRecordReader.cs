using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Specht.Analysis.Bindings;
using Specht.Model;

namespace Specht.Analysis.Policy;

/// <summary>Reads consumer-declared record identities and lifecycles using mapped field roles.</summary>
internal sealed class AnalysisRecordReader
{
    public BindingFeature Feature(AnalysisInput input, AnalysisPolicy policy, JsonNode? settings)
    {
        var section = new MarkdownTextReader().Read(input.Text!, _pipeline)
            .FirstOrDefault(section => section.Title == policy.Bindings.ClaimsSection);
        if (section is null || !section.Headers.Contains(Setting(settings, "claimIdColumn", "ID"), StringComparer.Ordinal))
        {
            throw new ArgumentException("The configured consumer claims section or ID column is missing.");
        }

        var feature = new BindingFeatureReader().ReadMapped(input.Text!, input.RelativePath, policy.Bindings,
            Setting(settings, "claimStatusColumn", "Status"), Setting(settings, "featureLifecycleKey", "spec_status"),
            Setting(settings, "featureIdentityKey", "id"), Setting(settings, "featureEpicKey", "epic"),
            Setting(settings, "claimIdColumn", "ID"));
        var identifiers = policy.Bindings.Identifiers;
        Identifier("feature", feature.Identity.Feature, identifiers);
        if (policy.Bindings.DeclaresEpicGrammar)
        {
            Identifier("epic", feature.Identity.Epic, identifiers);
        }

        if (feature.State is { } state && state is not ("draft" or "in-review" or "approved" or "superseded")
            && !(policy.Bindings.Markers.TryGetValue("approved", out var approved) && approved == state)
            && !(policy.Bindings.Markers.TryGetValue("superseded", out var superseded) && superseded == state))
        {
            throw new ArgumentException("A supplied Feature has an unknown lifecycle value.");
        }

        foreach (var claim in feature.Claims)
        {
            Identifier("claim", claim.Id, identifiers);
            if (claim.State is { } status && status is not ("Active" or "Amended" or "Withdrawn" or "Superseded")
                && !(policy.Bindings.Markers.TryGetValue("withdrawn", out var withdrawn) && withdrawn == status)
                && !(policy.Bindings.Markers.TryGetValue("superseded", out var retired) && retired == status))
            {
                throw new ArgumentException("A supplied claim has an unknown lifecycle value.");
            }
        }

        return feature;
    }

    public BindingAdr Adr(AnalysisInput input, AnalysisPolicy policy, JsonNode? settings, IReadOnlyList<BindingFeature> features)
    {
        var profiles = (settings?["adrProfiles"]?.AsArray() ?? new JsonArray())
            .Where(profile => _paths.Matches(input.RelativePath, profile!["glob"]!.GetValue<string>())).ToArray();
        if (profiles.Length != 1)
        {
            throw new ArgumentException("A supplied ADR requires exactly one matching record profile.");
        }

        var profile = profiles[0]!;
        var document = Markdown.Parse(input.Text!, _pipeline);
        var frontmatter = new FrontmatterTextReader().Read(input.Text!).Node;
        var id = Selected(document, frontmatter, profile["identity"]!, false);
        Identifier("adr", id, policy.Bindings.Identifiers);
        var lifecycle = Selected(document, frontmatter, profile["lifecycle"]!, true)?.ToLowerInvariant();
        if (lifecycle is not (null or "accepted" or "proposed" or "rejected" or "deprecated" or "superseded"))
        {
            throw new ArgumentException("A supplied ADR has an unknown lifecycle value.");
        }

        FeatureIdentity? owner = null;
        if (profile["owner"]!.GetValue<string>() == "containingFeature")
        {
            var owners = features.Where(feature => Contains(feature.RelativePath, input.RelativePath)).ToArray();
            if (owners.Length != 1)
            {
                throw new ArgumentException("A Feature-owned ADR requires one discovered containing Feature.");
            }

            owner = owners[0].Identity;
        }

        return new BindingAdr(id!, owner, lifecycle, input.RelativePath);
    }

    private static string Setting(JsonNode? settings, string key, string fallback) => settings?[key]?.GetValue<string>() ?? fallback;

    private static bool Contains(string feature, string adr)
    {
        var index = feature.LastIndexOf('/');
        return index >= 0 && adr.StartsWith(feature.Substring(0, index + 1), StringComparison.Ordinal);
    }

    private static string? Selected(MarkdownDocument document, JsonObject? frontmatter, JsonNode selector, bool lifecycle)
    {
        if (selector["frontmatterKey"] is { } key)
        {
            return frontmatter?[key.GetValue<string>()]?.GetValue<string>();
        }

        if (!lifecycle)
        {
            var headings = document.OfType<HeadingBlock>().Where(heading => heading.Level == selector["headingLevel"]!.GetValue<int>()).ToArray();
            if (headings.Length != 1)
            {
                throw new ArgumentException("An ADR identity must select exactly one heading.");
            }

            var title = Text(headings[0]);
            var index = title.IndexOf(selector["separator"]!.GetValue<string>(), StringComparison.Ordinal);
            if (index < 0)
            {
                throw new ArgumentException("The configured ADR identity separator is absent.");
            }

            return title.Substring(0, index).Trim();
        }

        var selected = document.OfType<HeadingBlock>().Where(heading => Text(heading) == selector["section"]!.GetValue<string>()).ToArray();
        if (selected.Length != 1)
        {
            return null;
        }

        var start = document.IndexOf(selected[0]) + 1;
        var paragraph = document.Skip(start).TakeWhile(block => block is not HeadingBlock heading || heading.Level > selected[0].Level)
            .OfType<ParagraphBlock>().FirstOrDefault();
        return paragraph is null ? null : Text(paragraph).Split(new[] { ' ', '\t', '\r', '\n', ',', '.' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
    }

    private static string Text(MarkdownObject node) => string.Concat(((node as LeafBlock)?.Inline ?? node).Descendants().Select(static part => part switch
    {
        LiteralInline literal => literal.ToString(),
        CodeInline code => code.Content,
        _ => string.Empty,
    })).Trim();

    private static void Identifier(string role, string? value, IReadOnlyDictionary<string, string> identifiers)
    {
        if (string.IsNullOrWhiteSpace(value)
            || identifiers.TryGetValue(role, out var grammar) && !new Regex("\\A(?:" + grammar + ")\\z", RegexOptions.CultureInvariant).IsMatch(value!))
        {
            throw new ArgumentException("A supplied record identity violates its consumer grammar.");
        }
    }

    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
    private readonly AnalysisPaths _paths = new();
}
