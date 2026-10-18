using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class AdrLifecycleCases : TheoryData<string, bool>
{
    public AdrLifecycleCases()
    {
        Add("accepted", true);
        foreach (var state in new[] { "proposed", "rejected", "deprecated", "superseded" })
        {
            Add(state, false);
        }
    }
}
