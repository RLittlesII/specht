using AwesomeAssertions;

namespace specht.tests;

/// <summary>
/// A specification parsed from text held in memory (ADR-0001 stage A, item 0104): the frontmatter, the sections, the
/// table each section opens with, and the one-based line of each, with no file behind it.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecDocumentUnitTests
{
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
    [InlineData("1. Business Goal", 8)]
    [InlineData("3. Acceptance Criteria", 12)]
    public void ASpecificationHeldAsText_WhenParsed_ShouldCarryTheSectionAtItsHeadingsOneBasedLine(string title, int line)
    {
        // Given
        var text = Specification;

        // When
        var document = SpecDocument.Parse(text, RelativePath);

        // Then
        document.Section(title).Should().NotBeNull().And.Subject.As<SpecSection>().Line.Should().Be(line);
    }

    [Fact]
    public void ASectionOpeningWithAPipeTable_WhenParsed_ShouldCarryItsHeadersRowsAndOneBasedRowLines()
    {
        // Given
        var text = Specification;

        // When
        var section = SpecDocument.Parse(text, RelativePath).Section("3. Acceptance Criteria")!;

        // Then
        section.Headers.Should().Equal("ID", "Claim");
        section.Rows.Should().HaveCount(2);
        section.Rows[0].Should().Equal("B-001", "First.");
        section.Rows[1].Should().Equal("B-002", "Second.");
        section.RowLines.Should().Equal(16, 17);
    }

    [Fact]
    public void ASectionWithNoTable_WhenParsed_ShouldCarryNoHeadersAndNoRows()
    {
        // Given
        var text = Specification;

        // When
        var section = SpecDocument.Parse(text, RelativePath).Section("1. Business Goal")!;

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
