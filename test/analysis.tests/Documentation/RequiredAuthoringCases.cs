namespace Specht.Analysis.Tests.Documentation;

public sealed class RequiredAuthoringCases : TheoryData<string, bool, bool, string?>
{
    public RequiredAuthoringCases()
    {
        Add("test/Example.cs", true, false, "SPCT0007");
        Add("src/Example.cs", true, false, null);
        Add("test/Example.g.cs", true, false, null);
        Add("test/Example.cs", false, false, null);
        Add("test/Example.g.cs", true, true, "SPCT0003");
        Add("test/Example.cs", true, true, "SPCT0003,SPCT0007");
    }
}
