namespace Specht.Tests.Facts;

/// <summary>
/// The two faults that leave a specification's out-of-scope section with no facts (<c>0132-F2</c> B-015): <c>false</c>
/// for a specification with no out-of-scope section, and <c>true</c> for one whose out-of-scope section has no table
/// the parse reads, its header row given with no delimiter row.
/// </summary>
public sealed class FactsOutOfScopeFaults : TheoryData<bool>
{
    public FactsOutOfScopeFaults()
    {
        Add(false);
        Add(true);
    }
}
