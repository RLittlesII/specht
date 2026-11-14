using AwesomeAssertions;
using Specht.Discovery;
using Specht.Model;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Model;

/// <summary>
/// The model builder over a listing and the text of each listed file, with no file system (item 0166; <c>0001-F1</c>
/// B-004; ADR-0010 § "I/O is confined"): each specification, companion, item, epic and test file is parsed from its text
/// and carried in the order the listing gives, and an item's directory is the root-relative directory of its path. A record
/// beside a specification is built from its text with its path, its kind, its frontmatter and the first line under its
/// <c>## Status</c> heading, a frontmatter it cannot read included (<c>0132-F2</c> B-011; item <c>0368</c>).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtModelBuilderUnitTests
{
    [Fact]
    public void AListingAndTheTextOfEachListedFile_WhenBuilt_ShouldCarryEveryFileParsedFromItsTextInTheListingsOrder()
    {
        // Given
        var listing = new SpechtListing(
            [
                new SpechtLocationFixture().WithRelativePath("src/zeta/.spec/README.md").WithDirectory("src/zeta/.spec"),
                new SpechtLocationFixture().WithRelativePath("src/alpha/.spec/README.md").WithDirectory("src/alpha/.spec"),
            ],
            [["src/zeta/.spec/zeta.feature"], []],
            ["src/zeta/.spec/0001-02-do.md", "src/alpha/.spec/0001-01-do.md"],
            ["epics/0002-later/epic.md", "epics/0001-first/epic.md"],
            ["test/Zeta.cs", "test/Alpha.cs"]);
        var texts = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["src/zeta/.spec/README.md"] = "---\nepic: '0001'\nid: F2\n---\n",
            ["src/alpha/.spec/README.md"] = "---\nepic: '0001'\nid: F1\n---\n",
            ["src/zeta/.spec/zeta.feature"] = "Feature: zeta\n",
            ["src/zeta/.spec/0001-02-do.md"] = "---\nid: '0001-02'\n---\n",
            ["src/alpha/.spec/0001-01-do.md"] = "---\nid: '0001-01'\n---\n",
            ["epics/0002-later/epic.md"] = "---\nid: '0002'\n---\n",
            ["epics/0001-first/epic.md"] = "---\nid: '0001'\n---\n",
            ["test/Zeta.cs"] = "class Zeta;\n",
            ["test/Alpha.cs"] = "class Alpha;\n",
        };

        // When
        var model = Builder().Build(Schemas(), listing, texts);

        // Then
        model.Features.Select(static feature => (feature.RelativePath, feature.Identity))
            .Should().Equal(("src/zeta/.spec/README.md", "0001-F2"), ("src/alpha/.spec/README.md", "0001-F1"));
        model.Features.Select(static feature => feature.FeatureFiles.Select(static file => file.RelativePath).ToArray())
            .Should().BeEquivalentTo([new[] { "src/zeta/.spec/zeta.feature" }, []], static options => options.WithStrictOrdering());
        model.Items.Select(static item => (item.RelativePath, item.Id))
            .Should().Equal(("src/zeta/.spec/0001-02-do.md", "0001-02"), ("src/alpha/.spec/0001-01-do.md", "0001-01"));
        model.Epics.Select(static epic => (epic.RelativePath, epic.Id))
            .Should().Equal(("epics/0002-later/epic.md", "0002"), ("epics/0001-first/epic.md", "0001"));
        model.TestFiles.Select(static file => (file.RelativePath, file.Text))
            .Should().Equal(("test/Zeta.cs", "class Zeta;\n"), ("test/Alpha.cs", "class Alpha;\n"));
    }

    [Fact]
    public void AnItemAtTheRootAndAnItemInAFolder_WhenBuilt_ShouldCarryEachOnesFileNameAndRootRelativeDirectory()
    {
        // Given
        var listing = new SpechtListing([], [], ["0001-01-top.md", "src/area/.spec/0001-02-do.md"], [], []);
        var texts = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["0001-01-top.md"] = "---\nid: '0001-01'\n---\n",
            ["src/area/.spec/0001-02-do.md"] = "---\nid: '0001-02'\n---\n",
        };

        // When
        var model = Builder().Build(Schemas(), listing, texts);

        // Then
        model.Items.Select(static item => (item.FileName, item.ParentDirectory))
            .Should().Equal(("0001-01-top.md", string.Empty), ("0001-02-do.md", "src/area/.spec"));
    }

    [Fact]
    public void ARecordWhoseFrontmatterCannotBeRead_WhenBuilt_ShouldCarryItsPathKindNoTitleAndTheFirstLineUnderItsStatusHeading()
    {
        // Given
        const string path = "src/area/.spec/adr/0001-a-broken-frontmatter.md";
        const string text = "---\ntitle: [unclosed\n---\n\n# ADR-0001\n\n## Status\n\nAccepted, 2026-10-12, by the owner.\n";

        // When
        var record = Builder().Record(path, "adr", text);

        // Then
        record.RelativePath.Should().Be(path);
        record.Kind.Should().Be("adr");
        record.Frontmatter.Text("title").Should().BeNull();
        record.StatusLine.Should().Be("Accepted, 2026-10-12, by the owner.");
    }

    private static SpechtModelBuilder Builder() =>
        new(new SpechtDocumentParser(new FrontmatterReader()), new FrontmatterReader(), new FeatureTagParser());

    private static SpechtSchemas Schemas() =>
        new SpechtSchemaCompiler().Compile(
            EngineServices.Versions().Find(new SemanticVersion(0, 1, 0))!, EngineServices.Parser().Parse("{}"), SpechtSchemaSource.Embedded);
}
