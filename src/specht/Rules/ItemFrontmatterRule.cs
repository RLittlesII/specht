using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC003 - every item file's frontmatter satisfies the item schema in <c>.specht/schema/frontmatter/</c>.</summary>
/// <param name="check">The frontmatter schema evaluation the three schema rules share.</param>
/// <implements epic="0001" feature="F5" claim="B-008" />
public sealed class ItemFrontmatterRule(FrontmatterSchemaCheck check) : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC003";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F5" claim="B-008" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        foreach (var item in model.Items)
        {
            foreach (var finding in Evaluate(item.RelativePath, item.Frontmatter, model.Schemas))
            {
                yield return finding;
            }
        }
    }

    /// <summary>Evaluates one task-shaped document, as the tree's walk does each of its own.</summary>
    /// <param name="path">The document's repository-relative path.</param>
    /// <param name="frontmatter">The document's frontmatter.</param>
    /// <param name="schemas">The selected schemas.</param>
    /// <returns>Each failure of the item schema; none for a document with no frontmatter.</returns>
    /// <implements epic="0001" feature="F5" claim="B-008" />
    public IEnumerable<SpechtFinding> Evaluate(string path, Frontmatter frontmatter, SpechtSchemas schemas) =>
        check.Evaluate(path, frontmatter, schemas, FrontmatterKind.Task);
}
