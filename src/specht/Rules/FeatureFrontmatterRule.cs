using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// SPEC002 - every Feature specification's frontmatter satisfies the Feature schema in
/// <c>.specht/schema/frontmatter/</c>.
/// </summary>
/// <remarks>
/// SPEC005 - a linked specification records when it last mirrored - is
/// expressed inside the Feature schema as an <c>if</c>/<c>then</c>, because
/// "github_issue set implies synced_at present" is a statement about one
/// file's frontmatter and belongs with the rest of them.
/// </remarks>
/// <param name="check">The frontmatter schema evaluation the three schema rules share.</param>
/// <implements epic="0001" feature="F5" claim="B-008" />
public sealed class FeatureFrontmatterRule(FrontmatterSchemaCheck check) : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC002";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F5" claim="B-008" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        foreach (var feature in model.Features)
        {
            foreach (var finding in Evaluate(feature.RelativePath, feature.Document.Frontmatter, model.Schemas))
            {
                yield return finding;
            }
        }
    }

    /// <summary>Evaluates one Feature document, as the tree's walk does each of its own.</summary>
    /// <param name="path">The document's repository-relative path.</param>
    /// <param name="frontmatter">The document's frontmatter.</param>
    /// <param name="schemas">The selected schemas.</param>
    /// <returns>Each failure of the Feature schema; none for a document with no frontmatter.</returns>
    /// <implements epic="0001" feature="F5" claim="B-008" />
    public IEnumerable<SpechtFinding> Evaluate(string path, Frontmatter frontmatter, SpechtSchemas schemas) =>
        check.Evaluate(path, frontmatter, schemas, FrontmatterKind.Feature);
}
