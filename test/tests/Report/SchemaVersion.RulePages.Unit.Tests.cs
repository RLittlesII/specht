using System.Text.RegularExpressions;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Specht.Tests.Rules;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Report;

/// <summary>
/// The rule pages a shipped schema version carries (<c>0001-F3</c> B-031, B-032, B-033; C-8; decision 0003), read through
/// <see cref="SchemaVersion.RulePages"/>: the pages name exactly the version's vocabulary, the engine embeds no page a
/// version does not give, each page follows the one page shape and no other, its example violation shows the line the
/// tool prints for that rule, its metadata gives the version it ships under, and it carries no absolute path. Every
/// theory is driven from the embedded vocabulary; no rule id is written here.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SchemaVersionRulePagesUnitTests
{
    /// <summary>Gets the number of every schema version the engine embeds, as <c>major.minor.patch</c>.</summary>
    public static TheoryData<string> Shipped => [.. SchemaVersions.Embedded.Versions.Select(static version => version.Number.ToString())];

    /// <summary>Gets every shipped version's number with each rule id in its vocabulary.</summary>
    public static TheoryData<string, string> Rules
    {
        get
        {
            var rules = new TheoryData<string, string>();

            foreach (var version in SchemaVersions.Embedded.Versions)
            {
                foreach (var id in version.RuleIds.Order(StringComparer.Ordinal))
                {
                    rules.Add(version.Number.ToString(), id);
                }
            }

            return rules;
        }
    }

    [Theory]
    [MemberData(nameof(Shipped))]
    public void AShippedVersion_WhenItsRulePagesAreGathered_ShouldNameExactlyItsVocabularyOnePageEach(string number)
    {
        // Given
        var version = Version(number);

        // When
        var pages = version.RulePages.Keys;

        // Then
        pages.Should().BeEquivalentTo(version.RuleIds);
    }

    [Fact]
    public void TheEngine_WhenItsEmbeddedMarkdownIsCounted_ShouldEmbedNoPageAShippedVersionDoesNotGive()
    {
        // Given
        var given = SchemaVersions.Embedded.Versions.Sum(static version => version.RulePages.Count);

        // When
        var embedded = typeof(SchemaVersions).Assembly.GetManifestResourceNames()
            .Count(static name => name.EndsWith(".md", StringComparison.OrdinalIgnoreCase));

        // Then
        embedded.Should().Be(given);
    }

    [Theory]
    [MemberData(nameof(Rules))]
    public void ARulePage_WhenRead_ShouldFollowTheOnePageShapeInOrderWithNoOtherSection(string number, string id)
    {
        // Given
        var version = Version(number);
        var text = Page(version, id);

        // When
        var page = RulePage.Read(EmbeddedFolder.Of(version), id, text);

        // Then
        using var scope = new AssertionScope();
        page.Frontmatter.Text("title").Should().StartWith($"{id}: ").And.NotBe($"{id}: ");
        page.Frontmatter.Text("description").Should().NotBeNullOrWhiteSpace();
        page.Frontmatter.Text("type").Should().Be("rule");
        page.Outline.Should().Equal([$"# {page.Frontmatter.Text("title")}", .. RulePage.Shape]);
        page.Metadata?.Headers.Should().Equal("Property", "Value");
        page.Metadata?.Rows.Select(static row => row[0]).Should().Equal("Rule ID", "Family", "Default severity", "Schema version");
        page.Value("Rule ID").Should().Be(id);
        page.Value("Family").Should().NotBeNullOrWhiteSpace();
        page.Value("Default severity").Should().NotBeNullOrWhiteSpace();
        page.Body("## Cause").Should().NotBeEmpty();
        page.Body("## Rule description").Should().NotBeEmpty();
        page.Body("### Example violation").Should().NotBeEmpty();
        page.Body("### Corrected").Should().NotBeEmpty();
        page.Body("## How to fix violations").Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Rules))]
    public void ARulePage_WhenItsExampleViolationIsRead_ShouldShowTheLineTheToolPrintsForThatRule(string number, string id)
    {
        // Given
        var page = Read(number, id);

        // When
        var lines = page.ToolLineFiles();

        // Then
        lines.Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Rules))]
    public void ARulePage_WhenItsMetadataIsRead_ShouldGiveTheSchemaVersionItShipsUnder(string number, string id)
    {
        // Given
        var page = Read(number, id);

        // When
        var version = page.Value("Schema version");

        // Then
        version.Should().Be(page.VersionText());
    }

    [Theory]
    [MemberData(nameof(Rules))]
    public void ARulePage_WhenRead_ShouldCarryNoAbsolutePath(string number, string id)
    {
        // Given
        var page = Read(number, id);

        // When
        var absolute = Regex.Matches(page.Text, """(?<=^|[\s`"'(|])(?:/[A-Za-z]|[A-Za-z]:[\\/]|~/)\S*""", RegexOptions.Multiline)
            .Select(static match => match.Value)
            .ToList();

        // Then
        absolute.Should().BeEmpty();
    }

    private static SchemaVersion Version(string number) =>
        SchemaVersions.Embedded.Versions.Single(version => string.Equals(version.Number.ToString(), number, StringComparison.Ordinal));

    private static RulePage Read(string number, string id)
    {
        var version = Version(number);

        return RulePage.Read(EmbeddedFolder.Of(version), id, Page(version, id));
    }

    private static string Page(SchemaVersion version, string id)
    {
        version.RulePages.Should().ContainKey(id, "version {0} ships a page for every rule id in its vocabulary (B-031)", version.Number);

        return version.RulePages[id];
    }
}
