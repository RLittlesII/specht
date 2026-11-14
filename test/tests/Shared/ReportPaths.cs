namespace Specht.Tests.Shared;

/// <summary>
/// A report path in a directory that exists, each shape the command and the report store are handed (<c>0001-F3</c> B-022):
/// a file under a directory, and a bare file name.
/// </summary>
public sealed class ReportPaths : TheoryData<string, string>
{
    public ReportPaths()
    {
        Add("a file under a directory", "reports/specht-report.json");
        Add("a bare file name, whose directory is the working one", "specht-report.json");
    }
}
