using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class TargetMoveCases : TheoryData<string>
{
    public TargetMoveCases()
    {
        Add("claim");
        Add("repository ADR");
        Add("Feature ADR");
    }
}
