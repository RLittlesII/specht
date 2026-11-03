using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC011's order and guards over a model built in memory (<c>0001-F1</c> B-004; item 0158): the Features' order and, for
/// one Feature, the epic mismatch before the id mismatch; a Feature with no <c>epic</c>, no <c>id</c> or a declared segment
/// outside its path is skipped.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class PathIdentityRuleUnitTests
{
    [Fact]
    public void FeaturesUnderFoldersTheirIdentitiesDoNotName_WhenThePathIdentityRuleEvaluatesTheModel_ShouldReportSpec011InFeatureOrderEpicBeforeId()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("epics/0002-two/F9-nine/spec.md", Legacy, "0001", "F2"),
            Feature("epics/0001-one/F8-eight/spec.md", Legacy, "0001", "F2"));
        var rule = new PathIdentityRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Line, finding.Message)).Should().Equal(
            ("SPEC011", "epics/0002-two/F9-nine/spec.md", EpicLine, "frontmatter epic '0001' does not match the containing directory '0002-two'"),
            ("SPEC011", "epics/0002-two/F9-nine/spec.md", IdLine, "frontmatter id 'F2' does not match the containing directory 'F9-nine'"),
            ("SPEC011", "epics/0001-one/F8-eight/spec.md", IdLine, "frontmatter id 'F2' does not match the containing directory 'F8-eight'"));
    }

    [Fact]
    public void FeaturesWithNoEpicNoIdOrASegmentOutsideThePath_WhenThePathIdentityRuleEvaluatesTheModel_ShouldSkipThem()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("epics/0002-two/F9-nine/spec.md", Legacy, null, "F2"),
            Feature("epics/0003-three/F9-nine/spec.md", Legacy, "0001", null),
            Feature("epics/0004-four/F9-nine/spec.md", new SpechtLayout("legacy", "epics/**/spec.md", new SpechtPathIdentity(1, 3)), "0001", "F2"),
            Feature("epics/0005-five/F9-nine/spec.md", new SpechtLayout("legacy", "epics/**/spec.md", new SpechtPathIdentity(3, 2)), "0001", "F2"),
            Feature("epics/0006-six/F2-two/spec.md", Legacy, "0001", "F2"));
        var rule = new PathIdentityRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Line)).Should().Equal(("SPEC011", "epics/0006-six/F2-two/spec.md", EpicLine));
    }

    private static SpechtLayout Legacy => new("legacy", "epics/**/spec.md", new SpechtPathIdentity(1, 2));

    private static FeatureSpec Feature(string path, SpechtLayout layout, string? epic, string? id) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path).WithLayout(layout))
            .WithDocument(
                new SpechtDocumentFixture().WithRelativePath(path).WithFrontmatter(
                    new FrontmatterFixture()
                        .WithNode(new JsonObject { ["epic"] = epic, ["id"] = id })
                        .WithKeyLines(new Dictionary<string, int>(StringComparer.Ordinal) { ["epic"] = EpicLine, ["id"] = IdLine })));

    private const int EpicLine = 3;

    private const int IdLine = 2;
}
