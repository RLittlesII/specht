using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class OmittedLifecycleCases : TheoryData<string, bool, bool, bool, bool>
{
    public OmittedLifecycleCases()
    {
        foreach (var kind in new[] { "claim", "Feature", "ADR" })
        {
            foreach (var eligible in new[] { false, true })
            {
                Add(kind, kind == "Feature" ? eligible : !eligible, kind == "claim" ? eligible : !eligible,
                    kind == "ADR" ? eligible : !eligible, eligible);
            }
        }
    }
}
