using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class RetiredMarkerCases : TheoryData<string>
{
    public RetiredMarkerCases()
    {
        Add("retired");
        Add("replaced");
    }
}
