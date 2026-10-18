namespace Specht.Analysis.Tests.Bindings;

public sealed class ContainedProvenancePathCases : TheoryData<string>
{
    public ContainedProvenancePathCases()
    {
        Add("src/one/.spec/README.md");
        Add(string.Join('\\', "src", "one", ".spec", "README.md"));
        Add("src/old/../one/.spec/README.md");
    }
}
