using System.Globalization;
using AwesomeAssertions;
using Specht.Analysis.Bindings.Gherkin;

namespace Specht.Analysis.Tests.Gherkin;

[Trait("Tier", "Unit")]
public sealed class GherkinTagFormUnitTests
{
    [Theory]
    [InlineData("@claim")]
    [InlineData("@{claim}{claim}")]
    [InlineData("@{claim}-{claim}")]
    [InlineData("")]
    public void FormWithoutExactlyOnePlaceholder_WhenConstructed_ShouldFailTheManifestWithTheClaimTagMessage(string form)
    {
        // Given

        // When
        Action construct = () => _ = new GherkinTagForm(form, "claim", false, "^REQ-FIX-[0-9]{3}$");

        // Then
        var failure = construct.Should().Throw<GherkinInputException>().Which;
        failure.RelativePath.Should().BeNull();
        failure.Message.Should().Be("Native claim input requires claim.tag, mapped as /claim.");
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("tr-TR")]
    public void IgnoredCaseUnderDifferentCultures_WhenRead_ShouldFoldOrdinallyAndKeepTheAuthoredText(string culture)
    {
        // Given
        var form = new GherkinTagForm("@ISSUE:{claim}", "claim", true, "^I-[0-9]{3}$");
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);

            // When
            var folded = form.Read("@issue:i-001");

            // Then
            folded.Should().Be("i-001");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
