using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Specht.TestEvidence.VsTest.Tests;

public sealed class RunnerOutcomeRows : TheoryData<TestOutcome, string>
{
    public RunnerOutcomeRows()
    {
        Add(TestOutcome.Passed, "passed");
        Add(TestOutcome.Failed, "failed");
        Add(TestOutcome.Skipped, "skipped");
        Add(TestOutcome.None, "notRun");
        Add(TestOutcome.NotFound, "notRun");
    }
}
