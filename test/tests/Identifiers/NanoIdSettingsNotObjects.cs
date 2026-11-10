namespace Specht.Tests.Identifiers;

/// <summary>
/// A <c>nanoid</c> member that is not an object (<c>0001-F19</c> B-049, C-12), as JSON, one row per row of the scenario: a
/// number, a list and text.
/// </summary>
public sealed class NanoIdSettingsNotObjects : TheoryData<string>
{
    /// <summary>Initializes the number, list and text rows.</summary>
    public NanoIdSettingsNotObjects()
    {
        Add("8");
        Add("""["ab", 2]""");
        Add("\"ab\"");
    }
}
