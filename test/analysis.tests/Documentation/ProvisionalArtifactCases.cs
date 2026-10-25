namespace Specht.Analysis.Tests.Documentation;

public sealed class ProvisionalArtifactCases : TheoryData<string, string, bool, string>
{
    public ProvisionalArtifactCases()
    {
        Add("\n", "Debug", false, string.Empty);
        Add(string.Empty, "Release", false, string.Empty);
        Add(string.Empty, "Debug", true, string.Empty);
        Add(string.Empty, "Debug", false, "incomplete-discovery");
        Add(string.Empty, "Debug", false, "other-project");
        Add(string.Empty, "Debug", false, "other-framework");
    }
}
