using Xunit;

namespace Consumer.Ledger.Tests;

/// <summary>Cases for posting an amount.</summary>
public sealed class PostingTests
{
    /// <summary>A posting adds its amount to the balance.</summary>
    /// <verifies feature="ledger" claim="REQ-LED-001" />
    [Fact]
    public void PostingAddsTheAmount() => Assert.Equal(3, Postings.Post(1, 2));

    /// <summary>Each row's posting adds that row's amount.</summary>
    /// <verifies feature="ledger" claim="REQ-LED-001" />
    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 1, 2)]
    [InlineData(2, 3, 5)]
    public void PostingAddsEachRowsAmount(int balance, int amount, int expected) => Assert.Equal(expected, Postings.Post(balance, amount));

    /// <summary>A pending posting case, skipped until it is written.</summary>
    /// <verifies feature="ledger" claim="REQ-LED-001" />
    [Fact(Skip = "Pending in the fixture.")]
    [Trait("Category", "pending")]
    public void PostingOfAForeignAmountIsPending() => Assert.Equal(3, Postings.Post(1, 2));
}
