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

    public static TheoryData<string, string, string[]> IdentityViolationsByLayout { get; } = new()
    {
        { "epics", "epics/**/spec.md", ["SPEC011", "SPEC011"] },
        { "features", "**/.spec/README.md", [] },
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

    [Theory]
    [MemberData(nameof(IdentityViolationsByLayout))]
    public void AFeatureUnderFoldersItsIdentityDoesNotName_WhenTheIdentityRuleEvaluatesTheModel_ShouldReportSpec011InTheEpicsLayoutOnly(
        string layout,
        string glob,
        string[] expected)
    {
        // Given
        FeatureSpec feature = new FeatureSpecFixture()
            .WithLocation(new SpecLocationFixture().WithRelativePath("src/area/.spec/README.md").WithLayout(new SpecLayout(layout, glob)))
            .WithDocument(
                new SpecDocumentFixture().WithFrontmatter(
                    new FrontmatterFixture().WithNode(new JsonObject { ["epic"] = "0001", ["id"] = "F2" })));
        SpecModel model = new SpecModelFixture().WithFeatures(feature);

        // When
        var violations = new IdentityRule().Evaluate(model).ToList();

        // Then
        violations.Select(static violation => violation.RuleId).Should().Equal(expected);
    }

    [Fact]
    public void TwoFeaturesDeclaringOneEpicAndId_WhenTheIdentityRuleEvaluatesTheModel_ShouldReportSpec012OnceOnEachUnderTheIdentityNamingBothPaths()
    {
        // Given
        string[] paths = ["src/first/.spec/README.md", "src/second/.spec/README.md"];
        SpecModel model = new SpecModelFixture().WithFeatures(
            [
                .. paths.Select(static path => (FeatureSpec)new FeatureSpecFixture()
                    .WithLocation(new SpecLocationFixture().WithRelativePath(path))
                    .WithDocument(
                        new SpecDocumentFixture().WithFrontmatter(
                            new FrontmatterFixture().WithNode(new JsonObject { ["epic"] = "0001", ["id"] = "F2" })))),
            ]);

        // When
        var violations = new IdentityRule().Evaluate(model).ToList();

        // Then
        violations.Should().AllSatisfy(violation =>
        {
            violation.RuleId.Should().Be("SPEC012");
            violation.Identifier.Should().Be("0001-F2");
            violation.Message.Should().Contain(paths[0]).And.Contain(paths[1]);
        });
        violations.Select(static violation => violation.File).Should().BeEquivalentTo(paths);
    }
}
