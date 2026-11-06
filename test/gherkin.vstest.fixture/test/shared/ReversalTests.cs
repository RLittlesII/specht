using Xunit;

namespace Consumer.Ledger.Tests;

/// <summary>Cases for reversing an amount, compiled from outside the test project.</summary>
public sealed class ReversalTests
{
    /// <summary>A reversal subtracts its amount from the balance.</summary>
    /// <verifies feature="ledger" claim="REQ-LED-002" />
    [Fact]
    public void ReversalSubtractsTheAmount() => Assert.Equal(2, Postings.Reverse(3, 1));

    /// <summary>A pending reversal case, failing until it is written.</summary>
    /// <verifies feature="ledger" claim="REQ-LED-002" />
    [Fact]
    [Trait("Category", "pending")]
    public void ReversalBelowZeroIsPending() => Assert.Equal(-1, Postings.Reverse(0, 0));
}
