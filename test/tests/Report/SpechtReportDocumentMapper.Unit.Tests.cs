using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Model;
using Specht.Report;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Report;

/// <summary>
/// The report document the mapper makes from a report built in memory (<c>0001-F3</c> B-005, B-007; <c>0001-F6</c> B-009,
/// C-7; <c>0001-F7</c> B-032; item <c>0167</c>): what it counts, the name and the order it gives each layout, where it says
/// the schemas came from, and what a violation carries.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtReportDocumentMapperUnitTests
{
    /// <summary>Gets violations of each shape a rule reports: with and without an identifier and a line.</summary>
    public static TheoryData<string, SpechtViolation> Violations =>
        new()
        {
            { "an error with an identifier and a line", new SpechtViolationFixture().WithIdentifier("B-002").WithLine(7) },
            {
                "a warning on the file as a whole, with no identifier",
                new SpechtViolationFixture().WithSeverity(SpechtSeverity.Warning).WithFile("src/area/.spec/README.md").WithLine(0)
            },
        };

    [Fact]
    public void AReport_WhenMadeADocument_ShouldCarryEachLayoutsCountTheItemsTheRulesEvaluatedAndTheSeverityCounts()
    {
        // Given
        SpechtReport report = new SpechtReportFixture()
            .WithSpecificationCount(3)
            .WithLayouts(new SpechtReportLayout("epics", 2), new SpechtReportLayout("features", 1))
            .WithItemCount(4)
            .WithRulesEvaluated(9)
            .WithViolations(
                new SpechtViolationFixture(),
                new SpechtViolationFixture().WithLine(2),
                new SpechtViolationFixture().WithSeverity(SpechtSeverity.Warning));

        // When
        var document = new SpechtReportDocumentMapper().Map(report);

        // Then
        document.Layouts.Should().BeEquivalentTo([new SpechtReportLayout("epics", 2), new SpechtReportLayout("features", 1)]);
        document.ItemCount.Should().Be(4);
        document.RulesEvaluated.Should().Be(9);
        document.ErrorCount.Should().Be(2);
        document.WarningCount.Should().Be(1);
        document.Violations.Should().HaveCount(3);
    }

    [Fact]
    public void AReportNamingItsLayouts_WhenMadeADocument_ShouldCarryEachByItsNameInTheReportsOrderWithALayoutHoldingNoneAtZero()
    {
        // Given
        SpechtReport report = new SpechtReportFixture().WithLayouts(
            new SpechtReportLayout("documentation", 1),
            new SpechtReportLayout("beside-code", 0),
            new SpechtReportLayout("old-tree", 2));

        // When
        var document = new SpechtReportDocumentMapper().Map(report);

        // Then
        document.Layouts.Should().Equal(
            new SpechtReportLayout("documentation", 1),
            new SpechtReportLayout("beside-code", 0),
            new SpechtReportLayout("old-tree", 2));
    }

    [Fact]
    public void AReportCheckedAgainstAPinnedVersion_WhenMadeADocument_ShouldNameThatVersionAndTheEmbeddedSource()
    {
        // Given
        SpechtReport report = new SpechtReportFixture().WithSchemaVersion(new SemanticVersion(0, 2, 0));

        // When
        var document = new SpechtReportDocumentMapper().Map(report);

        // Then
        document.SchemaVersion.Should().Be("0.2.0");
        document.SchemaSource.Should().Be(SpechtSchemaSource.Embedded);
    }

    [Fact]
    public void AReportOfACheckOverAnUpstreamCopy_WhenMadeADocument_ShouldSayTheSchemasCameFromTheUpstreamCopy()
    {
        // Given
        var report = (SpechtReport) new SpechtReportFixture() with { SchemaSource = SpechtSchemaSource.Upstream };

        // When
        var document = new SpechtReportDocumentMapper().Map(report);

        // Then
        document.SchemaSource.Should().Be(SpechtSchemaSource.Upstream);
    }

    [Theory]
    [MemberData(nameof(Violations))]
    public void AViolation_WhenMadeADocument_ShouldCarryItsRuleSeverityFileLineIdentifierMessageAndAnExpectedObject(
        string because,
        SpechtViolation violation)
    {
        // Given
        SpechtReport report = new SpechtReportFixture().WithViolations(violation);

        // When
        var document = new SpechtReportDocumentMapper().Map(report);

        // Then
        var carried = document.Violations.Should().ContainSingle(because).Which;
        carried.RuleId.Should().Be(violation.RuleId, because);
        carried.Severity.Should().Be(violation.Severity, because);
        carried.File.Should().Be(violation.File, because);
        carried.Line.Should().Be(violation.Line, because);
        carried.Identifier.Should().Be(violation.Identifier, because);
        carried.Message.Should().Be(violation.Message, because);
        carried.Expected.Should().NotBeNull(because);
    }
}
