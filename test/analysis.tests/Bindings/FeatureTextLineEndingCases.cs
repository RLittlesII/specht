namespace Specht.Analysis.Tests.Bindings;

public sealed class FeatureTextLineEndingCases : TheoryData<string>
{
    public FeatureTextLineEndingCases()
    {
        Add("\n");
        Add("\r\n");
    }
}
