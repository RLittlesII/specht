namespace Specht.Tests.Identifiers;

/// <summary>
/// The key a manifest names for a Feature's claim prefix, and the prefix a Feature declaring <c>ACTIVITY</c> under
/// <c>claim_prefix</c> has under it (<c>0001-F19</c> A-9; item <c>0437</c>): <c>ACTIVITY</c> under the shipped key, and nothing
/// for no key.
/// </summary>
public sealed class ClaimPrefixKeys : TheoryData<string?, string?>
{
    /// <summary>Initializes the shipped-key row and the no-key row.</summary>
    public ClaimPrefixKeys()
    {
        Add("claim_prefix", "ACTIVITY");
        Add(null, null);
    }
}
