namespace Specht.Tests.Trace;

/// <summary>
/// Claim <c>B-002</c>'s traceability row, or none, beside two scenarios carrying its tag and a test file holding
/// <c>TracesAnother</c> and <c>TracesAgain</c>, with the line the trace prints for it (<c>0001-F13</c> B-001): the path,
/// the id, the row's Status or <c>none</c>, the scenario count, and the resolved tests out of those named, a skipped test
/// counting in neither, five fields separated by tabs.
/// </summary>
public sealed class TraceLineCases : TheoryData<string?, string>
{
    public TraceLineCases()
    {
        const string path = "src/trace/.spec/README.md";
        Add("| B-002 | Tracing another | `TracesAnother`, `TracesAgain`, `TracesNowhere` | Covered |", $"{path}\tB-002\tCovered\t2\t2/3");
        Add("| B-002 | Tracing another | `TracesAnother`, `TracesNowhere` | Partial |", $"{path}\tB-002\tPartial\t2\t1/2");
        Add("| B-002 | Tracing another | `TracesAnother`, `TracesNowhere` | Waived |", $"{path}\tB-002\tWaived\t2\t0/0");
        Add(null, $"{path}\tB-002\tnone\t2\t0/0");
    }
}
