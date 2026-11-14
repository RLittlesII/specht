using Specht.Model;

namespace Specht.Tests.Check;

/// <summary>
/// The severities a report carries, the arguments a run takes, and the document form added to them (<c>0001-F3</c> B-004):
/// <c>--json</c>, <c>--report</c> with a bare file name, and both.
/// </summary>
public sealed class ReportPathVerdicts : TheoryData<SpechtSeverity[], string[], string[]>
{
    public ReportPathVerdicts()
    {
        Add([SpechtSeverity.Error], [], ["--json"]);
        Add([SpechtSeverity.Error], [], ["--report", "specht-report.json"]);
        Add([SpechtSeverity.Error], [], ["--json", "--report", "specht-report.json"]);
        Add([], [], ["--report", "specht-report.json"]);
        Add([SpechtSeverity.Warning], [], ["--report", "specht-report.json"]);
        Add([SpechtSeverity.Warning], ["--strict"], ["--report", "specht-report.json"]);
    }
}
