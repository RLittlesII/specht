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
        var document = SpecDocument.Parse($$"""
            ## 9. Traceability Matrix
            | Claim | Scenario | Tests | State |
            | --- | --- | --- | --- |
            | B-003 | Names | {{cell}} | Partial |
            """, "src/sample/.spec/README.md");
        var structure = EngineServices.Parser().Parse("""{"schemaVersion":"0.2.0","tests":{"files":[]}}""");

        // When
        var rows = new MatrixResolver().Resolve(document, structure, []);

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
        var document = SpecDocument.Parse($$"""
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
                ["declarations"] = new JsonArray(declarations.Select(static form => (JsonNode?)JsonValue.Create(form)).ToArray()),
            },
        };
        var structure = EngineServices.Parser().Parse(manifest.ToJsonString());
        SpecTestFile file = new SpecTestFileFixture().WithText(text);

        // When
        var test = new MatrixResolver().Resolve(document, structure, [file]).Single().Tests.Single();

        // Then
        test.Name.Should().Be(name);
        test.Status.Should().Be(line is null ? MatrixTestStatus.Unresolved : MatrixTestStatus.Resolved);
        test.File.Should().Be(line is null ? null : "test/sample.cs");
        test.Line.Should().Be(line);
    }

    [Fact]
    public void RenamedMatrixHeaders_WhenResolved_ShouldKeepColumnRolesAndSourceLines()
    {
        // Given
        var document = SpecDocument.Parse("""
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
        var rows = new MatrixResolver().Resolve(document, structure, []);

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
        var document = SpecDocument.Parse("""
            ## 9. Traceability Matrix
            | Claim ID | Scenario | Test | Status |
            | --- | --- | --- | --- |
            | B-004 | Matching | `Steps.Alpha` | Covered |
            """, "src/sample/.spec/README.md");
        var structure = EngineServices.Parser().Parse("""{"schemaVersion":"0.2.0","tests":{"files":["test/**/*.cs"]}}""");
        SpecTestFile qualifier = new SpecTestFileFixture().WithRelativePath("test/Steps.cs").WithText("class Steps");
        SpecTestFile method = new SpecTestFileFixture().WithRelativePath("test/Alpha.cs").WithText("void Alpha()");

        // When
        var result = new MatrixResolver().Resolve(document, structure, [qualifier, method]).Single().Tests.Single();

        // Then
        result.Status.Should().Be(MatrixTestStatus.Unresolved);
        result.File.Should().BeNull();
        result.Line.Should().BeNull();
    }

    [Fact]
    public void SeveralMatchingFilesAndDeclarations_WhenResolved_ShouldReturnTheFirstOrdinalFileAndNameLine()
    {
        // Given
        var document = SpecDocument.Parse("""
            ## 9. Traceability Matrix
            | Claim ID | Scenario | Test | Status |
            | --- | --- | --- | --- |
            | B-005 | Matching | `Alpha` | Covered |
            """, "src/sample/.spec/README.md");
        var structure = EngineServices.Parser().Parse("""{"schemaVersion":"0.2.0","tests":{"files":["test/**/*.cs"]}}""");
        SpecTestFile later = new SpecTestFileFixture().WithRelativePath("test/z.cs").WithText("Alpha");
        SpecTestFile first = new SpecTestFileFixture().WithRelativePath("test/a.cs").WithText("// preface\nAlpha\nAlpha");

        // When
        var result = new MatrixResolver().Resolve(document, structure, [later, first]).Single().Tests.Single();

        // Then
        result.File.Should().Be("test/a.cs");
        result.Line.Should().Be(2);
    }

    [Fact]
    public void NoTestsSetting_WhenResolved_ShouldKeepNamesUnconfiguredWithoutUsingSuppliedFileTexts()
    {
        // Given
        var document = SpecDocument.Parse("""
            ## 9. Traceability Matrix
            | Claim ID | Scenario | Test | Status |
            | --- | --- | --- | --- |
            | B-005 | Matching | `Alpha` | Covered |
            """, "src/sample/.spec/README.md");
        var structure = EngineServices.Parser().Parse("""{"schemaVersion":"0.1.0"}""");
        SpecTestFile file = new SpecTestFileFixture().WithText("Alpha");

        // When
        var result = new MatrixResolver().Resolve(document, structure, [file]).Single().Tests.Single();

        // Then
        result.Status.Should().Be(MatrixTestStatus.Unconfigured);
        result.File.Should().BeNull();
        result.Line.Should().BeNull();
    }

    [Fact]
    public void NoMatrixSection_WhenResolved_ShouldReturnNoRows()
    {
        // Given
        var document = SpecDocument.Parse("## Other\n`Alpha`", "src/sample/.spec/README.md");
        var structure = EngineServices.Parser().Parse("""{"schemaVersion":"0.2.0"}""");

        // When
        var rows = new MatrixResolver().Resolve(document, structure, []);

        // Then
        rows.Should().BeEmpty();
    }
}
