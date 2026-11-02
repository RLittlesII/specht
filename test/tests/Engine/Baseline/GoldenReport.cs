using System.Text.Json;
using Specht.Evaluation;

namespace Specht.Tests.Engine.Baseline;

/// <summary>
/// The golden reports (<c>0001-F1</c> B-004, B-018, C-10): the engine's verdicts on <see cref="BaselineTree"/> at commit
/// <c>e7dba24</c>, pinned to schema 0.1.0, committed as <c>Baseline/engine-e7dba24.json</c>; and its verdicts on the same
/// tree pinned to schema 0.3.0, committed as <c>Baseline/engine-v0.3.0.json</c>. Both are copied beside the test assembly.
/// They are test data, never regenerated from a later engine.
/// </summary>
public static class GoldenReport
{
    /// <summary>The 0.1.0 golden report's path, relative to the test assembly's directory (B-004).</summary>
    public const string RelativePath = "Baseline/engine-e7dba24.json";

    /// <summary>The 0.3.0 golden report's path, relative to the test assembly's directory (B-018).</summary>
    public const string VersionZeroThreeZeroPath = "Baseline/engine-v0.3.0.json";

    /// <summary>The schema version the second golden report is the verdict under (B-018).</summary>
    public const string VersionZeroThreeZero = "0.3.0";

    /// <summary>Reads the 0.1.0 committed verdicts, in their committed order.</summary>
    /// <returns>Each violation's six fields.</returns>
    public static IReadOnlyList<Verdict> Read() => Read(RelativePath);

    /// <summary>Reads the verdicts committed at <paramref name="relativePath"/>, in their committed order.</summary>
    /// <param name="relativePath">The golden report's path, relative to the test assembly's directory.</param>
    /// <returns>Each violation's six fields.</returns>
    public static IReadOnlyList<Verdict> Read(string relativePath) =>
        JsonSerializer.Deserialize<List<Verdict>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, relativePath)), Options)
            ?? throw new InvalidDataException($"{relativePath} holds no verdicts.");

    /// <summary>The verdicts <paramref name="report"/> gives, in its order, in the golden report's terms.</summary>
    /// <param name="report">A run of the engine.</param>
    /// <returns>Each violation's six fields.</returns>
    public static IReadOnlyList<Verdict> Of(SpechtReport report) =>
        [.. report.Violations.Select(static violation => new Verdict(
            violation.RuleId,
            violation.Severity.ToString().ToLowerInvariant(),
            violation.File,
            violation.Line,
            violation.Identifier,
            violation.Message))];

    /// <summary>One violation's six baseline fields (C-9).</summary>
    /// <param name="RuleId">The rule id that fired, by the pinned version's vocabulary.</param>
    /// <param name="Severity"><c>error</c> or <c>warning</c>.</param>
    /// <param name="File">The root-relative path.</param>
    /// <param name="Line">The one-based line, or 0 for the whole file.</param>
    /// <param name="Identifier">The identifier at fault, where the rule has one.</param>
    /// <param name="Message">What is wrong.</param>
    public sealed record Verdict(string RuleId, string Severity, string File, int Line, string? Identifier, string Message);

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
