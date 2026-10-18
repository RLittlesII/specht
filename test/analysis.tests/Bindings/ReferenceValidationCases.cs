using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class ReferenceValidationCases : TheoryData<string, string?, string?, string?, BindingResolution>
{
    public ReferenceValidationCases()
    {
        Add("claim", "0400", null, "B-001", BindingResolution.Incomplete);
        Add("claim", "0400", "F1", null, BindingResolution.Incomplete);
        Add("claim", "0400", "F1", "", BindingResolution.Incomplete);
        Add("claim", "bad", "F1", "B-001", BindingResolution.Invalid);
        Add("claim", "0400", "0400-F1", "B-001", BindingResolution.Invalid);
        Add("claim", "0400", " F1", "B-001", BindingResolution.Invalid);
        Add("claim", "0400", "F1", "B-001 ", BindingResolution.Invalid);
        Add("adr", null, null, null, BindingResolution.Incomplete);
        Add("adr", null, null, "", BindingResolution.Incomplete);
        Add("adr", "0400", null, "ADR-0001", BindingResolution.Invalid);
        Add("adr", "bad", "F1", "ADR-0001", BindingResolution.Invalid);
        Add("adr", "0400", " F1", "ADR-0001", BindingResolution.Invalid);
    }
}
