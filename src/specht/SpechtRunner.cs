using specht.Report;

namespace specht;

/// <summary>
/// Runs every <see cref="ISpecRule"/> in this assembly against the resolved
/// specification tree.
/// </summary>
public static class SpechtRunner
{
    /// <summary>Loads the tree under <paramref name="root"/> and evaluates its rules with the embedded version set.</summary>
    public static SpechtReport Run(string root) => Run(root, SchemaVersions.Embedded);

    /// <summary>
    /// Loads the tree under <paramref name="root"/> and evaluates the rules of the version its manifest pins from
    /// <paramref name="versions"/>: a rule outside that version's vocabulary is not evaluated, and a violation it would
    /// report is dropped (<c>0001-F7</c> B-014).
    /// </summary>
    public static SpechtReport Run(string root, SchemaVersions versions) =>
        Evaluate(SpecModel.Load(root, versions), Discover());

    /// <summary>
    /// Evaluates <paramref name="rules"/>, in the order given, over <paramref name="model"/> under the vocabulary of the
    /// version its schemas keep (<c>0001-F1</c> B-004; <c>0001-F7</c> B-014; ADR-0008).
    /// </summary>
    /// <param name="model">The resolved tree.</param>
    /// <param name="rules">The rule set.</param>
    /// <returns>The report.</returns>
    public static SpechtReport Evaluate(SpecModel model, IEnumerable<ISpecRule> rules)
    {
        var version = model.Schemas.Version;
        var evaluated = rules.Where(rule => rule.ReportedIds.Any(version.RuleIds.Contains)).ToList();
        var violations = new List<SpecViolation>();

        foreach (var rule in evaluated)
        {
            violations.AddRange(rule.Evaluate(model).Where(violation => version.RuleIds.Contains(violation.RuleId)));
        }

        return new SpechtReport(
            version.Number,
            model.Features.Count,
            [
                .. model.Schemas.Structure.Discovery.Layouts.Select(layout =>
                    new SpecReportLayout(layout.Name, model.Features.Count(feature => feature.Location.Layout == layout))),
            ],
            model.Items.Count,
            evaluated.SelectMany(static rule => rule.ReportedIds).Count(version.RuleIds.Contains),
            Order(violations));
    }

    /// <summary>
    /// Orders <paramref name="violations"/> as the report carries them (<c>0001-F1</c> B-004, C-9): severity
    /// descending, then file, line and rule id, file and rule id compared ordinally, and violations equal on all
    /// four keys kept in the order they were given.
    /// </summary>
    /// <param name="violations">The violations to order.</param>
    /// <returns>The violations, ordered.</returns>
    public static IReadOnlyList<SpecViolation> Order(IEnumerable<SpecViolation> violations) =>
        violations
            .OrderByDescending(static violation => violation.Severity)
            .ThenBy(static violation => violation.File, StringComparer.Ordinal)
            .ThenBy(static violation => violation.Line)
            .ThenBy(static violation => violation.RuleId, StringComparer.Ordinal)
            .ToList();

    /// <summary>Writes <paramref name="report"/> to <paramref name="path"/> as the report document (<c>0001-F3</c> B-008).</summary>
    /// <param name="report">The run's report.</param>
    /// <param name="path">Where the document is written.</param>
    public static void WriteReport(SpechtReport report, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, SpecReportDocument.From(report).ToJson());
    }

    /// <summary>
    /// Finds every rule by reflection, so a new rule file needs no registration
    /// edit - the same reason the test projects glob their sources.
    /// </summary>
    private static IReadOnlyList<ISpecRule> Discover() =>
        typeof(SpechtRunner).Assembly
            .GetTypes()
            .Where(static type => typeof(ISpecRule).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false })
            .Select(static type => (ISpecRule)Activator.CreateInstance(type)!)
            .OrderBy(static rule => rule.Id, StringComparer.Ordinal)
            .ToList();
}
