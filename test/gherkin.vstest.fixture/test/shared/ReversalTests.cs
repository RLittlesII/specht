using Xunit;

namespace Consumer.Ledger.Tests;

/// <summary>Cases for reversing an amount, compiled from outside the test project.</summary>
public sealed class ReversalTests
{
    /// <summary>A reversal subtracts its amount from the balance.</summary>
    /// <verifies feature="ledger" claim="REQ-LED-002" />
    [Fact]
    public void ReversalSubtractsTheAmount() => Assert.Equal(2, Postings.Reverse(3, 1));
}
