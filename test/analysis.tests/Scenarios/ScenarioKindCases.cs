namespace Specht.Analysis.Tests.Scenarios;

public sealed class ScenarioKindCases : TheoryData<bool>
{
    public ScenarioKindCases()
    {
        Add(false);
        Add(true);
    }
}
