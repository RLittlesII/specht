using Specht.Manifest;
using Specht.Model;
using Specht.Rules;

namespace Specht.Evaluation;

/// <summary>
/// Evaluates a resolved tree with the rules it was given (ADR-0010 § "The evaluator is pure"): select, run, stamp, sort.
/// </summary>
/// <param name="rules">The rule set, in the order it is run.</param>
/// <param name="order">The order the report carries its violations in.</param>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class SpechtEvaluator(IEnumerable<ISpechtRule> rules, SpechtViolationOrder order)
{
    /// <summary>
    /// Selects the rules whose id, in the form the version <paramref name="model"/>'s schemas keep names it by, is in that
    /// version's vocabulary and whose level - the manifest's for that id, else the rule's own - is not off (<c>0001-F1</c>
    /// B-004; <c>0001-F7</c> B-014, B-151, B-152; <c>0001-F5</c> B-010, B-061, B-062), runs each once in the order given,
    /// stamps each finding with that id and that level as its severity, a rule failure at error whatever the level
    /// (<c>0001-F5</c> B-011; <c>0001-F1</c> B-015), and orders the violations.
    /// </summary>
    /// <param name="model">The resolved tree.</param>
    /// <returns>The report, counting the rules selected.</returns>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public SpechtReport Evaluate(SpechtModel model) => Evaluate(model, rule => rule.Evaluate(model));

    /// <summary>
    /// Selects, stamps, counts and orders as <see cref="Evaluate(SpechtModel)"/> does, taking each selected rule's findings
    /// from <paramref name="findings"/> instead of its whole-tree evaluation: one document's check (<c>0001-F1</c> B-001).
    /// </summary>
    /// <param name="model">The resolved tree whose schemas select the rules.</param>
    /// <param name="findings">The findings of one selected rule, asked for once per rule selected.</param>
    /// <returns>The report, counting the rules selected.</returns>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F7" claim="B-151" />
    /// <implements epic="0001" feature="F7" claim="B-152" />
    internal SpechtReport Evaluate(SpechtModel model, Func<ISpechtRule, IEnumerable<SpechtFinding>> findings)
    {
        var version = model.Schemas.Version;
        var settings = model.Schemas.Structure.Rules;
        var selected = rules
            .Select(rule => (Rule: rule, Id: version.RuleId(rule.Id)))
            .Select(selection => (selection.Rule, selection.Id,
                Level: settings.TryGetValue(selection.Id, out var level) ? level : selection.Rule.DefaultLevel))
            .Where(selection => version.RuleIds.Contains(selection.Id) && selection.Level != SpechtRuleLevel.Off)
            .ToList();
        var violations = new List<SpechtViolation>();

        foreach (var (rule, id, level) in selected)
        {
            violations.AddRange(findings(rule).Select(finding => Stamped(finding, id, level)));
        }

        return new SpechtReport(
            version.Number,
            model.Features.Count,
            [
                .. model.Schemas.Structure.Discovery.Layouts.Select(layout =>
                    new SpechtReportLayout(layout.Name, model.Features.Count(feature => feature.Location.Layout == layout))),
            ],
            model.Items.Count,
            selected.Count,
            order.Order(violations))
        { SchemaSource = model.Schemas.Source };
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static SpechtViolation Stamped(SpechtFinding finding, string id, SpechtRuleLevel level) =>
        new(
            id,
            finding.RuleFailure is null && level == SpechtRuleLevel.Warning ? SpechtSeverity.Warning : SpechtSeverity.Error,
            finding.File,
            finding.Line,
            finding.Identifier,
            finding.Message)
        {
            Expected = finding.Expected,
            RuleFailure = finding.RuleFailure,
        };
}
