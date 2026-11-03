namespace Specht.Tests.Model;

/// <summary>
/// A schema 0.3.0 task grammar, two items of epic <c>0001</c> with a gap between them, and the missing sequence number
/// padded to the width the second item's captured sequence has (<c>0001-F5</c> B-053).
/// </summary>
public sealed class SequenceGaps : TheoryData<string, string, string, string>
{
    public SequenceGaps()
    {
        Add("^(?<epic>[0-9]{4})-(?<sequence>[0-9]{2})$", "0001-01", "0001-03", "02");
        Add("^(?<epic>[0-9]{4})-(?<sequence>[0-9]{3})$", "0001-001", "0001-004", "002");
    }
}
