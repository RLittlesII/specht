namespace Specht.Tests.Model;

/// <summary>
/// The members of a schema 0.4.0 manifest and a Feature's claim prefix that render its claim head as the default scenario
/// head <c>S-</c> (<c>0001-F5</c> B-100; A-8): a <c>form</c> with no separator and the prefix <c>S-</c>, a <c>form</c>
/// with one and the prefix <c>S</c>, and a prefix differing in case while claim tags are compared ignoring it.
/// </summary>
public sealed class SharedHeadPrefixes : TheoryData<string, string>
{
    /// <summary>Initializes the rows.</summary>
    public SharedHeadPrefixes()
    {
        Add("""{ "ids": { "claim": { "form": "{prefix}{id}" } } }""", "S-");
        Add("""{ "ids": { "claim": { "form": "{prefix}-{id}" } } }""", "S");
        Add("""{ "claim": { "tagIgnoreCase": true }, "ids": { "claim": { "form": "{prefix}{id}" } } }""", "s-");
    }
}
