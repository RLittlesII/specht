using System.Text.Json;

namespace specht.tests;

/// <summary>
/// The golden report (<c>0001-F1</c> B-004, C-10): the engine's verdicts on <see cref="BaselineTree"/> at commit
/// <c>e7dba24</c>, committed as <c>Baseline/engine-e7dba24.json</c> and copied beside the test assembly. It is test data,
/// never regenerated from a later engine.
/// </summary>
public static class GoldenReport
{
    /// <summary>The golden report's path, relative to the test assembly's directory.</summary>
    public const string RelativePath = "Baseline/engine-e7dba24.json";

    /// <summary>Reads the committed verdicts, in their committed order.</summary>
    /// <returns>Each violation's six fields.</returns>
    public static IReadOnlyList<Verdict> Read() =>
        JsonSerializer.Deserialize<List<Verdict>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, RelativePath)), Options)
            ?? throw new InvalidDataException($"{RelativePath} holds no verdicts.");

    /// <summary>The verdicts <paramref name="report"/> gives, in its order, in the golden report's terms.</summary>
    /// <param name="report">A run of the engine.</param>
    /// <returns>Each violation's six fields.</returns>
    public static IReadOnlyList<Verdict> Of(SpecCheckReport report) =>
        [.. report.Violations.Select(static violation => new Verdict(
            violation.RuleId,
            violation.Severity.ToString().ToLowerInvariant(),
            violation.File,
            violation.Line,
            violation.Identifier,
            violation.Message))];

    /// <summary>One violation's six baseline fields (C-9).</summary>
    /// <param name="RuleId">The <c>SPEC###</c> rule that fired.</param>
    /// <param name="Severity"><c>error</c> or <c>warning</c>.</param>
    /// <param name="File">The root-relative path.</param>
    /// <param name="Line">The one-based line, or 0 for the whole file.</param>
    /// <param name="Identifier">The identifier at fault, where the rule has one.</param>
    /// <param name="Message">What is wrong.</param>
    public sealed record Verdict(string RuleId, string Severity, string File, int Line, string? Identifier, string Message);

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
