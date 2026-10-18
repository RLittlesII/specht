namespace Specht.Analysis.Tests.Documentation;

public sealed class BlockedDefaultCases : TheoryData<string>
{
    public BlockedDefaultCases()
    {
        Add("duplicate feature");
        Add("duplicate claim");
        Add("withdrawn claim");
        Add("superseded feature");
    }
}
