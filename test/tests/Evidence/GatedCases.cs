using Xunit;

namespace Specht.Tests.Evidence;

public sealed class GatedCases : TheoryData<bool?, bool, bool>
{
    public GatedCases()
    {
        Add(null, false, true);
        Add(true, false, true);
        Add(false, false, false);
        Add(true, true, false);
        Add(false, true, false);
        Add(null, true, false);
    }
}
