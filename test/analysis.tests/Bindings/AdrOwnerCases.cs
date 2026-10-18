using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class AdrOwnerCases : TheoryData<string?, string?, string>
{
    public AdrOwnerCases()
    {
        Add(null, null, ".spec/adr/0001-choice.md");
        Add("0400", "F1", "src/one/.spec/adr/0001-choice.md");
    }
}
