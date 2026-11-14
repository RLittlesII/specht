namespace Specht.Tests.Facts;

/// <summary>
/// An ADR's first line under its <c>## Status</c> heading and the status OQ-3's default reads from it (<c>0132-F2</c>
/// B-011): the line's first word, up to its first character that is neither a letter nor a digit, in the case it is
/// written, and <see langword="null"/> with no line or a line that begins with no such word.
/// </summary>
public sealed class FactsAdrStatusLines : TheoryData<string?, string?>
{
    public FactsAdrStatusLines()
    {
        Add("accepted - 2026-10-08, by the owner", "accepted");
        Add("Accepted, 2026-10-12, by the owner", "Accepted");
        Add("Rejected — withdrawn by the owner", "Rejected");
        Add("Superseded by ADR-0002.", "Superseded");
        Add("Proposed", "Proposed");
        Add("— proposed, awaiting the owner", null);
        Add(null, null);
    }
}
