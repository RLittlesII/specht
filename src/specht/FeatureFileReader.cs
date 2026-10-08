using System.Text.RegularExpressions;

namespace specht;

/// <summary>Scans a Gherkin file for the claim tags its scenarios carry.</summary>
/// <remarks>
/// Comment-aware by necessity: a <c>#</c>-commented line in one of this
/// repository's feature files contains three <c>@B-0nn</c> tokens that are
/// prose, not tags. Treating them as tags reports three phantom orphans.
/// </remarks>
public static class FeatureFileReader
{
    /// <summary>Reads every claim tag in the file at <paramref name="absolutePath"/>.</summary>
    public static IReadOnlyList<FeatureTag> ReadTags(string absolutePath)
    {
        var tags = new List<FeatureTag>();
        var lines = File.ReadAllLines(absolutePath);

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];

            if (line.AsSpan().TrimStart().StartsWith("#"))
            {
                continue;
            }

            foreach (var match in ClaimTag.Matches(line).Cast<Match>())
            {
                tags.Add(new FeatureTag(match.Groups[1].Value, index + 1));
            }
        }

        return tags;
    }

    private static readonly Regex ClaimTag = new(@"@(B-\d{3}[a-z]?)\b", RegexOptions.Compiled);
}
