using Xunit;

namespace Specht.Tests.Evidence;

public sealed class ClaimFileSetSpellings : TheoryData<string, string>
{
    public ClaimFileSetSpellings()
    {
        Add("./src/read/.spec/*.feature", "./src/read/.spec/README.md");
        Add("src//read/.spec/*.feature", "src//read/.spec/README.md");
        Add(string.Join('\\', "src", "read", ".spec", "*.feature"), string.Join('\\', "src", "read", ".spec", "README.md"));
    }
}
