using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// The report store over an in-memory file system, the write beneath <c>--report</c> (<c>0001-F3</c> B-022, B-023, B-024;
/// decision 0002; item <c>0036</c>): the text it is given is the whole content of the file at the path it is given, a bare
/// file name included, the directories above that path are created when they do not exist, and a file already there is
/// replaced.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtReportStoreUnitTests
{
    [Theory]
    [ClassData(typeof(ReportPaths))]
    public void TheReportText_WhenSavedToAPathInADirectoryThatExists_ShouldBeThatFilesWholeContent(string because, string path)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(fileSystem.Path.GetFullPath(path))!);

        // When
        new SpechtReportStore(fileSystem).Save(Text, path);

        // Then
        fileSystem.File.Exists(path).Should().BeTrue(because);
        fileSystem.File.ReadAllText(path).Should().Be(Text, because);
    }

    [Fact]
    public void APathUnderDirectoriesThatDoNotExist_WhenSaved_ShouldCreateThemAndWriteTheText()
    {
        // Given
        var fileSystem = new MockFileSystem();
        var path = fileSystem.Path.Combine("reports", "new", "specht-report.json");

        // When
        new SpechtReportStore(fileSystem).Save(Text, path);

        // Then
        fileSystem.Directory.Exists(fileSystem.Path.Combine("reports", "new")).Should().BeTrue();
        fileSystem.File.ReadAllText(path).Should().Be(Text);
    }

    [Fact]
    public void APathHoldingALongerFile_WhenSaved_ShouldHoldOnlyTheNewText()
    {
        // Given
        var path = Path.Combine("reports", "specht-report.json");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { [path] = new(Text + Text + Text) });

        // When
        new SpechtReportStore(fileSystem).Save(Text, path);

        // Then
        fileSystem.File.ReadAllText(path).Should().Be(Text);
    }

    private const string Text = "{\n  \"schemaVersion\": \"0.1.0\"\n}";
}
