using AwesomeAssertions;
using Microsoft.Playwright.Xunit.v3;
using Xunit;

namespace BindingSpike.Frameworks;

/// <spec feature="F1" epic="0001" />
[Trait("Tier", "Unit")]
public sealed class ArithmeticTests
{
    /// <verifies feature="F1" epic="0001" claim="B-001" />
    [Fact]
    public void Fact_WhenIncremented_ShouldMatch()
    {
        // Given
        var value = 1;
        // When
        var result = value + 1;
        // Then
        result.Should().Be(Environment.GetEnvironmentVariable("SPIKE_NEGATIVE") == "1" ? 999 : 2);
    }

    /// <verifies feature="F1" epic="0001" claim="B-002" />
    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    public void InlineTheory_WhenIncremented_ShouldMatch(int value, int expected)
    {
        // Given / When
        var result = value + 1;
        // Then
        result.Should().Be(expected);
    }

    public static TheoryData<int, int> Rows => new() { { 3, 4 }, { 4, 5 } };

    /// <verifies feature="F1" epic="0001" claim="B-002" />
    [Theory]
    [MemberData(nameof(Rows))]
    public void MemberTheory_WhenIncremented_ShouldMatch(int value, int expected)
    {
        // Given / When
        var result = value + 1;
        // Then
        result.Should().Be(expected);
    }

    /// <verifies feature="F1" epic="0001" claim="B-001" />
    [Fact(DisplayName = "Repeated display name")]
    public void First_WhenNamedIdentically_ShouldRemainDistinct() => 1.Should().Be(1);

    /// <verifies feature="F2" epic="0001" claim="B-001" />
    [Fact(DisplayName = "Repeated display name")]
    public void Second_WhenNamedIdentically_ShouldRemainDistinct() => 2.Should().Be(2);

    /// <verifies feature="F1" epic="0001" claim="B-001" />
    [Fact(Skip = "Intentional outcome control")]
    public void Skipped_WhenBound_ShouldRemainSkipped() => throw new InvalidOperationException("Must not run.");
}

/// <spec feature="F1" epic="0001" />
[Trait("Tier", "Integration")]
public sealed class BrowserTests : PageTest
{
    /// <verifies feature="F1" epic="0001" claim="B-001" />
    [Fact]
    public async Task Fact_WhenContentRendered_ShouldMatch()
    {
        // Given
        await Page.SetContentAsync("<button>Ready</button>");
        // When
        var button = Page.GetByRole(Microsoft.Playwright.AriaRole.Button);
        // Then
        await Expect(button).ToHaveTextAsync("Ready");
    }

    /// <verifies feature="F1" epic="0001" claim="B-002" />
    [Theory]
    [InlineData("Alpha")]
    [InlineData("Beta")]
    public async Task Theory_WhenContentRendered_ShouldMatch(string label)
    {
        // Given
        await Page.SetContentAsync($"<button>{label}</button>");
        // When
        var button = Page.GetByRole(Microsoft.Playwright.AriaRole.Button);
        // Then
        await Expect(button).ToHaveTextAsync(label);
    }
}
