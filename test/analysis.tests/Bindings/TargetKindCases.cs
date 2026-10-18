using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class TargetKindCases : TheoryData<string, BindingTargetKind?>
{
    public TargetKindCases()
    {
        Add("claim", BindingTargetKind.Claim);
        Add("adr", BindingTargetKind.Adr);
        foreach (var kind in new[] { "Feature-only", "epic-only", "task", "constraint", "lesson" })
        {
            Add(kind, null);
        }
    }
}
