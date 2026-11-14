using AwesomeAssertions;
using Specht.Manifest;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC024, the orphan-companion rule, over a model built in memory whose companions beside no specification are set
/// (<c>0001-F6</c> B-013; item 0441): each is reported once, at line 0, at warning by default.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class OrphanCompanionRuleUnitTests
{
    [Fact]
    public void CompanionsBesideNoSpecificationInTheModel_WhenTheOrphanCompanionRuleEvaluatesIt_ShouldReportEachOnceAtLineZeroAtWarning()
    {
        // Given
        SpechtModel built = new SpechtModelFixture();
        var model = new SpechtModel(built.Features, built.Items, built.Epics, built.Schemas, built.TestFiles)
        {
            OrphanCompanions = ["epics/0001-e/notes/n.feature", "src/a/.spec/a.feature"],
        };
        var rule = EngineServices.Rules().SingleOrDefault(static rule => rule.Id == "SPEC024");

        // When
        rule.Should().NotBeNull("schema 0.4.0 ships the orphan-companion rule");
        var findings = rule!.Evaluate(model).ToArray();

        // Then
        rule.DefaultLevel.Should().Be(SpechtRuleLevel.Warning);
        findings.Select(static finding => (finding.File, finding.Line)).Should().Equal(
            ("epics/0001-e/notes/n.feature", 0),
            ("src/a/.spec/a.feature", 0));
    }
}
