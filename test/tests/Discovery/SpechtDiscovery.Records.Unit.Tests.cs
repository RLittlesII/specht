using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Tests.Shared;

namespace Specht.Tests.Discovery;

/// <summary>
/// The records beside one specification, listed over an injected in-memory file system with no git (<c>0132-F2</c> B-011;
/// item <c>0368</c>): every Markdown file directly in the named folder beside the specification file, in either layout,
/// by its root-relative path in ordinal order of file name, and no file below that folder, of another extension or in
/// another of the three folders.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtDiscoveryRecordsUnitTests
{
    [Theory]
    [ClassData(typeof(RecordLayouts))]
    public void FilesInAndBelowTheFoldersBesideASpecification_WhenItsRecordsAreFound_ShouldListOnlyTheMarkdownFilesDirectlyInTheNamedFolderInOrdinalOrder(
        string relativePath,
        string directory)
    {
        // Given
        SpechtLocation specification = new SpechtLocationFixture().WithRelativePath(relativePath).WithDirectory(directory);
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine([Root, .. relativePath.Split('/')])] = new(string.Empty),
        });
        foreach (var name in new[]
                 {
                     "adr/k3m9xq2a-named-by-a-nano-id.md",
                     "adr/0002-second.md",
                     "adr/Zeta.md",
                     "adr/0001-first.md",
                     "adr/archive/0000-archived.md",
                     "adr/diagram.svg",
                     "adr/.gitkeep",
                     "decisions/0001-a-decision.md",
                     "0001-01-an-item.md",
                 })
        {
            fileSystem.AddFile(Path.Combine([Root, .. directory.Split('/'), .. name.Split('/')]), new MockFileData(string.Empty));
        }

        var discovery = new SpechtDiscovery(fileSystem, new UnexpectedChildProcess());

        // When
        var adr = discovery.FindRecords(Root, specification, "adr");
        var lessons = discovery.FindRecords(Root, specification, "lessons");

        // Then
        adr.Should().Equal(
            $"{directory}/adr/0001-first.md",
            $"{directory}/adr/0002-second.md",
            $"{directory}/adr/Zeta.md",
            $"{directory}/adr/k3m9xq2a-named-by-a-nano-id.md");
        lessons.Should().BeEmpty();
    }

    private const string Root = "repo";
}
