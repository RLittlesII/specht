using AwesomeAssertions;
using Specht.Model;

namespace Specht.Tests.Model;

/// <summary>
/// A specification parsed from text held in memory by the instance parser (ADR-0001 stage A, item 0104; item 0166): the frontmatter, the sections, the
/// table each section opens with, and the one-based line of each, with no file behind it. A grid table is read as no table
/// (<c>0001-F1</c> B-008). The first cell of every table row, under a <c>##</c> section or a subsection, is read for the guard
/// (<c>0001-F19</c> B-041, item <c>0418</c>). The first line under a record's <c>## Status</c> heading is read for the facts
/// (<c>0132-F2</c> B-011, OQ-3; item <c>0368</c>). Each list item among a section's own blocks is read with its text and
/// first line, beside the table, whose cells do not change (<c>0132-F2</c> B-009, C-1; item <c>0367</c>).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtDocumentParserUnitTests
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
        var document = new SpechtDocumentParser(new FrontmatterReader()).Parse(text, RelativePath);

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
        var document = new SpechtDocumentParser(new FrontmatterReader()).Parse(text, RelativePath);

        // Then
        document.Section(title).Should().NotBeNull().And.Subject.As<SpechtSection>().Line.Should().Be(line);
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
        var section = new SpechtDocumentParser(new FrontmatterReader()).Parse(text, RelativePath).Section(title)!;

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
        var section = new SpechtDocumentParser(new FrontmatterReader()).Parse(text, RelativePath).Section("9. Traceability Matrix")!;

        // Then
        section.Headers.Should().BeEmpty();
        section.Rows.Should().BeEmpty();
        section.RowLines.Should().BeEmpty();
    }

    [Fact]
    public void ASpecificationWithTablesUnderItsSectionsAndASubsection_WhenItsFirstCellsAreRead_ShouldGiveTheFirstCellOfEachTableRow()
    {
        // Given
        const string text = """
            # Specification: Sample

            ## 3. Acceptance Criteria

            | ID    | Claim   |
            | ----- | ------- |
            | B-001 | First.  |
            | B-002 | Second. |

            ## 4. Constraints

            | ID  | Constraint |
            | --- | ---------- |
            | C-1 | Fast.      |

            ### Later

            | ID   | Question |
            | ---- | -------- |
            | OQ-1 | Which?   |
            """;

        // When
        var cells = new SpechtDocumentParser(new FrontmatterReader()).FirstCells(text);

        // Then
        cells.Should().Contain(["B-001", "B-002", "C-1", "OQ-1"]);
    }

    [Theory]
    [ClassData(typeof(SectionListItems))]
    public void ASectionsOwnLists_WhenParsed_ShouldCarryEachItemsTextAndOneBasedFirstLine(string text, string[] items, int[] lines)
    {
        // Given
        var parser = new SpechtDocumentParser(new FrontmatterReader());

        // When
        var section = parser.Parse(text, RelativePath).Section("10. Lessons / Spec Deltas")!;

        // Then
        section.Items.Should().Equal(items);
        section.ItemLines.Should().Equal(lines);
    }

    [Fact]
    public void ASectionWithAListBeforeItsTable_WhenParsed_ShouldCarryTheTablesHeadersRowsAndRowLinesBesideTheItems()
    {
        // Given
        const string text =
            """
            ## 10. Lessons / Spec Deltas

            - A lesson.

            | Trigger        | Change                         | Affected IDs |
            | -------------- | ------------------------------ | ------------ |
            | Review round 1 | `B-001` narrowed in [§ 3](#3)  | B-001        |
            """;

        // When
        var section = new SpechtDocumentParser(new FrontmatterReader()).Parse(text, RelativePath).Section("10. Lessons / Spec Deltas")!;

        // Then
        section.Headers.Should().Equal("Trigger", "Change", "Affected IDs");
        section.Rows.Should().BeEquivalentTo(
            new[] { new[] { "Review round 1", "B-001 narrowed in § 3", "B-001" } },
            static options => options.WithStrictOrdering());
        section.RowLines.Should().Equal(7);
        section.Items.Should().Equal("A lesson.");
        section.ItemLines.Should().Equal(3);
    }

    [Theory]
    [ClassData(typeof(StatusSectionTexts))]
    public void ARecordsText_WhenTheFirstLineUnderItsStatusHeadingIsRead_ShouldGiveTheFirstParagraphsFirstLineFlattenedOrNone(string text, string? line)
    {
        // Given
        var parser = new SpechtDocumentParser(new FrontmatterReader());

        // When
        var read = parser.FirstLine(text, "Status");

        // Then
        read.Should().Be(line);
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
