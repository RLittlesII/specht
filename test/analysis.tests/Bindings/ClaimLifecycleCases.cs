using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

public sealed class ClaimLifecycleCases : TheoryData<string, string, bool>
{
    public ClaimLifecycleCases()
    {
        foreach (var feature in new[] { "draft", "in-review", "approved", "superseded" })
        {
            foreach (var claim in new[] { "Active", "Amended", "Withdrawn", "Superseded" })
            {
                Add(claim, feature, feature != "superseded" && (claim == "Active" || claim == "Amended"));
            }
        }
    }
}
