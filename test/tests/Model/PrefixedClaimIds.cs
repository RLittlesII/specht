namespace Specht.Tests.Model;

/// <summary>
/// The manifest's claim grammar, or null for the pin's default, whether claim tags are read ignoring case, a Feature's
/// declared claim prefix, the claim ids its claims section declares in order, and those the claim-id rule reports
/// (<c>0001-F5</c> B-076, B-079; decision 0022). The prefix is literal and case-sensitive, so a regular-expression
/// character in it matches only itself and a tag case setting does not reach it; the suffix is three ASCII digits and an
/// optional lowercase ASCII letter whatever <c>identifiers.claim</c> holds.
/// </summary>
public sealed class PrefixedClaimIds : TheoryData<string?, bool, string, string[], string[]>
{
    public PrefixedClaimIds()
    {
        Add(
            null,
            false,
            "CI-AC-",
            ["CI-AC-001", "CI-AC-001a", "CI-AC-01", "CI-AC-0001", "CI-AC-001A", "CI-AC-001aa", "B-002"],
            ["CI-AC-01", "CI-AC-0001", "CI-AC-001A", "CI-AC-001aa", "B-002"]);
        Add(null, true, "CI.AC-", ["CI.AC-001", "CIXAC-002", "ci.ac-003"], ["CIXAC-002", "ci.ac-003"]);
        Add(null, false, "(R)+.", ["(R)+.001", "RRx001", "(R)+.001b"], ["RRx001"]);
        Add(null, false, "CI-AC-", ["CI-AC-001", "CI-AC-\u0661\u0662\u0663"], ["CI-AC-\u0661\u0662\u0663"]);
        Add("^(CI-AC-|REQ-)[0-9]{4}$", false, "CI-AC-", ["CI-AC-001a", "CI-AC-0001", "REQ-0001"], ["CI-AC-0001", "REQ-0001"]);
    }
}
