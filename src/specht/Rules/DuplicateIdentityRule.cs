using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// SPEC012 - no identity is claimed by two files.
/// </summary>
/// <remarks>
/// SPEC012 is the migration's safety net. Converting a Feature from
/// <c>epics/</c> to a co-located <c>.spec/</c> folder means removing the old
/// tree in the same change; a copy left behind is the failure mode that
/// produces two homes for one specification, which is the thing the layout
/// exists to end.
/// </remarks>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class DuplicateIdentityRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC012";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F3" claim="B-012" />
    /// <implements epic="0001" feature="F5" claim="B-004" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        var duplicates = model.FeaturesWithIdentity
            .GroupBy(static feature => feature.Identity, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1);

        foreach (var group in duplicates)
        {
            var paths = group.Select(static feature => feature.RelativePath).Order(StringComparer.Ordinal).ToList();

            foreach (var feature in group)
            {
                yield return new SpechtFinding(
                    feature.RelativePath,
                    feature.Document.Frontmatter.LineOf(feature.Keys.Id),
                    group.Key,
                    $"'{group.Key}' is specified in {paths.Count} places ({string.Join(", ", paths)}) - "
                        + "a migration removes the old tree in the same change")
                {
                    Expected = new JsonObject
                    {
                        ["form"] = model.Schemas.Structure.Forms?.Identity ?? "{epic}-{feature}",
                        ["value"] = group.Key,
                        ["firstDeclaration"] = group.First().RelativePath,
                    },
                };
            }
        }
    }
}
