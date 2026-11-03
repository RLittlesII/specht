using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Specht.Comparison;

/// <summary>Projects one comparison document into the human-readable and machine-readable reports.</summary>
public static class ComparisonReport
{
    public static string Markdown(ReleasePair pair, ComparisonDocument document)
    {
        var session = document.Session;
        var text = new StringBuilder();
        text.Append("# Release comparison\n\n")
            .Append("Overall judgment: ").Append(Display(document.Judgment)).Append("\n\n")
            .Append("## Release pair\n\n");
        Table(text, ["Release", "Id", "Tag", "Commit"],
            new[] { ("Current", pair.Current), ("Previous", pair.Previous) }.Where(static r => r.Item2 is not null)
                .Select(static r => new[] { r.Item1, Number(r.Item2!.Id), r.Item2.Tag, r.Item2.Commit ?? $"Unresolved: {r.Item2.ResolutionFailure}" }));
        text.Append("Pair digest: ").Append(Escape(ComparisonJson.Digest(pair))).Append("\n\n")
            .Append("## Session\n\n");
        Table(text, ["Comparator commit", "Comparator version", "Run id", "Attempt", "Session id", "Runner"],
            [[session.ComparatorCommit, session.ComparatorVersion, session.RunId, Number(session.Attempt), session.SessionId, session.RunnerIdentity]]);
        text.Append("## Measurement conditions\n\n");
        Table(text, ["Side", "Product commit", "Operating system", "Runtime", "Architecture", "Toolchain", "Garbage collection", "Job",
                "Harness", "Exporter", "Invocation"],
            new[] { ("Previous", session.Previous), ("Current", session.Current) }.Select(s => new[]
            {
                s.Item1, s.Item1 == "Previous" && pair.Previous is null ? "None" : s.Item2.ProductCommit,
                Known(s.Item2.Conditions.OperatingSystem), Known(s.Item2.Conditions.Runtime), Known(s.Item2.Conditions.Architecture),
                Known(s.Item2.Conditions.Toolchain), Known(s.Item2.Conditions.GarbageCollection), Known(s.Item2.Conditions.Job),
                s.Item2.HarnessVersion, s.Item2.ExporterVersion, string.Join(' ', s.Item2.Invocation),
            }));
        text.Append("## Policy\n\n");
        Table(text, ["Policy version", "Timing screen", "Managed allocation screen"],
            [[Number(document.Policy.Version), Percent(document.Policy.TimingIncreasePercent), Percent(document.Policy.AllocationIncreasePercent)]]);
        text.Append("## Cases\n\n");
        Table(text, ["Case", "Status", "Reason"], document.Cases.Select(static c => new[] { c.Key, Display(c.Status), c.Reason ?? "" }));
        text.Append("## Workloads\n\n");
        Table(text, ["Case", "Side", "Operation", "Operation revision", "Fixture", "Fixture revision", "Parameters", "Boundary"],
            document.Cases.SelectMany(static c => new[] { ("Previous", c.Evidence.Previous), ("Current", c.Evidence.Current) }
                .Where(static s => s.Item2 is not null).Select(s => new[]
                {
                    c.Key, s.Item1, s.Item2!.Case.OperationId, s.Item2.Case.OperationRevision, s.Item2.Case.FixtureId, s.Item2.Case.FixtureRevision,
                    string.Join(", ", s.Item2.Case.Parameters.Select(static p => $"{p.Key} = {p.Value.Value} ({p.Value.Kind})")), s.Item2.Case.Boundary,
                })));
        text.Append("## Metrics\n\n");
        Table(text, ["Case", "Metric", "Judgment", "Unit", "Previous", "Current", "Absolute change", "Percent change", "Reason"],
            document.Cases.SelectMany(static c => c.Metrics.Select(m => new[]
            {
                c.Key, Display(m.Metric), Display(m.Judgment), m.Unit ?? "", Value(m.Previous), Value(m.Current), Change(m.AbsoluteChange, ""),
                Change(m.PercentChange, "%"), m.Reason ?? "",
            })));
        text.Append("## Coverage gaps\n\n");
        Table(text, ["Case", "Metric", "Reason"],
            document.Gaps.Select(static g => new[] { g.CaseKey, g.Metric is { } metric ? Display(metric) : "Whole case", g.Reason }));
        text.Append("## Raw evidence\n\n");
        Table(text, ["Case", "Side", "Metric", "Raw export", "Locator"],
            document.Cases.SelectMany(static c => new[] { ("previous", c.Evidence.Previous), ("current", c.Evidence.Current) }
                .SelectMany(s => s.Item2?.Observations.Select(o => new[] { c.Key, s.Item1, Display(o.Metric), BundlePath(s.Item1, o.SourceFile), o.Locator })
                    ?? [])));
        return text.ToString();
    }

