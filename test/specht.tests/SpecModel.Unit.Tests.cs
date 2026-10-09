using System.IO.Abstractions.TestingHelpers;
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
        new[] { "src/sample/.spec/first.feature", "src/sample/.spec/second.feature" },
    };

    [Theory]
    [MemberData(nameof(CompanionCountsOtherThanOne))]
    public void AFeatureBuiltInMemoryWithoutExactlyOneCompanion_WhenTheFeatureFileRuleEvaluatesTheModel_ShouldReportSpec020(
        string[] featureFiles)
    {
        // Given
        var document = SpecDocument.Parse(
            """
            ---
            epic: "0009"
            id: F1
            ---

            ## 3. Acceptance Criteria

            | ID    | Claim  |
            | ----- | ------ |
            | B-001 | First. |
            """,
            RelativePath);
        var schemas = SpecSchemas.Load(
            new MockFileSystem(new Dictionary<string, MockFileData>
            {
                [Path.Combine(Root, ".spec", "schema", "spec-structure.schema.json")] = new("{}"),
                [Path.Combine(Root, ".spec", "schema", "feature-spec.frontmatter.schema.json")] = new("{}"),
                [Path.Combine(Root, ".spec", "schema", "task.frontmatter.schema.json")] = new("{}"),
                [Path.Combine(Root, ".spec", "schema", "epic.frontmatter.schema.json")] = new("{}"),
            }),
            Root);
        var location = new SpecLocation(Path.Combine(Root, RelativePath), RelativePath, SpecLayout.CoLocated, "src/sample/.spec");
        var model = new SpecModel(Root, [new FeatureSpec(location, document, featureFiles)], [], [], schemas);

        // When
        var violations = new FeatureFileRule().Evaluate(model).ToList();

        // Then
        violations.Should().Equal(
            new SpecViolation(
                "SPEC020",
                SpecSeverity.Error,
                RelativePath,
                0,
                "0009-F1",
                $"found {featureFiles.Length} '.feature' files beside this specification - expected exactly one companion"));
    }

    private const string RelativePath = "src/sample/.spec/README.md";

    private const string Root = "repo";
}
