namespace Specht.Tests.Evidence;

/// <summary>The ways of naming the same two exports, each yielding the same supplied set.</summary>
public sealed class EvidencePathForms : TheoryData<string[]>
{
    public EvidencePathForms()
    {
        Add(["a.json", "b.json"]);
        Add(["--evidence", "a.json", "--evidence", "b.json"]);
    }
}
