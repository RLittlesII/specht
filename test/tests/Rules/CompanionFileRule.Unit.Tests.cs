using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>SPEC020's order over a model built in memory (<c>0001-F1</c> B-004; item 0159): the Features' order.</summary>
[Trait("Tier", "Unit")]
public sealed class CompanionFileRuleUnitTests
{
    [Fact]
    public void FeaturesWithNoneOrTwoCompanions_WhenTheCompanionFileRuleEvaluatesTheModel_ShouldReportSpec020InFeatureOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/b/.spec/README.md"),
            Feature("src/one/.spec/README.md", new FeatureFileFixture().WithRelativePath("src/one/.spec/one.feature")),
            Feature(
                "src/a/.spec/README.md",
                new FeatureFileFixture().WithRelativePath("src/a/.spec/first.feature"),
                new FeatureFileFixture().WithRelativePath("src/a/.spec/second.feature")));
        var rule = new CompanionFileRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Message)).Should().Equal(
            ("SPEC020", "src/b/.spec/README.md", "found 0 '.feature' files beside this specification - expected exactly one companion"),
            ("SPEC020", "src/a/.spec/README.md", "found 2 '.feature' files beside this specification - expected exactly one companion"));
    }

    private static FeatureSpec Feature(string path, params FeatureFile[] companions) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(new SpechtDocumentFixture().WithRelativePath(path))
            .WithFeatureFiles(companions);
}
