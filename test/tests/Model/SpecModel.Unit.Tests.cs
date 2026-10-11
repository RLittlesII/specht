using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>
/// A model assembled in memory from its parts and evaluated by a rule (ADR-0001 stage A, item 0104): no tree on disk,
/// no <see cref="SpecModelLoader"/>.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecModelUnitTests
{
    public static TheoryData<string[]> CompanionCountsOtherThanOne { get; } = new()
    {
        Array.Empty<string>(),
        new[] { "repo/src/sample/.spec/first.feature", "repo/src/sample/.spec/second.feature" },
    };

    /// <summary>
    /// Gets a layout, the path of a specification of epic <c>0001</c> and id <c>F2</c> found in it, and the rules reported
    /// (<c>0001-F6</c> B-004, decision 0009).
    /// </summary>
    public static TheoryData<string, SpecLayout, string, string[]> IdentityViolationsByDeclaration { get; } = new()
    {
        {
            "a layout declaring both segments is checked at both",
            new SpecLayout("epics", "epics/**/spec.md", new SpecPathIdentity(1, 2)),
            "epics/0009-epic/F9-feature/spec.md",
            ["SPEC011", "SPEC011"]
        },
        {
            "a layout declaring none is not checked, whatever it is named",
            new SpecLayout("epics", "epics/**/spec.md"),
            "epics/0009-epic/F9-feature/spec.md",
            []
        },
        {
            "a layout of another name declaring them is checked",
            new SpecLayout("old-tree", "epics/**/spec.md", new SpecPathIdentity(1, 2)),
            "epics/0009-epic/F9-feature/spec.md",
            ["SPEC011", "SPEC011"]
        },
        {
            "segments other than the second and third are read where they are declared",
            new SpecLayout("documentation", "docs/archive/tree/**/spec.md", new SpecPathIdentity(3, 4)),
            "docs/archive/tree/0001-epic/F9-feature/spec.md",
            ["SPEC011"]
        },
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
        var violations = new FeatureFileRule(new FeatureFileReader(new MockFileSystem())).Evaluate(model).ToList();

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
    [MemberData(nameof(IdentityViolationsByDeclaration))]
    public void AFeatureUnderFoldersItsIdentityDoesNotName_WhenTheIdentityRuleEvaluatesTheModel_ShouldReportSpec011AtTheSegmentsItsLayoutDeclares(
        string because,
        SpecLayout layout,
        string relativePath,
        string[] expected)
    {
        // Given
        FeatureSpec feature = new FeatureSpecFixture()
            .WithLocation(new SpecLocationFixture().WithRelativePath(relativePath).WithLayout(layout))
            .WithDocument(
                new SpecDocumentFixture().WithFrontmatter(
                    new FrontmatterFixture().WithNode(new JsonObject { ["epic"] = "0001", ["id"] = "F2" })));
        SpecModel model = new SpecModelFixture().WithFeatures(feature);

        // When
        var violations = new IdentityRule().Evaluate(model).ToList();

        // Then
        violations.Select(static violation => violation.RuleId).Should().Equal(expected, because);
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
