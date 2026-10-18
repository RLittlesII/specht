namespace Specht.Analysis.Tests.Bindings;

public sealed class ProvenancePathCases : TheoryData<string, bool>
{
    public ProvenancePathCases()
    {
        Add("POSIX root", false);
        Add("Windows drive", false);
        Add("Windows drive relative", false);
        Add("UNC root", false);
        Add("backslash root", false);
        Add("forward escape", false);
        Add("backslash escape", false);
        Add("mixed escape", false);
        Add("nested relative", true);
        Add("backslash relative", true);
        Add("contained parent", true);
    }
}
