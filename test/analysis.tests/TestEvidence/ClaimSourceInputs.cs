using Specht.Analysis.Policy;
using Specht.Analysis.Tests.Gherkin;
using Specht.Analysis.Tests.Policy;

namespace Specht.Analysis.Tests.TestEvidence;

internal sealed class ClaimSourceInputs : TheoryData<List<AnalysisInput>>
{
    public ClaimSourceInputs()
    {
        Add(new AnalysisInputs().Inputs);
        Add(new GherkinInputs().Inputs);
    }
}
