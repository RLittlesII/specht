namespace Specht.Tests.Upgrade;

/// <summary>
/// The <c>schemaVersion</c> values the check rejects that <c>0001-F7</c> B-021 holds <c>specht upgrade</c> to, as the
/// manifest's JSON writes them: a <c>major.minor.patch</c> version the tool does not ship (B-003), and the check's values that
/// are not a <c>major.minor.patch</c> string (B-039) - the integer <c>1</c>, a string of one, two or four numbers, a prefix, a
/// part that is not a number, a boolean, a null and an array.
/// </summary>
public sealed class UpgradeRejectedPins : TheoryData<string>
{
    public UpgradeRejectedPins()
    {
        Add("\"7.0.0\"");
        Add("1");
        Add("\"1\"");
        Add("\"0.1\"");
        Add("\"0.1.0.0\"");
        Add("\"v0.1.0\"");
        Add("\"0.1.x\"");
        Add("true");
        Add("null");
        Add("[1]");
    }
}
