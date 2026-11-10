using Xunit;

namespace Specht.Tests.Evidence;

public sealed class UnsafeProjectPaths : TheoryData<string>
{
    public UnsafeProjectPaths()
    {
        Add("");
        Add('/' + "test/tests.csproj");
        Add(string.Join('\\', "test", "tests.csproj"));
        Add("C" + ':' + "tests.csproj");
        Add(string.Join('/', ".", "test", "tests.csproj"));
        Add(string.Join('/', "test", "", "tests.csproj"));
        Add(string.Join('/', "..", "escape.csproj"));
    }
}
