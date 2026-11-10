using Xunit;

namespace Specht.Tests.Evidence;

public sealed class AbsentClaimFileSets : TheoryData<string?>
{
    public AbsentClaimFileSets()
    {
        Add((string?) null);
        Add("{}");
        Add("[]");
    }
}
