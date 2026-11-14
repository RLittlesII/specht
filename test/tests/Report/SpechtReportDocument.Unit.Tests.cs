using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Model;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// The run summary the report document makes (<c>0001-F2</c> B-002; <c>0001-F6</c> B-009, C-7): what it counts, the name and
/// the order it gives each layout, and that no line of it is in the diagnostic shape. The document is made by the mapper
/// from a report built in memory.
/// </summary>
[Trait("Tier", "Unit")]
public sealed partial class SpechtReportDocumentUnitTests
{
    [Fact]
    public void AReportNamingItsLayouts_WhenSummarized_ShouldNameEachByItsNameInTheReportsOrderWithALayoutHoldingNoneAtZero()
    {
        // Given
        SpechtReport report = new SpechtReportFixture().WithLayouts(
            new SpechtReportLayout("documentation", 1),
            new SpechtReportLayout("beside-code", 0),
            new SpechtReportLayout("old-tree", 2));

        // When
        var summary = new SpechtReportDocumentMapper().Map(report).SummaryLines();

        // Then
        summary[0].Should().Be("specifications: documentation 1, beside-code 0, old-tree 2");
    }

    [Fact]
    public void AReport_WhenSummarized_ShouldPrintEachLayoutsCountThenTheItemsThenTheRulesEvaluatedThenTheSeverityCounts()
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
        var summary = new SpechtReportDocumentMapper().Map(report).SummaryLines();

        // Then
        summary.Should().Equal(
            "specifications: epics 2, features 1",
            "items: 4",
            "rules evaluated: 9",
            "errors: 2, warnings: 1");
    }

    [Fact]
    public void AReportWithViolations_WhenSummarized_ShouldPrintNoLineInTheDiagnosticShape()
    {
        // Given
        SpechtReport report = new SpechtReportFixture()
            .WithSpecificationCount(1)
            .WithLayouts(new SpechtReportLayout("epics", 1), new SpechtReportLayout("features", 0))
            .WithRulesEvaluated(1)
            .WithViolations(new SpechtViolationFixture(), new SpechtViolationFixture().WithSeverity(SpechtSeverity.Warning));

        // When
        var summary = new SpechtReportDocumentMapper().Map(report).SummaryLines();

        // Then
        summary.Should().NotBeEmpty().And.AllSatisfy(static line => Diagnostic().IsMatch(line).Should().BeFalse(line));
    }

    [GeneratedRegex(@"^(?<file>[^\s(:][^(:]*)(\((?<line>\d+)\))?: (?<severity>error|warning) (?<rule>SPEC\d{3,4}): \S.*$")]
    private static partial Regex Diagnostic();
}
