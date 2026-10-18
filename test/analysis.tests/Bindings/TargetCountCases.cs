using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class TargetCountCases : TheoryData<int, BindingResolution>
{
    public TargetCountCases()
    {
        Add(0, BindingResolution.Unresolved);
        Add(1, BindingResolution.Resolved);
        Add(2, BindingResolution.Ambiguous);
    }
}
