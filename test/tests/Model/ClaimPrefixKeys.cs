namespace Specht.Tests.Model;

/// <summary>
/// The name a schema 0.3.1 manifest gives <c>keys.feature.claimPrefix</c>, or null to leave the pin's default, the
/// frontmatter key a Feature writes its prefix <c>CI-AC-</c> under, and whether the prefix is read, so its claim
/// <c>B-001</c> is reported for lacking it (<c>0001-F5</c> B-004, B-076; decision 0022).
/// </summary>
public sealed class ClaimPrefixKeys : TheoryData<string?, string, bool>
{
    public ClaimPrefixKeys()
    {
        Add(null, "claim_prefix", true);
        Add("criterion_prefix", "criterion_prefix", true);
        Add("criterion_prefix", "claim_prefix", false);
    }
}
