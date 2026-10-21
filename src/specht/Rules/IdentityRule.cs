using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// SPEC011 and SPEC012 - a specification's frontmatter identity agrees with
/// where it sits, and no identity is claimed by two files.
/// </summary>
/// <remarks>
/// SPEC012 is the migration's safety net. Converting a Feature from
/// <c>epics/</c> to a co-located <c>.spec/</c> folder means removing the old
/// tree in the same change; a copy left behind is the failure mode that
/// produces two homes for one specification, which is the thing the layout
/// exists to end.
/// </remarks>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class IdentityRule : ISpecRule
{
    /// <inheritdoc />
    public string Id => "SPEC011";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => ["SPEC011", "SPEC012"];

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        foreach (var feature in model.Features)
        {
            var segments = feature.RelativePath.Split('/');

            if (feature.Location.Layout.Identity is not { } identity
                || feature.Epic is null
                || feature.Id is null
                || identity.Epic >= segments.Length - 1
                || identity.Feature >= segments.Length - 1)
            {
                continue;
            }

            if (identity.Epic is { } epicSegment
                && segments[epicSegment] is var epicDirectory
                && !epicDirectory.StartsWith(feature.Epic + "-", StringComparison.Ordinal))
            {
                yield return new SpecViolation(
                    "SPEC011",
                    SpecSeverity.Error,
                    feature.RelativePath,
                    feature.Document.Frontmatter.LineOf("epic"),
                    feature.Identity,
                    $"frontmatter epic '{feature.Epic}' does not match the containing directory '{epicDirectory}'")
                {
                    Expected = new JsonObject
                    {
                        ["form"] = model.Schemas.Structure.Identifiers["epic"],
                        ["value"] = DirectoryIdentity(epicDirectory, model.Schemas.Structure.Identifiers["epic"]),
                    },
                };
            }

            if (identity.Feature is { } featureSegment
                && segments[featureSegment] is var featureDirectory
                && !featureDirectory.StartsWith(feature.Id + "-", StringComparison.Ordinal))
            {
                yield return new SpecViolation(
                    "SPEC011",
                    SpecSeverity.Error,
                    feature.RelativePath,
                    feature.Document.Frontmatter.LineOf("id"),
                    feature.Identity,
                    $"frontmatter id '{feature.Id}' does not match the containing directory '{featureDirectory}'")
                {
                    Expected = new JsonObject
                    {
                        ["form"] = model.Schemas.Structure.Identifiers["feature"],
                        ["value"] = DirectoryIdentity(featureDirectory, model.Schemas.Structure.Identifiers["feature"]),
                    },
                };
            }
        }

        var duplicates = model.FeaturesWithIdentity
            .GroupBy(static feature => feature.Identity, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1);

        foreach (var group in duplicates)
        {
            var paths = group.Select(static feature => feature.RelativePath).Order(StringComparer.Ordinal).ToList();

            foreach (var feature in group)
            {
                yield return new SpecViolation(
                    "SPEC012",
                    SpecSeverity.Error,
                    feature.RelativePath,
                    feature.Document.Frontmatter.LineOf("id"),
                    group.Key,
                    $"'{group.Key}' is specified in {paths.Count} places ({string.Join(", ", paths)}) - "
                        + "a migration removes the old tree in the same change")
                {
                    Expected = new JsonObject
                    {
                        ["form"] = "{epic}-{feature}",
                        ["value"] = group.Key,
                        ["firstDeclaration"] = group.First().RelativePath,
                    },
                };
            }
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static string DirectoryIdentity(string directory, string grammar)
    {
        for (var index = directory.IndexOf('-'); index >= 0; index = directory.IndexOf('-', index + 1))
        {
            var prefix = directory[..index];
            if (Regex.IsMatch(prefix, $"\\A(?:{grammar})\\z", RegexOptions.CultureInvariant))
            {
                return prefix;
            }
        }

        var separator = directory.IndexOf('-');
        return separator < 0 ? directory : directory[..separator];
    }

}
