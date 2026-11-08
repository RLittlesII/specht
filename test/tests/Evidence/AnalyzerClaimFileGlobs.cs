using Xunit;

namespace Specht.Tests.Evidence;

public sealed class AnalyzerClaimFileGlobs : TheoryData<string, string>
{
    public AnalyzerClaimFileGlobs()
    {
        Add("./src/read/.spec/*.feature", "src/read/.spec/read.feature");
        Add("src/read/claims/**", "src/read/claims/read.feature");
        Add("src/read/.spec/**", "src/read/.spec/read.feature");
    }
}