    public static string Json(ReleasePair pair, ComparisonDocument document) =>
        ComparisonJson.Write(new { formatVersion = 1, pair, comparison = Bundled(document) });

    internal static string BundlePath(string side, string sourceFile) => $"raw/{side}/{sourceFile[(sourceFile.LastIndexOf('/') + 1)..]}";

    internal static IEnumerable<(string Key, string Side, Observation Observation)> Citations(ComparisonDocument document) =>
        document.Cases.SelectMany(static c => c.Evidence.Metrics.Concat(c.Metrics.Select(static m => m.Evidence))
            .Select(static p => (p.Previous, p.Current))
            .Append((Previous: c.Evidence.Previous?.Observations ?? [], Current: c.Evidence.Current?.Observations ?? []))
            .SelectMany(static sides => sides.Previous.Select(static o => (Side: "previous", Observation: o))
                .Concat(sides.Current.Select(static o => (Side: "current", Observation: o))))
            .Select(cited => (c.Key, cited.Side, cited.Observation)));

    internal static string Escape(string value)
    {
        var escaped = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            escaped.Append(c switch
            {
                '\\' => @"\\",
                '|' => @"\|",
                '`' => @"\`",
                '<' => "&lt;",
                '>' => "&gt;",
                _ when char.IsControl(c) => " ",
                _ => c.ToString(),
            });
        }

        return escaped.ToString();
    }

    internal static string Display(ComparisonJudgment judgment) => judgment switch
    {
        ComparisonJudgment.ReviewRequired => "Review required",
        ComparisonJudgment.Incomplete => "Incomplete",
        ComparisonJudgment.NoBaseline => "No baseline",
        _ => "No regression flagged",
    };

    internal static string Display(MetricKind metric) => metric switch
    {
        MetricKind.ManagedAllocation => "Managed allocation",
        MetricKind.ToolPeakMemory => "Tool peak memory",
        _ => "Time",
    };

    private static ComparisonDocument Bundled(ComparisonDocument document) => document with
    {
        Cases = [.. document.Cases.Select(static c => c with
        {
            Metrics = [.. c.Metrics.Select(static m => m with { Evidence = Bundled(m.Evidence) })],
            Evidence = c.Evidence with
            {
                Previous = Bundled(c.Evidence.Previous, "previous"),
                Current = Bundled(c.Evidence.Current, "current"),
                Metrics = [.. c.Evidence.Metrics.Select(Bundled)],
            },
        })],
    };

    private static MetricPair Bundled(MetricPair pair) =>
        pair with { Previous = Bundled(pair.Previous, "previous"), Current = Bundled(pair.Current, "current") };

    private static CaseEvidence? Bundled(CaseEvidence? evidence, string side) =>
        evidence is null ? null : evidence with { Observations = Bundled(evidence.Observations, side) };

    private static ImmutableArray<Observation> Bundled(ImmutableArray<Observation> observations, string side) =>
        [.. observations.Select(o => o with { SourceFile = BundlePath(side, o.SourceFile) })];

    private static void Table(StringBuilder text, string[] headers, IEnumerable<string[]> rows)
    {
        text.Append("| ").AppendJoin(" | ", headers).Append(" |\n|");
        foreach (var _ in headers)
        {
            text.Append(" --- |");
        }

        text.Append('\n');
        foreach (var row in rows)
        {
            text.Append("| ").AppendJoin(" | ", row.Select(Escape)).Append(" |\n");
        }

        text.Append('\n');
    }

    private static string Known(string? value) => value ?? "Unknown";

    private static string Number(long value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Rounded(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero).ToString("0.##", CultureInfo.InvariantCulture);

    private static string Value(decimal? value) => value is { } known ? Rounded(known) : "Not defined";

    private static string Percent(decimal value) => Rounded(value) + "%";

    private static string Change(decimal? value, string suffix) => value is not { } known ? "Not defined"
        : (Math.Round(known, 2, MidpointRounding.AwayFromZero) > 0 ? "+" : "") + Rounded(known) + suffix;

    private static string Display(MetricJudgment judgment) => judgment switch
    {
        MetricJudgment.ReviewRequired => "Review required",
        MetricJudgment.InsufficientEvidence => "Insufficient evidence",
        MetricJudgment.NotComparable => "Not comparable",
        MetricJudgment.Informational => "Informational",
        _ => "No regression flagged",
    };

    private static string Display(PairingStatus status) => status switch
    {
        PairingStatus.NotComparable => "Not comparable",
        PairingStatus.Added => "Added",
        PairingStatus.Removed => "Removed",
        PairingStatus.InsufficientEvidence => "Insufficient evidence",
        _ => "Comparable",
    };
}
