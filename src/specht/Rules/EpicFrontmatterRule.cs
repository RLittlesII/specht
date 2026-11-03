using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC004 - every epic file's frontmatter satisfies the epic schema in <c>.specht/schema/frontmatter/</c>.</summary>
/// <param name="check">The frontmatter schema evaluation the three schema rules share.</param>
/// <implements epic="0001" feature="F5" claim="B-008" />
public sealed class EpicFrontmatterRule(FrontmatterSchemaCheck check) : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC004";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F5" claim="B-008" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        foreach (var epic in model.Epics)
        {
            foreach (var finding in Evaluate(epic.RelativePath, epic.Frontmatter, model.Schemas))
            {
                yield return finding;
            }
        }
    }

    /// <summary>Evaluates one epic document, as the tree's walk does each of its own.</summary>
    /// <param name="path">The document's repository-relative path.</param>
    /// <param name="frontmatter">The document's frontmatter.</param>
    /// <param name="schemas">The selected schemas.</param>
    /// <returns>Each failure of the epic schema; none for a document with no frontmatter.</returns>
    /// <implements epic="0001" feature="F5" claim="B-008" />
    public IEnumerable<SpechtFinding> Evaluate(string path, Frontmatter frontmatter, SpechtSchemas schemas) =>
        check.Evaluate(path, frontmatter, schemas, FrontmatterKind.Epic);
}
