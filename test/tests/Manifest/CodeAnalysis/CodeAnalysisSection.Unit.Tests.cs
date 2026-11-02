using AwesomeAssertions;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest.CodeAnalysis;

[Trait("Tier", "Unit")]
public sealed class CodeAnalysisSectionUnitTests
{
    [Theory]
    [ClassData(typeof(ShippedSchemaPins))]
    public void AnalysisSection_WhenAddedToAnExistingPin_ShouldPreserveThePinAndCheckerSettings(string pin)
    {
        // Given
        var parser = EngineServices.Parser();
        var original = "{\"schemaVersion\":\"" + pin + "\"}";
        var extended = "{\"schemaVersion\":\"" + pin + "\",\"codeAnalysis\":{\"projects\":[],\"features\":[]}}";
        var before = parser.Parse(original);

        // When
        var after = parser.Parse(extended);

        // Then
        after.SchemaVersion.Should().Be(before.SchemaVersion);
        after.Sections.Should().Equal(before.Sections);
        after.Identifiers.Should().BeEquivalentTo(before.Identifiers);
        parser.PinnedVersion(after).RuleIds.Should().BeEquivalentTo(parser.PinnedVersion(before).RuleIds);
    }
}
