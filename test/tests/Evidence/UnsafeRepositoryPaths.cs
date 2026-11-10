using Xunit;

namespace Specht.Tests.Evidence;

public sealed class UnsafeRepositoryPaths : TheoryData<string>
{
    public UnsafeRepositoryPaths()
    {
        Add("");
        Add(string.Join('\\', "", "src", "read.feature"));
        Add("c" + ':' + "read.feature");
    }
}
