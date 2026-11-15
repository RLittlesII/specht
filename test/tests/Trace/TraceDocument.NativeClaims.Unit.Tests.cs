using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules.Matrix;
using Specht.Tests.Shared;
using Specht.Trace;

namespace Specht.Tests.Trace;

/// <summary>
/// The trace projected from a model the reader builds over an in-memory tree under a schema 0.4.0 manifest whose
/// <c>claim.source</c> is <c>gherkin</c> (<c>0001-F16</c> B-024; <c>0001-F13</c> B-001; § 7, item <c>0396</c>): a
/// claim file set with no record lists each claim as <c>none</c> with the count of the scenarios that carry it, at the
/// path of its first claim file, and a set with a record whose matrix holds a row for a claim lists the row's Status at
/// the record's path.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class TraceDocumentNativeClaimsUnitTests
{
    [Fact]
    public void AClaimFileSetWithNoRecord_WhenTheTraceIsMade_ShouldListEachClaimAsNoneWithItsScenarioCountAtTheFirstClaimFile()
    {
        // Given
        var model = Read(string.Empty, [(AddFile, TwoClaims)]);

        // When
        var lines = TraceDocument.From(model, new MatrixResolver()).Lines();

        // Then
        lines.Select(static line => string.Join('\t', line.Split('\t').Take(4)))
            .Should().Equal($"{AddFile}\tB-001\tnone\t2", $"{AddFile}\tB-002\tnone\t1");
    }

    [Fact]
    public void AClaimFileSetWhoseRecordHoldsAMatrix_WhenTheTraceIsMade_ShouldListEachClaimWithItsRowsStatusAtTheRecord()
    {
        // Given
        var record = "---\nid: F1\n---\n\n# Arithmetic\n\n"
            + string.Join(
                '\n',
                SpechtTree.SectionsWith(
                    "9. Traceability Matrix",
                    "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n"
                    + "| B-001 | Adds | Missing | Covered |\n| B-002 | Adds | Missing | Partial |\n"));
        var model = Read(", \"record\": \"" + RecordFile + "\"", [(AddFile, TwoClaims), (RecordFile, record)]);

        // When
        var lines = TraceDocument.From(model, new MatrixResolver()).Lines();

        // Then
        lines.Select(static line => string.Join('\t', line.Split('\t').Take(4)))
            .Should().Equal($"{RecordFile}\tB-001\tCovered\t2", $"{RecordFile}\tB-002\tPartial\t1");
    }

    private static SpechtModel Read(string record, (string Path, string Text)[] files)
    {
        var manifest = "{ \"schemaVersion\": \"0.4.0\", \"exclusions\": [\"obj\"], \"claim\": { \"source\": \"gherkin\", \"files\": ["
            + "{ \"glob\": \".spec/features/arithmetic/*.feature\", \"feature\": \"F1\"" + record + " } ] } }";
        var fileSystem = new MockFileSystem(
            files.Append((Path: SpechtTree.ManifestPath, Text: manifest))
                .ToDictionary(
                    static file => Path.Combine(["repo", .. file.Path.Split('/')]),
                    static file => new MockFileData(file.Text)));

        return EngineServices.Reader(fileSystem).Read(fileSystem.Path.GetFullPath("repo"), null);
    }

    private const string AddFile = ".spec/features/arithmetic/add.feature";

    private const string RecordFile = ".spec/features/arithmetic/README.md";

    private const string TwoClaims = "@B-001\nFeature: Arithmetic\n\n  @B-002\n  Scenario: Adds\n    Given two numbers\n\n"
        + "  Scenario: Adds three\n    Given three numbers\n";
}
