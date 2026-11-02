using System.IO.Abstractions;
using Specht.Evaluation;
using Specht.Manifest;
using Specht.Model;
using Specht.Report;
using Specht.Rules;

namespace Specht;

/// <summary>
/// Reads the resolved specification tree through the model reader and evaluates it with the engine's evaluator.
/// </summary>
/// <param name="reader">The model reader.</param>
/// <param name="evaluator">The evaluator over the registered rule set.</param>
/// <param name="rules">The registered rule set, from which the frontmatter rules are taken for one document.</param>
/// <param name="order">The violation order a document's evaluation sorts with.</param>
/// <param name="fileSystem">The file system a report is written through.</param>
/// <param name="frontmatter">The frontmatter parser.</param>
/// <param name="check">The frontmatter schema evaluation, for the manifest's kind names.</param>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class SpechtRunner(
    SpechtModelReader reader,
    SpechtEvaluator evaluator,
    IEnumerable<ISpechtRule> rules,
    SpechtViolationOrder order,
    IFileSystem fileSystem,
    FrontmatterReader frontmatter,
    FrontmatterSchemaCheck check)
{
    /// <summary>Loads and evaluates the tree using the catalog supplied at composition.</summary>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public SpechtReport Run(string root) => Run(root, null);

    /// <summary>Loads and evaluates the tree with the frontmatter source selected for this invocation.</summary>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public SpechtReport Run(string root, SchemaSourceSelection? selection) => evaluator.Evaluate(reader.Read(root, selection));

    /// <summary>Parses a frontmatter kind before the caller reads its document or root.</summary>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public FrontmatterKind ParseFrontmatterKind(string kind) => check.ParseKind(kind);

    /// <summary>
    /// Checks only the supplied document's frontmatter against the root's selected schemas: an evaluator over the four
    /// frontmatter rules selects, stamps, counts and orders them as the tree's does, each rule's findings taken from the
    /// document.
    /// </summary>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public SpechtReport CheckFrontmatter(string root, FrontmatterKind kind, string path, string text)
    {
        var loaded = reader.ReadSchemas(root, null);
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

    /// <summary>Writes <paramref name="report"/> to <paramref name="path"/> as the report document (<c>0001-F3</c> B-008).</summary>
    /// <param name="report">The run's report.</param>
    /// <param name="path">Where the document is written.</param>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public void WriteReport(SpechtReport report, string path)
    {
        fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(path)!);
        fileSystem.File.WriteAllText(path, SpechtReportDocument.From(report).ToJson());
    }

    private List<ISpechtRule> FrontmatterRules =>
    [
        .. rules
            .Select(static rule => rule is FaultIsolatedRule isolated ? isolated.Inner : rule)
            .Where(static rule => rule is FrontmatterPresentRule or FeatureFrontmatterRule or ItemFrontmatterRule or EpicFrontmatterRule),
    ];
}
