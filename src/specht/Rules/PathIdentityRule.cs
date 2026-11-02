using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// SPEC011 - a specification's frontmatter identity agrees with where it sits.
/// </summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class PathIdentityRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC011";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
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
                yield return new SpechtFinding(
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
                yield return new SpechtFinding(
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
