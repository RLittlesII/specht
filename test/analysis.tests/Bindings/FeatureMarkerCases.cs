using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class FeatureMarkerCases : TheoryData<string, bool>
{
    public FeatureMarkerCases()
    {
        Add("ratified", true);
        Add("approved", false);
    }
}
