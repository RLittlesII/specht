using Xunit;

namespace Specht.Tests.Evidence;

public sealed class NotPassingOutcomes : TheoryData<string>
{
    public NotPassingOutcomes()
    {
        Add("failed");
        Add("skipped");
        Add("absent");
    }
}
