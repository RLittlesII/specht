using Specht.Comparison.Model;

namespace Specht.Comparison.Tests.Fixtures;

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
