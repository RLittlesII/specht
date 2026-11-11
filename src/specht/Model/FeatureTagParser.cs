using System.Text.RegularExpressions;
using Specht.Manifest;

namespace Specht.Model;

/// <summary>Parses companion text using the selected manifest's tag contract without I/O.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class FeatureTagParser
{
    /// <summary>Returns claim tags and their one-based lines; an absent form retains the legacy scanner.</summary>
    /// <param name="text">The companion's complete text.</param>
    /// <param name="structure">The effective manifest, or null for the legacy scanner.</param>
    /// <returns>Each recognized claim tag in file order.</returns>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public IReadOnlyList<FeatureTag> Parse(string text, SpechtStructure? structure = null) => Scan(text, structure, null).Tags;

    /// <summary>Reads a companion's claim tags and its scenarios, with the tags Gherkin gives each, in one pass.</summary>
    /// <param name="relativePath">The path relative to the checked root.</param>
    /// <param name="text">The companion's complete text.</param>
    /// <param name="structure">The effective manifest, or null for the legacy scanner.</param>
    /// <param name="grammar">The claim grammar of the Feature the companion sits beside, or null for the manifest's.</param>
    /// <returns>The companion snapshot.</returns>
    /// <implements epic="0001" feature="F12" claim="B-008" />
    /// <implements epic="0001" feature="F5" claim="B-080" />
    public FeatureFile Read(string relativePath, string text, SpechtStructure? structure = null, SpechtClaimGrammar? grammar = null)
    {
        var (tags, scenarios) = Scan(text, structure, grammar);
        return new FeatureFile(relativePath, tags, scenarios);
    }

    /// <summary>The text after the <c>@</c> of every tag on a tag line of a companion, comment lines skipped.</summary>
    /// <param name="text">The companion's complete text.</param>
    /// <returns>Each tag's name, in file order.</returns>
    /// <implements epic="0001" feature="F19" claim="B-042" />
    public IReadOnlyList<string> TagNames(string text) => [];

    /// <implements epic="0001" feature="F1" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-080" />
    private static (IReadOnlyList<FeatureTag> Tags, IReadOnlyList<FeatureScenario> Scenarios) Scan(
        string text,
        SpechtStructure? structure,
        SpechtClaimGrammar? claimGrammar)
    {
        var tags = new List<FeatureTag>();
        var form = structure?.ClaimTagForm;
        var parts = form?.Split("{claim}", StringSplitOptions.None);
        var ignoreCase = structure?.ClaimTagIgnoreCase == true;
        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var grammar = form is null ? null : new Regex(
            $"\\A(?:{claimGrammar?.Pattern ?? structure!.Identifiers["claim"]})\\z",
            RegexOptions.CultureInvariant | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None));
        var scenarios = new List<(string Title, int Line, List<FeatureTag> Tags)>();
        List<FeatureTag> pending = [];
        List<FeatureTag> feature = [];
        List<FeatureTag> rule = [];
        List<FeatureTag>? outline = null;
        using var reader = new StringReader(text);
        var lineNumber = 0;
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            var trimmed = line.Trim();
            if (trimmed.StartsWith('#'))
            {
                continue;
            }

            var found = new List<FeatureTag>();
            if (parts is null)
            {
                found.AddRange(LegacyClaimTag.Matches(line).Cast<Match>().Select(match => new FeatureTag(match.Groups[1].Value, lineNumber)));
            }
            else
            {
                foreach (var token in line.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries))
                {
                    var length = token.Length - parts[0].Length - parts[1].Length;
                    if (length < 0 || !token.StartsWith(parts[0], comparison) || !token.EndsWith(parts[1], comparison))
                    {
                        continue;
                    }

                    var claim = token.Substring(parts[0].Length, length);
                    if (grammar!.IsMatch(claim))
                    {
                        found.Add(new FeatureTag(claim, lineNumber) { Text = token });
                    }
                }
            }

            tags.AddRange(found);
            if (trimmed.Length == 0)
            {
                continue;
            }

            if (trimmed[0] == '@')
            {
                pending.AddRange(found);
                continue;
            }

            if (trimmed.StartsWith("Feature:", StringComparison.Ordinal))
            {
                feature = pending;
            }
            else if (trimmed.StartsWith("Rule:", StringComparison.Ordinal))
            {
                rule = pending;
                outline = null;
            }
            else if (Title(trimmed) is { } title)
            {
                outline = [.. feature, .. rule, .. pending];
                scenarios.Add((title, lineNumber, outline));
            }
            else if (trimmed.StartsWith("Examples:", StringComparison.Ordinal) || trimmed.StartsWith("Scenarios:", StringComparison.Ordinal))
            {
                outline?.AddRange(pending);
            }
            else if (Keywords.Any(keyword => trimmed.StartsWith(keyword, StringComparison.Ordinal)))
            {
                outline = null;
            }

            pending = [];
        }

        return (tags, scenarios.Select(static scenario => new FeatureScenario(scenario.Title, scenario.Line, scenario.Tags.ToArray())).ToArray());
    }

    /// <implements epic="0001" feature="F12" claim="B-008" />
    private static string? Title(string line) =>
        line.StartsWith("Scenario:", StringComparison.Ordinal) ? line["Scenario:".Length..].Trim()
        : line.StartsWith("Scenario Outline:", StringComparison.Ordinal) ? line["Scenario Outline:".Length..].Trim()
        : null;

    private static readonly string[] Keywords = ["Background:", "Example:", "Scenario Template:"];

    private static readonly Regex LegacyClaimTag = new(@"@(B-\d{3}[a-z]?)\b", RegexOptions.Compiled);
}
