using Xunit;

namespace Specht.Tests.Model;

public sealed class RepositoryGlobMatches : TheoryData<string, string, bool>
{
    public RepositoryGlobMatches()
    {
        Add("src/read/.spec/x.feature", "src/read/**", true);
        Add("src/other/x.feature", "src/read/**", false);
        Add("src/read/a/b/x.feature", "src/read/**/x.feature", true);
        Add("src/read/x.feature", "src/read/**/x.feature", true);
        Add("src/read/a/x.feature", "src/read/*.feature", false);
        Add("src/read/xay.feature", "src/read/x.y.feature", false);
        Add("src/read/.spec/x.feature", "./src//read/.spec/*.feature", true);
        Add(string.Join('\\', "src", "read", ".spec", "x.feature"), "src/read/.spec/*.feature", true);
    }
}
