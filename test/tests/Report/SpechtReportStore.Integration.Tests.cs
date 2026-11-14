using System.IO.Abstractions;
using AwesomeAssertions;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// The report document a check makes, written to disk through the report store (<c>0001-F3</c> B-002; item <c>0167</c>):
/// the written file carries root-relative paths only.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtReportStoreIntegrationTests
{
    [Fact]
    public void AReportWrittenToDisk_WhenRead_ShouldCarryRelativePathsOnly()
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "nearly" });
        var report = tree.Run();
        var path = Path.Combine(tree.Root, ".artifacts", "spec-check", "spec-check.json");

        // When
        new SpechtReportStore(new FileSystem()).Save(new SpechtReportSerializer().Serialize(new SpechtReportDocumentMapper().Map(report)), path);
        var json = File.ReadAllText(path);

        // Then
        json.Should().NotContain(tree.Root);
        json.Should().Contain("epics/0001-epic/F1-feature/spec.md");
        json.Should().Contain("SPEC1002");
    }
}
