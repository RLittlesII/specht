using AwesomeAssertions;
using Specht.Model;

namespace Specht.Tests.Model;

/// <summary>
/// A specification parsed from text held in memory (ADR-0001 stage A, item 0104): the frontmatter, the sections, the
/// table each section opens with, and the one-based line of each, with no file behind it. A grid table is read as no table
/// (<c>0001-F1</c> B-008).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecDocumentUnitTests
{
    /// <summary>Gets each section of <see cref="Specification"/> with its heading's one-based line.</summary>
    public static TheoryData<string, int> SectionLines { get; } = new()
    {
        { "1. Business Goal", 8 },
        { "3. Acceptance Criteria", 12 },
    };

    /// <summary>
    /// Gets each section of <see cref="Specification"/> with the header cells, body rows and one-based row lines of the
    /// table it opens with: empty for the section with no table.
    /// </summary>
    public static TheoryData<string, string[], string[][], int[]> SectionTables { get; } = new()
    {
        { "1. Business Goal", [], [], [] },
        { "3. Acceptance Criteria", ["ID", "Claim"], [["B-001", "First."], ["B-002", "Second."]], [16, 17] },
    };

    [Fact]
    public void ASpecificationHeldAsText_WhenParsed_ShouldCarryTheRelativePathAndTheFrontmatterWithTheLineOfEachKey()
    {
        // Given
        var text = Specification;

        // When
        var document = SpecDocument.Parse(text, RelativePath);

        // Then
        document.RelativePath.Should().Be(RelativePath);
        document.Frontmatter.StartLine.Should().Be(1);
        document.Frontmatter.Text("epic").Should().Be("0009");
        document.Frontmatter.Text("id").Should().Be("F1");
        document.Frontmatter.LineOf("epic").Should().Be(2);
        document.Frontmatter.LineOf("id").Should().Be(3);
    }

    [Theory]
    [MemberData(nameof(SectionLines))]
    public void ASpecificationHeldAsText_WhenParsed_ShouldCarryTheSectionAtItsHeadingsOneBasedLine(string title, int line)
    {
        // Given
        var text = Specification;

        // When
        var document = SpecDocument.Parse(text, RelativePath);

        // Then
        document.Section(title).Should().NotBeNull().And.Subject.As<SpecSection>().Line.Should().Be(line);
    }

    [Theory]
    [MemberData(nameof(SectionTables))]
    public void ASection_WhenParsed_ShouldCarryTheHeadersRowsAndOneBasedRowLinesOfTheTableItOpensWith(
        string title,
        string[] headers,
        string[][] rows,
        int[] rowLines)
    {
        // Given
        var text = Specification;

        // When
        var section = SpecDocument.Parse(text, RelativePath).Section(title)!;

        // Then
        section.Headers.Should().Equal(headers);
        section.Rows.Should().BeEquivalentTo(rows, static options => options.WithStrictOrdering());
        section.RowLines.Should().Equal(rowLines);
    }

    [Fact]
    public void ASectionHoldingAGridTable_WhenParsed_ShouldCarryNoHeadersAndNoRows()
    {
        // Given
        const string text =
            """
            ## 9. Traceability Matrix

            +----------+-------------------+---------+---------+
            | Claim ID | Scenario          | Test    | Status  |
            +==========+===================+=========+=========+
            | B-001    | It does the thing | Missing | Missing |
            +----------+-------------------+---------+---------+
            """;

        // When
        var section = SpecDocument.Parse(text, RelativePath).Section("9. Traceability Matrix")!;

        // Then
        section.Headers.Should().BeEmpty();
        section.Rows.Should().BeEmpty();
        section.RowLines.Should().BeEmpty();
    }

    private const string RelativePath = "src/sample/.spec/README.md";

    private const string Specification =
        """
        ---
        epic: "0009"
        id: F1
        ---

        # Sample

        ## 1. Business Goal

        Prose with no table.

        ## 3. Acceptance Criteria

        | ID      | Claim   |
        | ------- | ------- |
        | `B-001` | First.  |
        | B-002   | Second. |
        """;
}
