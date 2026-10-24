using Xunit;

namespace Fixture.Shared;

public sealed class SharedCases
{
    /// <verifies feature="F1" claim="B-002" />
    [Fact]
    public void NonvisualCase() { }

    /// <verifies feature="F1" claim="B-002" />
    [Fact]
    [Trait("Category", "visual")]
    [Trait("Category", "smoke")]
    public void VisualCase() { }
}
