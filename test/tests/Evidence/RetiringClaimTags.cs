using Xunit;

namespace Specht.Tests.Evidence;

public sealed class RetiringClaimTags : TheoryData<string>
{
    public RetiringClaimTags()
    {
        Add("@withdrawn");
        Add("@superseded");
    }
}
