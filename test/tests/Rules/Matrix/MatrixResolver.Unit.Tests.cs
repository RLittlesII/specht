using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Model;
using Specht.Rules.Matrix;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules.Matrix;

/// <summary>The matrix resolver reads parsed code spans and file text without I/O or rule verdicts.</summary>
[Trait("Tier", "Unit")]
public sealed class MatrixResolverUnitTests
{
    [Theory]
    [ClassData(typeof(MatrixNameCells))]
    public void ATestCell_WhenResolved_ShouldReadOnlyInlineCodeNamesInOrder(string cell, string[] expected)
    {
        // Given
        var document = DocumentParser.Parse($$"""
            ## 9. Traceability Matrix
            | Claim | Scenario | Tests | State |
            | --- | --- | --- | --- |
            | B-003 | Names | {{cell}} | Partial |
            """, "src/sample/.spec/README.md");
        var structure = EngineServices.Parser().Parse("""{"schemaVersion":"0.2.0","tests":{"files":[]}}""");

        // When
        var rows = new MatrixResolver().Resolve(new FeatureSpecFixture().WithDocument(document), structure, []);

        // Then
        rows.Should().ContainSingle().Which.Tests.Select(static test => test.Name).Should().Equal(expected);
        rows[0].Tests.Should().OnlyContain(static test => test.Status == MatrixTestStatus.Unresolved && test.File == null && test.Line == null);
    }

    [Theory]
    [ClassData(typeof(MatrixNameMatches))]
    public void ATestName_WhenResolved_ShouldRequireItsDeclaredFormAndEveryQualifierInOneFile(
        string name, string text, string[] declarations, int? line)
    {
        // Given
        var document = DocumentParser.Parse($$"""
            ## 9. Traceability Matrix
            | Claim ID | Scenario | Test | Status |
            | --- | --- | --- | --- |
            | B-005 | Matching | `{{name}}` | Covered |
            """, "src/sample/.spec/README.md");
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.2.0",
            ["tests"] = new JsonObject
            {
                ["files"] = new JsonArray("test/**/*.cs"),
                ["declarations"] = new JsonArray(declarations.Select(static form => (JsonNode?) JsonValue.Create(form)).ToArray()),
            },
        };
        var structure = EngineServices.Parser().Parse(manifest.ToJsonString());
        SpechtTestFile file = new SpechtTestFileFixture().WithText(text);

        // When
        var test = new MatrixResolver().Resolve(new FeatureSpecFixture().WithDocument(document), structure, [file]).Single().Tests.Single();

