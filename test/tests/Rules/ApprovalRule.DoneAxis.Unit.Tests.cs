using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// The approval rules over a model built in memory under a pinned manifest (<c>0001-F7</c> B-118 to B-123; decision 0015):
/// from schema 0.3.0 <c>SPEC7001</c> follows the delivery <c>status</c> against the manifest's done marker, whatever the
/// <c>spec_status</c>; below it, <c>SPEC060</c> follows the approved <c>spec_status</c>; <c>SPEC7002</c> follows approval
/// under 0.3.0. Where a test asserts the id, it reads the id the evaluator reports under the pin (<c>0001-F7</c> B-151,
/// B-152), not the rule's own declaration.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ApprovalRuleDoneAxisUnitTests
{
    [Theory]
    [ClassData(typeof(DoneSpecifications))]
    public void ADoneSpecificationWithAMissingRow_WhenTheCoverageRuleEvaluatesItAtZeroThreeZero_ShouldReportSpec7001AtErrorOnTheRowWhateverItsSpecStatus(
        string markers,
        string specStatus,
        string status)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0", ["markers"] = JsonNode.Parse(markers) };
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(
            SpechtDocument.Parse($"---\nspec_status: {specStatus}\nstatus: {status}\n---\n\n" + Sections, SamplePath));
        SpechtModel model = new SpechtModelFixture().WithManifest(manifest).WithFeatures(feature);
        ISpechtRule rule = new ApprovalCoverageRule();

        // When
        var report = new SpechtEvaluator([rule], new SpechtViolationOrder()).Evaluate(model);

        // Then
        report.Violations.Select(static violation => (violation.RuleId, violation.Severity, violation.Identifier))
            .Should()
            .Equal(("SPEC7001", SpechtSeverity.Error, "B-001"));
    }

    [Theory]
    [ClassData(typeof(NotDoneSpecifications))]
    public void ASpecificationWhoseStatusIsNotTheDoneMarker_WhenTheCoverageRuleEvaluatesItAtZeroThreeZero_ShouldReportNothingWhateverItsSpecStatus(
        string markers,
        string specStatus,
        string status)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0", ["markers"] = JsonNode.Parse(markers) };
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(
            SpechtDocument.Parse($"---\nspec_status: {specStatus}\nstatus: {status}\n---\n\n" + Sections, SamplePath));
        SpechtModel model = new SpechtModelFixture().WithManifest(manifest).WithFeatures(feature);
        var rule = new ApprovalCoverageRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(OlderPinApprovedSpecifications))]
    public void AnApprovedSpecificationWithAMissingRow_WhenTheCoverageRuleEvaluatesItBelowZeroThreeZero_ShouldReportSpec060WhateverItsStatus(
        string pin,
        string status)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = pin };
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(
            SpechtDocument.Parse($"---\nspec_status: approved\nstatus: {status}\n---\n\n" + Sections, SamplePath));
        SpechtModel model = new SpechtModelFixture().WithManifest(manifest).WithFeatures(feature);
        var rule = new ApprovalCoverageRule();

        // When
        var report = new SpechtEvaluator([rule], new SpechtViolationOrder()).Evaluate(model);

        // Then
        report.Violations.Select(static violation => (violation.RuleId, violation.Identifier)).Should().Equal(("SPEC060", "B-001"));
    }

    [Fact]
    public void ADoneSpecificationWithAMissingRow_WhenTheCoverageRuleEvaluatesItAtZeroThreeZero_ShouldGiveTheMessageNamingTheDoneStatus()
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0" };
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(
            SpechtDocument.Parse("---\nspec_status: approved\nstatus: done\n---\n\n" + Sections, SamplePath));
        SpechtModel model = new SpechtModelFixture().WithManifest(manifest).WithFeatures(feature);
        var rule = new ApprovalCoverageRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(static finding => finding.Message)
            .Should()
            .Equal("status is 'done' while § 9 still carries a 'Missing' cell");
    }

    [Theory]
    [ClassData(typeof(SignOffSpecifications))]
    public void ASignOffRowAwaitingReview_WhenTheSignOffRuleEvaluatesItAtZeroThreeZero_ShouldReportItOnlyWhenTheSpecStatusIsApproved(
        string specStatus,
        string status,
        bool reported)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0" };
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(
            SpechtDocument.Parse($"---\nspec_status: {specStatus}\nstatus: {status}\n---\n\n" + Sections, SamplePath));
        SpechtModel model = new SpechtModelFixture().WithManifest(manifest).WithFeatures(feature);
        var rule = new ApprovalSignOffRule();

        // When
        var report = new SpechtEvaluator([rule], new SpechtViolationOrder()).Evaluate(model);

        // Then
        report.Violations.Select(static violation => (violation.RuleId, violation.Identifier))
            .Should()
            .Equal(reported ? [("SPEC7002", "1-5")] : []);
    }

    private const string SamplePath = "src/sample/.spec/README.md";

    private const string Sections =
        "## 9. Traceability Matrix\n\n| Claim ID | Test |\n| -------- | ---- |\n| B-001 | Missing |\n\n"
            + "## 12. Sign-off\n\n| Section | Status |\n| ------- | ------ |\n| 1-5 | \U0001F7E1 |\n";
}
