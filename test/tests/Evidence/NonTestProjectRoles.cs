using Xunit;

namespace Specht.Tests.Evidence;

public sealed class NonTestProjectRoles : TheoryData<string?>
{
    public NonTestProjectRoles()
    {
        Add((string?) null);
        Add("implements");
        Add("follows");
    }
}
