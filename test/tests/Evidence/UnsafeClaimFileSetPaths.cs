using Xunit;

namespace Specht.Tests.Evidence;

public sealed class UnsafeClaimFileSetPaths : TheoryData<string, string>
{
    public UnsafeClaimFileSetPaths()
    {
        Add('/' + "src/*.feature", "src/read/.spec/README.md");
        Add(string.Join('/', "..", "x", "*.feature"), "src/read/.spec/README.md");
        Add(string.Join('/', "src", "..", "..", "x.feature"), "src/read/.spec/README.md");
        Add("C" + ':' + "x.feature", "src/read/.spec/README.md");
        Add(".", "src/read/.spec/README.md");
        Add("src/read/.spec/*.feature", string.Join('/', "..", "README.md"));
    }
}
