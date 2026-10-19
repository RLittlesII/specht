namespace Specht.TestEvidence.Mtp.Tests;

/// <summary>Provides xUnit fact and theory cases for MTP evidence collection.</summary>
[Trait("Tier", "Integration")]
public sealed class RunnerCases
{
    /// <verifies epic="0201" feature="F3" claim="B-001" />
    [Fact]
    public void FactCase() { }

    /// <verifies epic="0201" feature="F3" claim="B-002" />
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void TheoryCase(int value) => _ = value;

}
