using Xunit;

namespace Consumer.Ledger.Tests;

/// <summary>Pending cases, compiled from outside the test project and outside the verifies enforcement scope.</summary>
public sealed class PendingTests
{
    /// <summary>A pending posting case, skipped until it is written.</summary>
    [Fact(Skip = "Pending in the fixture.")]
    [Trait("Category", "pending")]
    public void PostingOfAForeignAmountIsPending() => Assert.Equal(3, Postings.Post(1, 2));

    /// <summary>A pending reversal case, failing until it is written.</summary>
    [Fact]
    [Trait("Category", "pending")]
    public void ReversalBelowZeroIsPending() => Assert.Equal(-1, Postings.Reverse(0, 0));
}
