using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>
/// The records read beside each Feature the model reader read, over a synthetic tree held in an injected file system
/// (<c>0132-F2</c> B-011, B-022; item <c>0368</c>): one list per Feature, in either layout, its ADRs, then its decisions,
/// then its lessons, each kind from its folder and each record's text read, and an empty list for a Feature with none.
/// Nothing touches the disk; the root is the file system's own full path of <c>repo</c>, as the reader's other tests have it.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtModelReaderRecordsUnitTests
{
    [Fact]
    public void FeaturesInBothLayoutsWithRecordsInEachFolderAndOneWithNone_WhenTheirRecordsAreRead_ShouldGiveEachFeaturesRecordsByKindInOrdinalOrderOfPath()
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpechtTree.ManifestPath)] = new("{}"),
            [Path.Combine("repo", "src", "area", ".spec", "README.md")] = new("---\nepic: \"0001\"\nid: F1\n---\n"),
            [Path.Combine("repo", "src", "area", ".spec", "lessons", "0001-a-lesson.md")] = new("---\ntitle: Lesson 0001\n---\n"),
            [Path.Combine("repo", "src", "area", ".spec", "decisions", "0002-later.md")] = new("---\ntitle: Decision 0002\n---\n"),
            [Path.Combine("repo", "src", "area", ".spec", "decisions", "0001-first.md")] = new("---\ntitle: Decision 0001\n---\n"),
            [Path.Combine("repo", "src", "area", ".spec", "adr", "0001-an-adr.md")] = new("---\ntitle: ADR-0001\n---\n\n## Status\n\nAccepted.\n"),
            [Path.Combine("repo", "src", "plain", ".spec", "README.md")] = new("---\nepic: \"0001\"\nid: F3\n---\n"),
            [Path.Combine("repo", "epics", "0001-epic", "F2-feature", "spec.md")] = new("---\nepic: \"0001\"\nid: F2\n---\n"),
            [Path.Combine("repo", "epics", "0001-epic", "F2-feature", "adr", "0001-a-legacy-adr.md")] = new("---\ntitle: ADR-0001 legacy\n---\n"),
        });
        var root = fileSystem.Path.GetFullPath("repo");
        var reader = EngineServices.Reader(fileSystem);
        var features = reader.Read(root, null).Features;
        features.Select(static feature => feature.RelativePath)
            .Should().Equal("epics/0001-epic/F2-feature/spec.md", "src/area/.spec/README.md", "src/plain/.spec/README.md");

        // When
        var records = reader.ReadRecords(root, features);

        // Then
        features.Select(feature => records[feature]
                .Select(static record => $"{record.Kind} {record.RelativePath} {record.Frontmatter.Text("title")} {record.StatusLine}")
                .ToArray())
            .Should().BeEquivalentTo(
                new[]
                {
                    new[] { "adr epics/0001-epic/F2-feature/adr/0001-a-legacy-adr.md ADR-0001 legacy " },
                    new[]
                    {
                        "adr src/area/.spec/adr/0001-an-adr.md ADR-0001 Accepted.",
                        "decision src/area/.spec/decisions/0001-first.md Decision 0001 ",
                        "decision src/area/.spec/decisions/0002-later.md Decision 0002 ",
                        "lesson src/area/.spec/lessons/0001-a-lesson.md Lesson 0001 ",
                    },
                    Array.Empty<string>(),
                },
                static options => options.WithStrictOrdering());
    }
}
