using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class UnknownLifecycleCases : TheoryData<string>
{
    public UnknownLifecycleCases()
    {
        Add("claim");
        Add("Feature");
        Add("ADR");
    }
}
