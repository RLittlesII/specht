using System.Text.Json;
using System.Text.Json.Serialization;

namespace specht;

/// <summary>
/// Runs every <see cref="ISpecRule"/> in this assembly against the resolved
/// specification tree.
/// </summary>
public static class SpecCheckRunner
{
    /// <summary>Loads the tree under <paramref name="root"/> and evaluates every rule.</summary>
    public static SpecCheckReport Run(string root)
    {
        var model = SpecModel.Load(root);
        var rules = Discover();
        var violations = new List<SpecViolation>();

        foreach (var rule in rules)
        {
            violations.AddRange(rule.Evaluate(model));
        }

        var ordered = violations
            .OrderByDescending(static violation => violation.Severity)
            .ThenBy(static violation => violation.File, StringComparer.Ordinal)
            .ThenBy(static violation => violation.Line)
            .ThenBy(static violation => violation.RuleId, StringComparer.Ordinal)
            .ToList();

        return new SpecCheckReport(
            model.Features.Count,
            model.LegacyCount,
            model.CoLocatedCount,
            model.Items.Count,
            rules.Sum(static rule => rule.ReportedIds.Count),
            ordered);
    }

    /// <summary>
    /// Writes <paramref name="report"/> as JSON to <paramref name="path"/>.
    /// </summary>
    /// <remarks>
    /// Every path in the payload is repository-relative. A report carrying
    /// absolute paths is useless on another checkout and harmful if it is ever
    /// committed - which is the lesson <c>format.json</c> already taught here.
    /// </remarks>
    public static void WriteReport(SpecCheckReport report, string path)
    {
        var payload = new
        {
            generatedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
            specificationCount = report.SpecificationCount,
            legacyCount = report.LegacyCount,
            coLocatedCount = report.CoLocatedCount,
            itemCount = report.ItemCount,
            rulesEvaluated = report.RulesEvaluated,
            violations = report.Violations.Select(static violation => new
            {
                ruleId = violation.RuleId,
                severity = violation.Severity.ToString().ToLowerInvariant(),
                file = violation.File,
                line = violation.Line,
                identifier = violation.Identifier,
                message = violation.Message,
            }),
        };

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(payload, ReportOptions));
    }

    /// <summary>
    /// Finds every rule by reflection, so a new rule file needs no registration
    /// edit - the same reason the test projects glob their sources.
    /// </summary>
    private static IReadOnlyList<ISpecRule> Discover() =>
        typeof(SpecCheckRunner).Assembly
            .GetTypes()
            .Where(static type => typeof(ISpecRule).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false })
            .Select(static type => (ISpecRule)Activator.CreateInstance(type)!)
            .OrderBy(static rule => rule.Id, StringComparer.Ordinal)
            .ToList();

    private static readonly JsonSerializerOptions ReportOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
