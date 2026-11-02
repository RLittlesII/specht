using AwesomeAssertions;
using Specht.Model;

namespace Specht.Tests.Model;

/// <summary>
/// The frontmatter parsed from a document's text (ADR-0001, item 0104; item 0166): each key's value and one-based line,
/// how a scalar is typed, and no frontmatter where none can be read. The reader holds no file system.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class FrontmatterReaderUnitTests
{
    /// <summary>Gets a scalar as written in YAML, with the JSON it is read as.</summary>
    public static TheoryData<string, string, string> Scalars { get; } = new()
    {
        { "a quoted date", "\"2026-09-29\"", "\"2026-09-29\"" },
        { "an unquoted date", "2026-08-27", "\"2026-08-27\"" },
        { "a quoted zero-padded id", "\"0001\"", "\"0001\"" },
        { "an unquoted zero-padded id", "0001", "\"0001\"" },
        { "a single-quoted boolean", "'true'", "\"true\"" },
        { "true", "true", "true" },
        { "false", "false", "false" },
        { "an integer", "42", "42" },
        { "zero", "0", "0" },
        { "null", "null", "null" },
        { "a tilde", "~", "null" },
        { "nothing", string.Empty, "null" },
        { "a word", "draft", "\"draft\"" },
        { "a flow sequence", "[0001, 2]", "[\"0001\",2]" },
    };

    /// <summary>Gets documents with no frontmatter that can be read.</summary>
    public static TheoryData<string, string> Unreadable { get; } = new()
    {
        { "no frontmatter", "# Sample\n\nProse.\n" },
        { "an empty document", string.Empty },
        { "an opening delimiter that is never closed", "---\nepic: \"0009\"\n\n# Sample\n" },
        { "malformed YAML", "---\nepic: [\"0009\"\n---\n" },
        { "a sequence where a mapping belongs", "---\n- epic\n- id\n---\n" },
    };

    [Fact]
    public void ADocumentWithFrontmatter_WhenRead_ShouldCarryEachKeysValueAndItsOneBasedLine()
    {
        // Given
        const string text = """
            ---
            epic: "0009"
            id: F1
            depends_on:
              - F2
              - F3
            spec_status: draft
            ---

            # Sample
            """;

        // When
        var frontmatter = new FrontmatterReader().Parse(text);

        // Then
        frontmatter.StartLine.Should().Be(1);
        frontmatter.Text("epic").Should().Be("0009");
        frontmatter.Text("id").Should().Be("F1");
        frontmatter.Text("spec_status").Should().Be("draft");
        frontmatter.Node!["depends_on"]!.ToJsonString().Should().Be("[\"F2\",\"F3\"]");
        frontmatter.KeyLines.Should().BeEquivalentTo(
            new Dictionary<string, int> { ["epic"] = 2, ["id"] = 3, ["depends_on"] = 4, ["spec_status"] = 7 });
    }

    [Theory]
    [MemberData(nameof(Scalars))]
    public void AScalar_WhenRead_ShouldStayAStringUnlessItIsUnquotedAndUnambiguouslyNullBooleanOrInteger(
        string because,
        string yaml,
        string json)
    {
        // Given
        var text = $"---\nvalue: {yaml}\n---\n";

        // When
        var frontmatter = new FrontmatterReader().Parse(text);

        // Then
        frontmatter.Node.Should().NotBeNull().And.ContainKey("value");
        (frontmatter.Node!["value"]?.ToJsonString() ?? "null").Should().Be(json, because);
    }

    [Theory]
    [MemberData(nameof(Unreadable))]
    public void ADocumentWithNoReadableFrontmatter_WhenRead_ShouldGiveNoFrontmatterAndNoKeyLines(string because, string text)
    {
        // Given
        var reader = new FrontmatterReader();

        // When
        var frontmatter = reader.Parse(text);

        // Then
        frontmatter.Node.Should().BeNull(because);
        frontmatter.StartLine.Should().Be(1, because);
        frontmatter.KeyLines.Should().BeEmpty(because);
    }
}
