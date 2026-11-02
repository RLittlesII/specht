using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC043 - each item's id agrees with its file name, and its parent with the Feature whose directory holds it.</summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class ItemPlacementRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC043";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        foreach (var item in model.Items)
        {
            if (item.Id is null)
            {
                continue;
            }

            if (!string.Equals(item.Id, item.FileNameId, StringComparison.Ordinal))
            {
                yield return new SpechtFinding(
                    item.RelativePath,
                    item.Frontmatter.LineOf("id"),
                    item.Id,
                    $"frontmatter id '{item.Id}' does not match the file name prefix '{item.FileNameId}'")
                {
                    Expected = new JsonObject { ["form"] = model.Schemas.Structure.Identifiers["task"], ["value"] = item.FileNameId },
                };
            }

            var parent = model.Features.FirstOrDefault(feature =>
                string.Equals(feature.Location.Directory, item.ParentDirectory, StringComparison.Ordinal));

            if (parent is not null && item.Parent is not null
                                   && !string.Equals(item.Parent, parent.Identity, StringComparison.Ordinal))
            {
                yield return new SpechtFinding(
                    item.RelativePath,
                    item.Frontmatter.LineOf("parent"),
                    item.Id,
                    $"frontmatter parent '{item.Parent}' does not match the Feature it sits in ('{parent.Identity}')")
                {
                    Expected = new JsonObject { ["form"] = "{epic}-{feature}", ["value"] = parent.Identity },
                };
            }
        }
    }
}
