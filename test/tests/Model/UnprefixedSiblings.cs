namespace Specht.Tests.Model;

/// <summary>
/// The manifest's claim grammar, or null for the pin's default, the claim ids a Feature declaring no prefix declares
/// beside a sibling declaring <c>CI-AC-</c>, and those the claim-id rule reports in it: the manifest grammar alone
/// (<c>0001-F5</c> B-077; decision 0022).
/// </summary>
public sealed class UnprefixedSiblings : TheoryData<string?, string[], string[]>
{
    public UnprefixedSiblings()
    {
        Add(null, ["B-001", "bad-id", "CI-AC-001"], ["bad-id", "CI-AC-001"]);
        Add("^(CI-AC-|REQ-)[0-9]{4}$", ["CI-AC-001a", "CI-AC-0001", "REQ-0001"], ["CI-AC-001a"]);
    }
}
