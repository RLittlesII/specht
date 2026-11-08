namespace Specht.Tests.Model;

/// <summary>
/// The manifest's claim grammar, or null for the pin's default, its claim tag form and case setting, a tag in the
/// companion of a Feature declaring <c>CI-AC-</c> and the claim <c>CI-AC-001a</c> that resolves to it, a tag naming no
/// claim, and the identifier that tag is reported under (<c>0001-F5</c> B-007, B-036, B-037, B-080; C-12). The tag is
/// read with the Feature's claim grammar under the existing form and case settings, whatever <c>identifiers.claim</c>
/// holds.
/// </summary>
public sealed class PrefixedClaimTags : TheoryData<string?, string, bool, string, string, string>
{
    public PrefixedClaimTags()
    {
        Add(null, "@{claim}", false, "@CI-AC-001a", "@CI-AC-002", "CI-AC-002");
        Add(null, "@{claim}", true, "@ci-ac-001A", "@ci-ac-002", "ci-ac-002");
        Add(null, "@req:{claim}", false, "@req:CI-AC-001a", "@req:CI-AC-002", "CI-AC-002");
        Add("^(CI-AC-|REQ-)[0-9]{4}$", "@{claim}", false, "@CI-AC-001a", "@CI-AC-002", "CI-AC-002");
    }
}
