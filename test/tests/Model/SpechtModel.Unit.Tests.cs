using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>
/// A model assembled in memory from its parts and evaluated by a rule (ADR-0001 stage A, item 0104): no tree on disk,
/// no <see cref="SpechtModelLoader"/>.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtModelUnitTests
{
    public static TheoryData<string[]> CompanionCountsOtherThanOne { get; } = new()
    {
        Array.Empty<string>(),
        new[] { "src/sample/.spec/first.feature", "src/sample/.spec/second.feature" },
    };

    /// <summary>
    /// Gets a layout, the path of a specification of epic <c>0001</c> and id <c>F2</c> found in it, and the rules reported
    /// (<c>0001-F6</c> B-004, decision 0009).
    /// </summary>
    public static TheoryData<string, SpechtLayout, string, string[]> IdentityViolationsByDeclaration { get; } = new()
    {
        {
            "a layout declaring both segments is checked at both",
            new SpechtLayout("epics", "epics/**/spec.md", new SpechtPathIdentity(1, 2)),
            "epics/0009-epic/F9-feature/spec.md",
            ["SPEC011", "SPEC011"]
        },
        {
            "a layout declaring none is not checked, whatever it is named",
            new SpechtLayout("epics", "epics/**/spec.md"),
            "epics/0009-epic/F9-feature/spec.md",
            []
        },
        {
            "a layout of another name declaring them is checked",
            new SpechtLayout("old-tree", "epics/**/spec.md", new SpechtPathIdentity(1, 2)),
            "epics/0009-epic/F9-feature/spec.md",
            ["SPEC011", "SPEC011"]
        },
        {
            "segments other than the second and third are read where they are declared",
            new SpechtLayout("documentation", "docs/archive/tree/**/spec.md", new SpechtPathIdentity(3, 4)),
            "docs/archive/tree/0001-epic/F9-feature/spec.md",
            ["SPEC011"]
        },
    };

    [Theory]
    [MemberData(nameof(CompanionCountsOtherThanOne))]
    public void AFeatureBuiltInMemoryWithoutExactlyOneCompanion_WhenTheCompanionFileRuleEvaluatesTheModel_ShouldReportSpec020(
        string[] featureFiles)
    {
        // Given
        const string relativePath = "src/area/.spec/README.md";
        FeatureSpec feature = new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(relativePath))
            .WithDocument(
                new SpechtDocumentFixture().WithFrontmatter(
                    new FrontmatterFixture().WithNode(new JsonObject { ["epic"] = "0009", ["id"] = "F1" })))
            .WithFeatureFiles(featureFiles.Select(static path => (FeatureFile)new FeatureFileFixture().WithRelativePath(path)).ToArray());
        SpechtModel model = new SpechtModelFixture().WithFeatures(feature);
        ISpechtRule rule = new CompanionFileRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        (rule.Id, rule.DefaultLevel).Should().Be(("SPEC020", SpechtRuleLevel.Error));
        findings.Should().Equal(
            new SpechtFinding(
                relativePath,
                0,
                "0009-F1",
                $"found {featureFiles.Length} '.feature' files beside this specification - expected exactly one companion")
            {
                Expected = new JsonObject { ["companionCount"] = 1 },
            });
    }

    [Theory]
    [MemberData(nameof(IdentityViolationsByDeclaration))]
    public void AFeatureUnderFoldersItsIdentityDoesNotName_WhenTheIdentityRuleEvaluatesTheModel_ShouldReportSpec011AtTheSegmentsItsLayoutDeclares(
        string because,
        SpechtLayout layout,
        string relativePath,
        string[] expected)
    {
        // Given
        FeatureSpec feature = new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(relativePath).WithLayout(layout))
            .WithDocument(
                new SpechtDocumentFixture().WithFrontmatter(
                    new FrontmatterFixture().WithNode(new JsonObject { ["epic"] = "0001", ["id"] = "F2" })));
        SpechtModel model = new SpechtModelFixture().WithFeatures(feature);
        var rule = new PathIdentityRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(_ => rule.Id).Should().Equal(expected, because);
    }

    [Fact]
    public void TwoFeaturesDeclaringOneEpicAndId_WhenTheIdentityRuleEvaluatesTheModel_ShouldReportSpec012OnceOnEachUnderTheIdentityNamingBothPaths()
    {
        // Given
        string[] paths = ["src/first/.spec/README.md", "src/second/.spec/README.md"];
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            [
                .. paths.Select(static path => (FeatureSpec)new FeatureSpecFixture()
                    .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
                    .WithDocument(
                        new SpechtDocumentFixture().WithFrontmatter(
                            new FrontmatterFixture().WithNode(new JsonObject { ["epic"] = "0001", ["id"] = "F2" })))),
            ]);
        var rule = new DuplicateIdentityRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        rule.Id.Should().Be("SPEC012");
        findings.Should().AllSatisfy(finding =>
        {
            finding.Identifier.Should().Be("0001-F2");
            finding.Message.Should().Contain(paths[0]).And.Contain(paths[1]);
        });
        findings.Select(static finding => finding.File).Should().BeEquivalentTo(paths);
    }
}
