namespace Specht.Tests.Model;

/// <summary>
/// A <c>record</c> the check cannot read, under a root <c>repo</c> whose <c>.spec/features/arithmetic/README.md</c> cannot
/// be read, beside an <c>outside/README.md</c> outside it, each of the two an approved record depending on <c>F2</c>
/// (<c>0001-F16</c> B-035; § 7, item <c>0395</c>): a path naming no file, the file that cannot be read, a rooted path to
/// the file outside, and a relative path leading to it.
/// </summary>
public sealed class UnreadableRecords : TheoryData<string>
{
    /// <summary>Initializes one row per condition B-035 names.</summary>
    public UnreadableRecords()
    {
        Add(".spec/features/arithmetic/missing.md");
        Add(".spec/features/arithmetic/README.md");
        Add("/outside/README.md");
        Add("../outside/README.md");
    }
}