        // Then
        test.Name.Should().Be(name);
        test.Status.Should().Be(line is null ? MatrixTestStatus.Unresolved : MatrixTestStatus.Resolved);
        test.File.Should().Be(line is null ? null : "test/sample.cs");
        test.Line.Should().Be(line);
    }

    [Theory]
    [ClassData(typeof(ScenarioTitleResolutions))]
    public void AScenarioCell_WhenResolved_ShouldSplitOnSemicolonsAndResolveEachTitleToAScenarioCarryingTheRowTag(
        string? matrix, bool tagIgnoreCase, string claim, string cell, string[] titles, MatrixScenarioStatus[] statuses, int?[] lines)
    {
        // Given
        const string companion = "src/sample/.spec/sample.feature";
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["claim"] = new JsonObject { ["tagIgnoreCase"] = tagIgnoreCase },
        };
        if (matrix is not null)
        {
            manifest["matrix"] = JsonNode.Parse(matrix);
        }

        var structure = EngineServices.Parser().Parse(manifest.ToJsonString());
        var file = new FeatureTagParser().Read(companion, """
            @B-001
            Feature: Sample

              @B-002
              Scenario: First case
                Given a thing

              @B-003
              Scenario Outline: Second case
                Given <thing>

                @B-004
                Examples:
                  | thing |
                  | one   |

              @B-005
              Example: An example
                Given a thing

              @B-006
              Scenario: Twin
                Given a thing

              @B-007
              Scenario: Twin
                Given a thing
            """, structure);
        FeatureSpec feature = new FeatureSpecFixture()
            .WithDocument(DocumentParser.Parse($$"""
                ## 9. Traceability Matrix
                | Claim ID | Scenario | Test | Status |
                | --- | --- | --- | --- |
                | {{claim}} | {{cell}} | `AlphaTest` | Partial |
                """, "src/sample/.spec/README.md"))
            .WithFeatureFiles(file);

        // When
        var rows = new MatrixResolver().Resolve(feature, structure, []);

        // Then
        rows.Should().ContainSingle().Which.Scenarios.Should().Equal(titles.Select((title, index) =>
            new MatrixScenarioResolution(title, statuses[index], lines[index] is null ? null : companion, lines[index])));
    }

    [Fact]
    public void RenamedMatrixHeaders_WhenResolved_ShouldKeepColumnRolesAndSourceLines()
    {
        // Given
        var document = DocumentParser.Parse("""
            ## 9. Traceability Matrix

            | Requirement | Description | Evidence | State |
            | --- | --- | --- | --- |
            | B-001 | First | `Alpha` | Covered |
            | B-002 | Second | `Beta` | Partial |
            """, "src/sample/.spec/README.md");
        var structure = EngineServices.Parser().Parse("""
            {"schemaVersion":"0.2.0",
             "tables":{"matrix":["Requirement","Description","Evidence","State"]},"tests":{"files":[]}}
            """);

        // When
        var rows = new MatrixResolver().Resolve(new FeatureSpecFixture().WithDocument(document), structure, []);

        // Then
        rows.Select(static row => row.Claim).Should().Equal("B-001", "B-002");
        rows.Select(static row => row.Scenario).Should().Equal("First", "Second");
        rows.Select(static row => row.Test).Should().Equal("Alpha", "Beta");
        rows.Select(static row => row.Status).Should().Equal("Covered", "Partial");
        rows.Select(static row => row.Line).Should().Equal(5, 6);
    }

    [Fact]
    public void DottedPartsInSeparateFiles_WhenResolved_ShouldNotCombineThem()
    {
        // Given
        var document = DocumentParser.Parse("""
            ## 9. Traceability Matrix
            | Claim ID | Scenario | Test | Status |
            | --- | --- | --- | --- |
            | B-004 | Matching | `Steps.Alpha` | Covered |
            """, "src/sample/.spec/README.md");
        var structure = EngineServices.Parser().Parse("""{"schemaVersion":"0.2.0","tests":{"files":["test/**/*.cs"]}}""");
        SpechtTestFile qualifier = new SpechtTestFileFixture().WithRelativePath("test/Steps.cs").WithText("class Steps");
        SpechtTestFile method = new SpechtTestFileFixture().WithRelativePath("test/Alpha.cs").WithText("void Alpha()");

        // When
        var result = new MatrixResolver().Resolve(new FeatureSpecFixture().WithDocument(document), structure, [qualifier, method]).Single().Tests.Single();

        // Then
        result.Status.Should().Be(MatrixTestStatus.Unresolved);
        result.File.Should().BeNull();
        result.Line.Should().BeNull();
    }

    [Fact]
    public void SeveralMatchingFilesAndDeclarations_WhenResolved_ShouldReturnTheFirstOrdinalFileAndNameLine()
    {
        // Given
        var document = DocumentParser.Parse("""
            ## 9. Traceability Matrix
            | Claim ID | Scenario | Test | Status |
            | --- | --- | --- | --- |
            | B-005 | Matching | `Alpha` | Covered |
            """, "src/sample/.spec/README.md");
        var structure = EngineServices.Parser().Parse("""{"schemaVersion":"0.2.0","tests":{"files":["test/**/*.cs"]}}""");
        SpechtTestFile later = new SpechtTestFileFixture().WithRelativePath("test/z.cs").WithText("Alpha");
        SpechtTestFile first = new SpechtTestFileFixture().WithRelativePath("test/a.cs").WithText("// preface\nAlpha\nAlpha");

        // When
        var result = new MatrixResolver().Resolve(new FeatureSpecFixture().WithDocument(document), structure, [later, first]).Single().Tests.Single();

        // Then
        result.File.Should().Be("test/a.cs");
        result.Line.Should().Be(2);
    }

    [Fact]
    public void NoTestsSetting_WhenResolved_ShouldKeepNamesUnconfiguredWithoutUsingSuppliedFileTexts()
    {
        // Given
        var document = DocumentParser.Parse("""
            ## 9. Traceability Matrix
            | Claim ID | Scenario | Test | Status |
            | --- | --- | --- | --- |
            | B-005 | Matching | `Alpha` | Covered |
            """, "src/sample/.spec/README.md");
        var structure = EngineServices.Parser().Parse("""{"schemaVersion":"0.1.0"}""");
        SpechtTestFile file = new SpechtTestFileFixture().WithText("Alpha");

        // When
        var result = new MatrixResolver().Resolve(new FeatureSpecFixture().WithDocument(document), structure, [file]).Single().Tests.Single();

        // Then
        result.Status.Should().Be(MatrixTestStatus.Unconfigured);
        result.File.Should().BeNull();
        result.Line.Should().BeNull();
    }

    [Fact]
    public void NoMatrixSection_WhenResolved_ShouldReturnNoRows()
    {
        // Given
        var document = DocumentParser.Parse("## Other\n`Alpha`", "src/sample/.spec/README.md");
        var structure = EngineServices.Parser().Parse("""{"schemaVersion":"0.2.0"}""");

        // When
        var rows = new MatrixResolver().Resolve(new FeatureSpecFixture().WithDocument(document), structure, []);

        // Then
        rows.Should().BeEmpty();
    }

    [Fact]
    public void AClaimWhoseRowNamesATestADeclaredFileHolds_WhenClaimsAreResolvedWithTheFiles_ShouldCarryEachTestsResolutionFileAndLine()
    {
        // Given
        var document = DocumentParser.Parse("""
            ## 3. Acceptance Criteria
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-001 | It traces. | brd | Active |

            ## 9. Traceability Matrix
            | Claim ID | Scenario | Test | Status |
            | --- | --- | --- | --- |
            | B-001 | Tracing | `Alpha`, `Beta` | Partial |
            """, "src/sample/.spec/README.md");
        var structure = EngineServices.Parser().Parse("""{"schemaVersion":"0.2.0","tests":{"files":["test/**/*.cs"]}}""");
        SpechtTestFile file = new SpechtTestFileFixture().WithText("// preface\nvoid Alpha()");

        // When
        var claim = new MatrixResolver().ResolveClaims(
            new FeatureSpecFixture().WithDocument(document), structure, new SpechtClaims(document, structure, null), [file]).Single();

        // Then
        claim.Rows.Should().ContainSingle().Which.Tests.Should().Equal(
            new MatrixTestResolution("Alpha", MatrixTestStatus.Resolved, "test/sample.cs", 2),
            new MatrixTestResolution("Beta", MatrixTestStatus.Unresolved));
    }

    [Theory]
    [ClassData(typeof(ClaimScenarioCompanions))]
    public void ACompanionScenarioCarryingAClaimsTag_WhenClaimsAreResolved_ShouldListEachSuchScenarioOnceInCompanionThenFileOrder(
        bool tagIgnoreCase, string claim, string[] titles, string[] files, int[] lines)
    {
        // Given
        var structure = EngineServices.Parser().Parse(new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["claim"] = new JsonObject { ["tagIgnoreCase"] = tagIgnoreCase },
        }.ToJsonString());
        var document = DocumentParser.Parse($$"""
            ## 3. Acceptance Criteria
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | {{claim}} | It traces. | brd | Active |
            """, "src/sample/.spec/README.md");
        FeatureSpec feature = new FeatureSpecFixture()
            .WithDocument(document)
            .WithFeatureFiles(
                new FeatureTagParser().Read("src/sample/.spec/a.feature", """
                    @B-001
                    Feature: Sample

                      @B-002
                      Scenario: First case
                        Given a thing

                      @B-002 @b-002
                      Scenario: Twice tagged
                        Given a thing

                      @B-005
                      Rule: A rule

                        @B-003
                        Scenario Outline: Second case
                          Given <thing>

                          @B-004
                          Examples:
                            | thing |
                            | one   |
                    """, structure),
                new FeatureTagParser().Read("src/sample/.spec/b.feature", """
                    Feature: Other

                      @B-002
                      Scenario: Third case
                        Given a thing
                    """, structure));

        // When
        var resolved = new MatrixResolver().ResolveClaims(feature, structure, new SpechtClaims(document, structure, null), []).Single();

        // Then
        resolved.Scenarios.Should().Equal(titles.Select((title, index) =>
            new MatrixScenarioResolution(title, MatrixScenarioStatus.Resolved, files[index], lines[index])));
    }

    private static SpechtDocumentParser DocumentParser => new(new FrontmatterReader());
}
