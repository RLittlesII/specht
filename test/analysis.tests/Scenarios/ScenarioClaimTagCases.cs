namespace Specht.Analysis.Tests.Scenarios;

public sealed class ScenarioClaimTagCases : TheoryData<string, string>
{
    public ScenarioClaimTagCases()
    {
        Add("B-999", "unresolved");
        Add("claim-1", "invalid");
    }
}
