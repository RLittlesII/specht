using System;
using System.Text;

namespace Specht.Comparison;

/// <summary>Renders a reporting outcome as the step summary of the workflow run.</summary>
public static class RunSummary
{
    public const string Pending = "## Release baseline comparison\n\nIncomplete: comparison pending\n";

    public static string Markdown(ReportingOutcome outcome)
    {
        var text = new StringBuilder("## Release baseline comparison\n\n");
        if (outcome.State == ReportingState.Incomplete) text.Append("Incomplete: ").Append(ComparisonReport.Escape(outcome.Reason ?? "")).Append("\n\n");
        if (outcome.Judgment is { } judgment) text.Append("Overall judgment: ").Append(ComparisonReport.Display(judgment)).Append("\n\n");
        if (outcome.State == ReportingState.Completed && outcome.Reason is { } reason) text.Append(ComparisonReport.Escape(reason)).Append("\n\n");
        if (outcome.Report is { } report)
        {
            text.Append("Report: [").Append(ComparisonReport.Escape(report.Bundle.Name)).Append("](https://github.com/")
                .Append(report.Release.Repository).Append("/releases/download/").Append(Uri.EscapeDataString(report.Release.Tag)).Append('/')
                .Append(Uri.EscapeDataString(report.Bundle.Name)).Append(")\n\n");
        }

        if (outcome.Flags.IsDefaultOrEmpty) return text.ToString();
        text.Append("| Case | Metric | Reason |\n| --- | --- | --- |\n");
        foreach (var flag in outcome.Flags)
        {
            text.Append("| ").Append(ComparisonReport.Escape(flag.CaseKey)).Append(" | ").Append(ComparisonReport.Display(flag.Metric)).Append(" | ")
                .Append(ComparisonReport.Escape(flag.Reason)).Append(" |\n");
        }

        return text.ToString();
    }
}
