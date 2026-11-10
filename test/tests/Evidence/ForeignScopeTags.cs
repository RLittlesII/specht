using Xunit;

namespace Specht.Tests.Evidence;

public sealed class ForeignScopeTags : TheoryData<string>
{
    public ForeignScopeTags()
    {
        Add("@feature:cap:list");
        Add("@epic:ep2");
    }
}
