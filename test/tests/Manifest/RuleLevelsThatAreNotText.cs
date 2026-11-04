namespace Specht.Tests.Manifest;

/// <summary>
/// A rule level that is not text, as the manifest's JSON writes it, which the rejection names as written
/// (<c>0001-F5</c> B-045; OQ-15 (b); decision 0021 call 6).
/// </summary>
public sealed class RuleLevelsThatAreNotText : TheoryData<string>
{
    public RuleLevelsThatAreNotText()
    {
        Add("1");
        Add("true");
        Add("null");
        Add("""["warning"]""");
        Add("""{"level":"warning"}""");
    }
}
