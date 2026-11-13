namespace Specht.Tests.Manifest;

/// <summary>
/// A <c>claim.source</c> value, as JSON, that is not the text <c>markdown</c> or <c>gherkin</c> (<c>0001-F16</c> B-031,
/// decision 0003).
/// </summary>
public sealed class UndefinedClaimSources : TheoryData<string>
{
    /// <summary>Initializes one row for each value the B-031 outline names.</summary>
    public UndefinedClaimSources()
    {
        Add("\"yaml\"");
        Add("\"Gherkin\"");
        Add("1");
    }
}
