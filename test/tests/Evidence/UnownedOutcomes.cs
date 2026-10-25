using Xunit;

namespace Specht.Tests.Evidence;

public sealed class UnownedOutcomes : TheoryData<string>
{
    public UnownedOutcomes()
    {
        Add("passed");
        Add("failed");
        Add("skipped");
    }
}
