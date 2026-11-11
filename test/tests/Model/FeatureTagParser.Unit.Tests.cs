using System.Globalization;
using AwesomeAssertions;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>
/// The tag parser remains culture independent and recognizes whole literal forms (0001-F5 C-12). The text after the
/// <c>@</c> of every tag a scenario carries is read for the guard (<c>0001-F19</c> B-042, C-18, item <c>0418</c>).
/// </summary>
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
            {"schemaVersion":"0.2.0","claim":{"tag":"@ISSUE:{claim}","tagIgnoreCase":true},
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

    [Fact]
    public void ACompanionWhoseScenariosCarryTags_WhenItsTagNamesAreRead_ShouldGiveTheTextAfterTheAtOfEachTag()
    {
        // Given
        const string text = """
            Feature: Sample

              @B-001
              Scenario: It does the thing
                Given a thing

              @S-abc123 @boundary
              Scenario: It does another
                Given a thing
            """;

        // When
        var names = new FeatureTagParser().TagNames(text);

        // Then
        names.Should().Contain(["B-001", "S-abc123", "boundary"]);
    }
}
