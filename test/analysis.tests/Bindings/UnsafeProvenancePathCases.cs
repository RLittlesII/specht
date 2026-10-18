namespace Specht.Analysis.Tests.Bindings;

public sealed class UnsafeProvenancePathCases : TheoryData<string>
{
    public UnsafeProvenancePathCases()
    {
        Add('/' + "outside.md");
        Add("Q" + ':' + '\\' + "outside.md");
        Add("Q" + ':' + "outside.md");
        Add(new string('\\', 2) + "server" + '\\' + "outside.md");
        Add('\\' + "outside.md");
        Add(string.Join('/', "safe", "..", "..", "outside.md"));
        Add(string.Join('\\', "safe", "..", "..", "outside.md"));
        Add("safe" + '\\' + ".." + '/' + ".." + '\\' + "outside.md");
    }
}
