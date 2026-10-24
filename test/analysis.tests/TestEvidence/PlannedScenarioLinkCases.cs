namespace Specht.Analysis.Tests.TestEvidence;

public sealed class PlannedScenarioLinkCases : TheoryData<string, string?, string?>
{
    public PlannedScenarioLinkCases()
    {
        Add("unresolved", "row-1", null);
        Add("resolved", null, "case-17");
    }
}
