using Xunit;

namespace Specht.Tests.Evidence;

public sealed class ClaimFileSetGaps : TheoryData<string?>
{
    public ClaimFileSetGaps()
    {
        Add((string?) null);
        Add("src/list/.spec/*.feature");
        Add("obj/*.feature");
    }
}
