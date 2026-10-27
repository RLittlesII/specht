using System.IO.Abstractions;
using Specht.Manifest;
using Specht.Model;
using Specht.Report;
using Specht.Rules;

namespace Specht;

/// <summary>
/// Runs the rules it was given against the resolved specification tree.
/// </summary>
/// <param name="loader">The model loader.</param>
/// <param name="rules">The rule set, in the order it is evaluated.</param>
/// <param name="fileSystem">The file system a report is written through.</param>
/// <param name="schemas">The shared root and schema selection stage.</param>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class SpechtRunner(
    SpecModelLoader loader, IEnumerable<ISpecRule> rules, IFileSystem fileSystem, SpecSchemasLoader schemas)
{
    /// <summary>
    /// Evaluates <paramref name="rules"/>, in the order given, over <paramref name="model"/> under the vocabulary of the
    /// version its schemas keep (<c>0001-F1</c> B-004; <c>0001-F7</c> B-014; ADR-0008), less the rule ids its manifest sets
    /// <c>off</c> or whose own level is off and not raised, each violation at the severity its manifest sets for its rule id
    /// (<c>0001-F5</c> B-010, B-011, B-061, B-062; ADR-0004).
    /// </summary>
    /// <param name="model">The resolved tree.</param>
    /// <param name="rules">The rule set.</param>
    /// <returns>The report.</returns>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public static SpechtReport Evaluate(SpecModel model, IEnumerable<ISpecRule> rules) =>
        Evaluate(model, rules, rule => rule.Evaluate(model));

    /// <summary>
    /// Orders <paramref name="violations"/> as the report carries them (<c>0001-F1</c> B-004, C-9): severity
    /// descending, then file, line and rule id, file and rule id compared ordinally, and violations equal on all
    /// four keys kept in the order they were given.
    /// </summary>
    /// <param name="violations">The violations to order.</param>
    /// <returns>The violations, ordered.</returns>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public static IReadOnlyList<SpecViolation> Order(IEnumerable<SpecViolation> violations) =>
        violations
            .OrderByDescending(static violation => violation.Severity)
            .ThenBy(static violation => violation.File, StringComparer.Ordinal)
            .ThenBy(static violation => violation.Line)
            .ThenBy(static violation => violation.RuleId, StringComparer.Ordinal)
            .ToList();

    /// <summary>Loads and evaluates the tree using the catalog supplied at composition.</summary>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public SpechtReport Run(string root) => Run(root, null);

    /// <summary>Loads and evaluates the tree with the frontmatter source selected for this invocation.</summary>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public SpechtReport Run(string root, SchemaSourceSelection? selection) => Evaluate(loader.Load(root, selection), rules);

    /// <summary>Parses a frontmatter kind before the caller reads its document or root.</summary>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public FrontmatterKind ParseFrontmatterKind(string kind) => FrontmatterRule.ParseKind(kind);

    /// <summary>Checks only the supplied document's frontmatter against the root's selected schemas.</summary>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public SpechtReport CheckFrontmatter(string root, FrontmatterKind kind, string path, string text)
    {
        var loaded = schemas.Load(root);
        var model = new SpecModel(root, [], [], [], loaded);
        var frontmatter = FrontmatterReader.Parse(text);

        return Evaluate(model, [FrontmatterRule],
            rule => ((FrontmatterSchemaRule)rule).Evaluate(path, frontmatter, loaded, kind));
    }

    /// <summary>Writes <paramref name="report"/> to <paramref name="path"/> as the report document (<c>0001-F3</c> B-008).</summary>
    /// <param name="report">The run's report.</param>
    /// <param name="path">Where the document is written.</param>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public void WriteReport(SpechtReport report, string path)
    {
        fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(path)!);
        fileSystem.File.WriteAllText(path, SpecReportDocument.From(report).ToJson());
    }

    private FrontmatterSchemaRule FrontmatterRule => rules
        .Select(static rule => rule is FaultIsolatedRule isolated ? isolated.Inner : rule)
        .OfType<FrontmatterSchemaRule>().Single();

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static SpechtReport Evaluate(
        SpecModel model, IEnumerable<ISpecRule> rules, Func<ISpecRule, IEnumerable<SpecViolation>> evaluate)
    {
        var version = model.Schemas.Version;
        var settings = model.Schemas.Structure.Rules;
        var evaluated = rules.Where(rule => rule.ReportedIds.Any(id => Enabled(id, rule, version.RuleIds, settings))).ToList();
        var violations = new List<SpecViolation>();

        foreach (var rule in evaluated)
        {
            violations.AddRange(
                evaluate(rule)
                    .Where(violation => violation.IsRuleFailure || Enabled(violation.RuleId, rule, version.RuleIds, settings))
                    .Select(violation => Graded(violation, settings)));
        }

        return new SpechtReport(
            version.Number,
            model.Features.Count,
            [
                .. model.Schemas.Structure.Discovery.Layouts.Select(layout =>
                    new SpecReportLayout(layout.Name, model.Features.Count(feature => feature.Location.Layout == layout))),
            ],
            model.Items.Count,
            evaluated.Sum(rule => rule.ReportedIds.Count(id => Enabled(id, rule, version.RuleIds, settings))),
            Order(violations))
        { SchemaSource = model.Schemas.Source };
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static bool Enabled(string id, ISpecRule rule, IReadOnlySet<string> vocabulary, SpecRuleSettings settings) =>
        vocabulary.Contains(id) && (settings.TryGetValue(id, out var level) ? level : rule.DefaultLevel) != SpecRuleLevel.Off;

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static SpecViolation Graded(SpecViolation violation, SpecRuleSettings settings) =>
        !violation.IsRuleFailure && settings.TryGetValue(violation.RuleId, out var level) ? level switch
        {
            SpecRuleLevel.Warning => violation with { Severity = SpecSeverity.Warning },
            SpecRuleLevel.Error => violation with { Severity = SpecSeverity.Error },
            _ => violation,
        } : violation;
}
