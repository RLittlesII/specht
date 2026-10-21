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
    public IReadOnlyList<FeatureTag> Parse(string text, SpecStructure? structure = null)
    {
        var tags = new List<FeatureTag>();
        var form = structure?.ClaimTagForm;
        var parts = form?.Split("{claim}", StringSplitOptions.None);
        var ignoreCase = structure?.ClaimTagIgnoreCase == true;
        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var grammar = form is null ? null : new Regex(
            $"\\A(?:{structure!.Identifiers["claim"]})\\z",
            RegexOptions.CultureInvariant | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None));
        using var reader = new StringReader(text);
        var lineNumber = 0;
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            if (line.AsSpan().TrimStart().StartsWith("#"))
            {
                continue;
            }

            if (parts is null)
            {
                tags.AddRange(LegacyClaimTag.Matches(line).Cast<Match>().Select(match => new FeatureTag(match.Groups[1].Value, lineNumber)));
                continue;
            }

            foreach (var token in line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                var length = token.Length - parts[0].Length - parts[1].Length;
                if (length < 0 || !token.StartsWith(parts[0], comparison) || !token.EndsWith(parts[1], comparison))
                {
                    continue;
                }

                var claim = token.Substring(parts[0].Length, length);
                if (grammar!.IsMatch(claim))
                {
                    tags.Add(new FeatureTag(claim, lineNumber) { Text = token });
                }
            }
        }

        return tags;
    }

    private static readonly Regex LegacyClaimTag = new(@"@(B-\d{3}[a-z]?)\b", RegexOptions.Compiled);
}
