namespace Specht.Analysis.Tests.Gherkin;

public sealed class ClaimFileSetParityCases : TheoryData<string, string, bool?>
{
    public ClaimFileSetParityCases()
    {
        Add("./src/read/claims/*.feature", "src/read/claims/read.feature", true);
        Add("src//read/claims/*.feature", "src/read/claims/read.feature", true);
        Add(string.Join('\\', "src", "read", "claims", "*.feature"), "src/read/claims/read.feature", true);
        Add("src/read/.spec/../claims/*.feature", "src/read/claims/read.feature", true);
        Add("src/read/claims/**", "src/read/claims/more/more.feature", true);
        Add('/' + "src/read/claims/*.feature", "src/read/claims/read.feature", null);
        Add("C" + ':' + "src/read/claims/*.feature", "src/read/claims/read.feature", null);
        Add(string.Join('/', "src", "..", "..", "claims", "*.feature"), "src/read/claims/read.feature", null);
    }
}
