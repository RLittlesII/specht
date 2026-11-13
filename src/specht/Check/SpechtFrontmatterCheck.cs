using Specht.Evaluation;
using Specht.Model;
using Specht.Rules;

namespace Specht.Check;

/// <summary>
/// Checks one document's frontmatter against the root's selected schemas of the pinned version (<c>0001-F8</c> B-006;
/// item <c>0167</c>): an evaluator over the four frontmatter rules selects, stamps, counts and orders them as the tree's does.
/// </summary>
/// <param name="reader">The model reader, for the pinned schemas.</param>
/// <param name="rules">The registered rule set, from which the frontmatter rules are taken.</param>
/// <param name="order">The violation order a document's evaluation sorts with.</param>
/// <param name="frontmatter">The frontmatter parser.</param>
/// <param name="check">The frontmatter schema evaluation, for the manifest's kind names.</param>
/// <implements epic="0001" feature="F8" claim="B-006" />
public sealed class SpechtFrontmatterCheck(
    SpechtModelReader reader,
    IEnumerable<ISpechtRule> rules,
    SpechtViolationOrder order,
    FrontmatterReader frontmatter,
    FrontmatterSchemaCheck check)
{
    /// <summary>Parses a frontmatter kind before the caller reads its document or root.</summary>
    /// <param name="kind">The kind as typed.</param>
    /// <returns>The kind.</returns>
    /// <implements epic="0001" feature="F8" claim="B-006" />
    public FrontmatterKind ParseKind(string kind) => check.ParseKind(kind);

    /// <summary>
    /// Checks only the supplied document's frontmatter against the root's selected schemas of the pinned version, whatever
    /// the manifest's pin policy (<c>0001-F7</c> OQ-73), each rule's findings taken from the document.
    /// </summary>
    /// <param name="root">The root whose manifest pins the schemas.</param>
    /// <param name="kind">The document's kind.</param>
    /// <param name="path">The path the document is reported under.</param>
    /// <param name="text">The document's text.</param>
    /// <returns>The report.</returns>
    /// <implements epic="0001" feature="F8" claim="B-006" />
    public SpechtReport Run(string root, FrontmatterKind kind, string path, string text)
    {
        var loaded = reader.ReadPinnedSchemas(root);
        var parsed = frontmatter.Parse(text);

        return new SpechtEvaluator(FrontmatterRules, order).Evaluate(new SpechtModel([], [], [], loaded, []), rule => rule switch
        {
            FrontmatterPresentRule present => present.Evaluate(path, parsed, loaded, kind),
            FeatureFrontmatterRule feature when kind == FrontmatterKind.Feature => feature.Evaluate(path, parsed, loaded),
            ItemFrontmatterRule item when kind == FrontmatterKind.Task => item.Evaluate(path, parsed, loaded),
            EpicFrontmatterRule epic when kind == FrontmatterKind.Epic => epic.Evaluate(path, parsed, loaded),
            _ => [],
        });
    }

    private List<ISpechtRule> FrontmatterRules =>
    [
        .. rules
            .Select(static rule => rule is FaultIsolatedRule isolated ? isolated.Inner : rule)
            .Where(static rule => rule is FrontmatterPresentRule or FeatureFrontmatterRule or ItemFrontmatterRule or EpicFrontmatterRule),
    ];
}
