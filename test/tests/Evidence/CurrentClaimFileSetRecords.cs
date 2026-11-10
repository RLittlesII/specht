using Xunit;

namespace Specht.Tests.Evidence;

public sealed class CurrentClaimFileSetRecords : TheoryData<string?>
{
    public CurrentClaimFileSetRecords()
    {
        Add((string?) null);
        Add("draft");
        Add("in-review");
    }
}
