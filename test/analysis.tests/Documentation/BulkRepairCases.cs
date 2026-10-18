using Microsoft.CodeAnalysis.CodeFixes;

namespace Specht.Analysis.Tests.Documentation;

public sealed class BulkRepairCases : TheoryData<string, FixAllScope>
{
    public BulkRepairCases()
    {
        foreach (var key in new[] { "Specht.MaterializeDefault", "Specht.RemoveDuplicates" })
        {
            foreach (var scope in new[] { FixAllScope.Document, FixAllScope.Project, FixAllScope.Solution })
            {
                Add(key, scope);
            }
        }
    }
}
