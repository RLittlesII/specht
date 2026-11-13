namespace Specht.Tests.Facts;

/// <summary>
/// A violation, a count or a verdict added to a facts document, on the document or on a specification, each a member the
/// published facts schema refuses (<c>0132-F2</c> B-014, C-5): where it is added, its name and its JSON value.
/// </summary>
public sealed class FactsVerdictMembers : TheoryData<bool, string, string>
{
    public FactsVerdictMembers()
    {
        Add(false, "violations", "[]");
        Add(false, "errorCount", "0");
        Add(false, "verdict", "\"failed\"");
        Add(true, "violations", "[]");
        Add(true, "severity", "\"error\"");
    }
}
