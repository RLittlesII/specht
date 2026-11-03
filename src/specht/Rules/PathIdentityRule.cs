using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Specht.Discovery;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// SPEC011 - a specification's frontmatter identity agrees with where it sits: from schema 0.3.0, it carries an epic
/// where the manifest declares the epic grammar, and only its Feature segment is checked where it declares none
/// (B-035, B-042, decision 0020).
/// </summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class PathIdentityRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC011";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F5" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-009" />
    /// <implements epic="0001" feature="F5" claim="B-035" />
    /// <implements epic="0001" feature="F5" claim="B-042" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        var structure = model.Schemas.Structure;

        foreach (var feature in model.Features)
        {
            var segments = feature.RelativePath.Split('/');
            var layoutIdentity = feature.Location.Layout.Identity;

            if (structure.Forms is not null && structure.DeclaresEpicGrammar && feature.Epic is null && feature.Id is not null)
            {
                yield return MissingEpic(feature, layoutIdentity, segments, structure.Identifiers["epic"]);
                continue;
            }

            if (layoutIdentity is not { } identity
                || (feature.Epic is null && !structure.EpicLess)
                || feature.Id is null
                || identity.Epic >= segments.Length - 1
                || identity.Feature >= segments.Length - 1)
            {
                continue;
            }

            if (!structure.EpicLess
                && identity.Epic is { } epicSegment
                && segments[epicSegment] is var epicDirectory
                && !epicDirectory.StartsWith(feature.Epic + "-", StringComparison.Ordinal))
            {
                yield return new SpechtFinding(
                    feature.RelativePath,
                    feature.Document.Frontmatter.LineOf(feature.Keys.Epic),
                    feature.Identity,
                    $"frontmatter {feature.Keys.Epic} '{feature.Epic}' does not match the containing directory '{epicDirectory}'")
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
                    feature.Document.Frontmatter.LineOf(feature.Keys.Id),
                    feature.Identity,
                    $"frontmatter {feature.Keys.Id} '{feature.Id}' does not match the containing directory '{featureDirectory}'")
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

    /// <implements epic="0001" feature="F5" claim="B-035" />
    private static SpechtFinding MissingEpic(FeatureSpec feature, SpechtPathIdentity? identity, string[] segments, string grammar)
    {
        var expected = new JsonObject { ["form"] = grammar };
        if (identity?.Epic is { } epicSegment && epicSegment < segments.Length - 1)
        {
            expected["value"] = DirectoryIdentity(segments[epicSegment], grammar);
        }

        return new SpechtFinding(
            feature.RelativePath,
            feature.Document.Frontmatter.LineOf(feature.Keys.Id),
            feature.Identity,
            $"frontmatter has no {feature.Keys.Epic} - the manifest declares the epic grammar")
        {
            Expected = expected,
        };
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
