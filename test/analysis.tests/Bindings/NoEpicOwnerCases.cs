using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class NoEpicOwnerCases : TheoryData<bool>
{
    public NoEpicOwnerCases()
    {
        Add(false);
        Add(true);
    }
}
