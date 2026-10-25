using AwesomeAssertions;
using Specht.Analysis.Bindings.Gherkin;
using Specht.Manifest;
using Specht.Model;
using Specht.Versioning;

namespace Specht.Analysis.Tests.Gherkin;

[Trait("Tier", "Unit")]
public sealed class ClaimTagParityUnitTests
{
    [Theory]
    [ClassData(typeof(ClaimTagCases))]
    public void ClaimTagToken_WhenReadByTheEngineAndTheAnalyzer_ShouldGiveTheSameAnswerFromBoth(
        string form, bool ignoreCase, string grammar, string token, string? expected)
    {
        // Given
        var structure = new SpecStructure(
            [],
            new Dictionary<string, IReadOnlyList<string>>(),
            new Dictionary<string, string> { ["claim"] = grammar },
            new SemanticVersion(0, 2, 0))
        {
            ClaimTagForm = form,
            ClaimTagIgnoreCase = ignoreCase,
        };
        var tagForm = new GherkinTagForm(form, "claim", ignoreCase, grammar);

        // When
        var engine = new FeatureTagParser().Parse(token, structure).SingleOrDefault()?.Id;
        var analyzer = tagForm.Read(token);

        // Then
        engine.Should().Be(expected);
        analyzer.Should().Be(expected);
    }
}
