namespace Specht.Tests.Manifest;

/// <summary>
/// The top-level keys schema 0.3.0 introduces, <c>keys</c> and <c>forms</c>, under each pin below it
/// (<c>0001-F5</c> B-049; decision 0016).
/// </summary>
public sealed class NewKeysUnderOlderPins : TheoryData<string, string>
{
    public NewKeysUnderOlderPins()
    {
        Add("0.1.0", "keys");
        Add("0.1.0", "forms");
        Add("0.2.0", "keys");
        Add("0.2.0", "forms");
        Add("0.2.1", "keys");
        Add("0.2.1", "forms");
    }
}
