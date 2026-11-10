using Xunit;

namespace Specht.Tests.Evidence;

public sealed class ExportRequirements : TheoryData<string?, string?, string?, bool>
{
    public ExportRequirements()
    {
        Add("@visual", null, null, true);
        Add(null, "verifies", "test/tests.csproj", true);
        Add(null, null, null, false);
        Add(null, "implements", "test/tests.csproj", false);
        Add(null, "verifies", "test/other.csproj", false);
        Add(null, null, "test/tests.csproj", false);
        Add(null, "verifies", null, false);
    }
}
