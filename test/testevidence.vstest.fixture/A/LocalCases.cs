using Xunit;

namespace Fixture.A;

public sealed class LocalCases
{
    /// <verifies feature="F1" claim="B-001" />
    [Fact]
    public void BoundFact() { }

    /// <verifies feature="F1" claim="B-001" />
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void BoundTheory(int value) => Assert.Equal(1, value);

    /// <verifies feature="F1" claim="B-001" />
    [Fact(Skip = "Skipped by the fixture.")]
    public void SkippedFact() { }
}
