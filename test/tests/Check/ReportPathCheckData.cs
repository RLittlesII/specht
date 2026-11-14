namespace Specht.Tests.Check;

/// <summary>A check run with a report path that returns a report, clean and with a violation, and its exit code (<c>0001-F3</c> C-7).</summary>
public sealed class ReportPathCheckData : TheoryData<string, int>
{
    public ReportPathCheckData()
    {
        Add("clean", 0);
        Add("violation", 1);
    }
}
