namespace Specht.Tests.Manifest;

/// <summary>
/// A <c>claim.source</c> value, as JSON, that is not the text <c>markdown</c> or <c>gherkin</c> (<c>0001-F16</c> B-031,
/// decision 0003): the values the B-031 outline names, and a declared JSON <c>null</c>, which is not text either.
/// </summary>
public sealed class UndefinedClaimSources : TheoryData<string>
{
    /// <summary>Initializes one row for each value the B-031 outline names, and one for <c>null</c>.</summary>
    public UndefinedClaimSources()
    {
        Add("\"yaml\"");
        Add("\"Gherkin\"");
        Add("1");
        Add("null");
    }
}
