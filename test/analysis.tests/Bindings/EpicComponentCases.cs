using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class EpicComponentCases : TheoryData<bool, string, string?, BindingResolution>
{
    public EpicComponentCases()
    {
        foreach (var kind in new[] { "claim", "adr" })
        {
            Add(true, kind, "0400", BindingResolution.Resolved);
            Add(true, kind, null, BindingResolution.Incomplete);
            Add(false, kind, null, BindingResolution.Resolved);
            Add(false, kind, "0400", BindingResolution.Invalid);
        }
    }
}
