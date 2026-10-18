namespace Specht.Analysis.Tests.Documentation;

public sealed class ScalarIdentifierCases : TheoryData<string, bool>
{
    public ScalarIdentifierCases()
    {
        Add("need/7,need/8", true);
        Add("need/7 need/8", false);
    }
}
