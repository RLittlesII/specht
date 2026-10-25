using Xunit;

namespace Specht.Tests.Evidence;

public sealed class PlannedLinkSets : TheoryData<string[], bool>
{
    public PlannedLinkSets()
    {
        Add([], false);
        Add(["planned"], true);
        Add(["planned", "planned"], true);
        Add(["planned", "active"], false);
        Add(["active"], false);
    }
}
