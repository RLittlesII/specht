namespace Specht.Tests.Model;

/// <summary>
/// A <c>claim_prefix</c> value as written in a Feature's YAML frontmatter, and whether the schema 0.3.1 Feature schema
/// rejects it: anything other than a string holding at least one non-white-space character (<c>0001-F5</c> B-078).
/// </summary>
public sealed class ClaimPrefixDeclarations : TheoryData<string, bool>
{
    public ClaimPrefixDeclarations()
    {
        Add("\"\"", true);
        Add("\"   \"", true);
        Add("\"\\t\"", true);
        Add("[\"CI-AC-\"]", true);
        Add("7", true);
        Add("true", true);
        Add("null", true);
        Add("\"CI-AC-\"", false);
        Add("CI-AC-", false);
        Add("\" CI AC \"", false);
    }
}
