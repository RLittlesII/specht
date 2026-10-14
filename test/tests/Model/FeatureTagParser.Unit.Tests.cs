using System.Globalization;
using AwesomeAssertions;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>The tag parser remains culture independent and recognizes whole literal forms (0001-F5 C-12).</summary>
[Trait("Tier", "Unit")]
public sealed class FeatureTagParserUnitTests
{
    [Theory]
    [InlineData("en-US")]
    [InlineData("tr-TR")]
    public void IgnoringTagCaseUnderDifferentCultures_WhenParsed_ShouldUseTheSameFormGrammarAndOriginalText(string culture)
    {
        // Given
        var structure = EngineServices.Parser().Parse("""
            {"schemaVersion":"0.2.0","claim":{"claimTagForm":"@ISSUE:{claim}","claimTagIgnoreCase":true},
             "identifiers":{"claim":"^I-[0-9]{3}$"}}
            """);
        var parser = new FeatureTagParser();
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);

            // When
            var tags = parser.Parse("# @ISSUE:I-999\n  @issue:i-001 @issue:i-002extra\n", structure);

            // Then
            tags.Should().ContainSingle().Which.Should().Be(new FeatureTag("i-001", 2) { Text = "@issue:i-001" });
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
