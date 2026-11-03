namespace Specht.Comparison.Tests.Workflow;

public sealed class ReportingStages : TheoryData<ReportingStage>
{
    public ReportingStages()
    {
        foreach (var stage in Enum.GetValues<ReportingStage>())
        {
            Add(stage);
        }
    }
}
