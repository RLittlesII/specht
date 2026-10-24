namespace Specht.Analysis.Tests.TestEvidence;

public sealed class InvalidScenarioStateCases : TheoryData<bool, string?>
{
    public InvalidScenarioStateCases()
    {
        Add(true, null);
        Add(false, null);
        Add(false, "ignored");
        Add(false, "Planned");
        Add(false, "");
    }
}
