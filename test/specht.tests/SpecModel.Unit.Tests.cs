using System.Text.Json.Nodes;
using AwesomeAssertions;
using specht.Rules;

namespace specht.tests;

/// <summary>
/// A model assembled in memory from its parts and evaluated by a rule (ADR-0001 stage A, item 0104): no tree on disk,
/// no <see cref="SpecModel.Load"/>.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecModelUnitTests
{
    public static TheoryData<string[]> CompanionCountsOtherThanOne { get; } = new()
    {
        Array.Empty<string>(),
        new[] { "repo/src/sample/.spec/first.feature", "repo/src/sample/.spec/second.feature" },
    };

    [Theory]
    [MemberData(nameof(CompanionCountsOtherThanOne))]
    public void AFeatureBuiltInMemoryWithoutExactlyOneCompanion_WhenTheFeatureFileRuleEvaluatesTheModel_ShouldReportSpec020(
        string[] featureFiles)
    {
        // Given
        const string relativePath = "src/area/.spec/README.md";
        FeatureSpec feature = new FeatureSpecFixture()
            .WithLocation(new SpecLocationFixture().WithRelativePath(relativePath))
            .WithDocument(
                new SpecDocumentFixture().WithFrontmatter(
                    new FrontmatterFixture().WithNode(new JsonObject { ["epic"] = "0009", ["id"] = "F1" })))
            .WithFeatureFiles(featureFiles);
        SpecModel model = new SpecModelFixture().WithFeatures(feature);

        // When
        var violations = new FeatureFileRule().Evaluate(model).ToList();

        // Then
        violations.Should().Equal(
            new SpecViolation(
                "SPEC020",
                SpecSeverity.Error,
                relativePath,
                0,
                "0009-F1",
                $"found {featureFiles.Length} '.feature' files beside this specification - expected exactly one companion"));
    }
}
