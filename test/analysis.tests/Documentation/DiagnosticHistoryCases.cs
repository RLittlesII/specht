using Specht.Analysis.Documentation;

namespace Specht.Analysis.Tests.Documentation;

public sealed class DiagnosticHistoryCases : TheoryData<DiagnosticPublication, bool>
{
    public DiagnosticHistoryCases()
    {
        Add(new DiagnosticPublication("SPCT0004", "Absent"), false);
        Add(new DiagnosticPublication("SPCT0004", "Grammar"), true);
        Add(new DiagnosticPublication("SPCT0004", "Absent", true), true);
        Add(new DiagnosticPublication("SPCT0099", "Removed"), true);
    }
}
