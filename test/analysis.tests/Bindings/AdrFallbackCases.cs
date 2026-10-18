using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class AdrFallbackCases : TheoryData<string?, string?>
{
    public AdrFallbackCases()
    {
        Add(null, "F1");
        Add("F1", null);
    }
}
