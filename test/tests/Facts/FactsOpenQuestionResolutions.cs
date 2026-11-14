namespace Specht.Tests.Facts;

/// <summary>
/// An open question's Resolution cell and whether OQ-2's default reads it as resolved (<c>0132-F2</c> B-008): open when
/// the cell is empty or its first word, up to the first character that is neither a letter nor a digit, is <c>Open</c>
/// in exact case, and resolved otherwise.
/// </summary>
public sealed class FactsOpenQuestionResolutions : TheoryData<string, bool>
{
    public FactsOpenQuestionResolutions()
    {
        Add("Open", false);
        Add("Open. Proceeds on the default.", false);
        Add("Resolved 2026-10-30 by the owner: the answer", true);
        Add("", false);
        Add("Open; waits on the owner.", false);
        Add("Open 2026-11-01, asked of the owner.", false);
        Add("RESOLVED, by the owner.", true);
        Add("Superseded by OQ-3.", true);
        Add("open", true);
        Add("Opened 2026-11-01 for the owner.", true);
    }
}
