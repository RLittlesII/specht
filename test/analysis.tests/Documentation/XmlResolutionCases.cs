namespace Specht.Analysis.Tests.Documentation;

public sealed class XmlResolutionCases : TheoryData<string, int, string>
{
    public XmlResolutionCases()
    {
        Add("Active", 0, "SPCT0004");
        Add("Active", 2, "SPCT0005");
        Add("Withdrawn", 1, "SPCT0006");
    }
}
