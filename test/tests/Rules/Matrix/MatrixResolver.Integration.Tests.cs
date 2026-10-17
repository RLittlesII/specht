using System.IO.Abstractions;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Model;
using Specht.Rules.Matrix;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules.Matrix;

/// <summary>The same resolver reads declared real-file evidence and parsed rows without a check or a test process.</summary>
[Trait("Tier", "Integration")]
public sealed class MatrixResolverIntegrationTests
{
    [Fact]
    public void AParsedMatrixAndDeclaredFiles_WhenResolved_ShouldRetainColumnRolesNamesAndDeclarationEvidence()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteRaw("test/sample.fs", "module Suite\nlet Alpha \nlet Beta ");
        tree.WriteRaw("test/obj/excluded.fs", "let Missing ");
        var path = tree.WriteRaw("src/sample/.spec/README.md", """
            ## 9. Traceability Matrix
            | Requirement | Description | Evidence | State |
            | --- | --- | --- | --- |
            | B-001 | Names | `Suite.Alpha; Suite.Beta`, prose | Partial |
            | B-002 | Boundary | `Missing` | Missing |
            """);
        var structure = EngineServices.Parser().Parse("""
            {"schemaVersion":"0.2.0",
             "tables":{"matrix":["Requirement","Description","Evidence","State"]},"exclusions":["obj"],
             "tests":{"files":["test/**/*.fs","test/sample.fs"],"declarations":["let {test} "]}}
            """);
        var fileSystem = new FileSystem();
        var document = SpecDocument.Parse(fileSystem.File.ReadAllText(path), "src/sample/.spec/README.md");
        var files = new SpecDiscovery(fileSystem).ReadTestFiles(tree.Root, structure.Tests!.Files, structure.Discovery.Exclusions);

        // When
        var rows = new MatrixResolver().Resolve(document, structure, files);

        // Then
        files.Should().ContainSingle();
        rows.Select(static row => row.Line).Should().Equal(4, 5);
        rows[0].Claim.Should().Be("B-001");
        rows[0].Tests.Select(static test => test.Name).Should().Equal("Suite.Alpha", "Suite.Beta");
        rows[0].Tests.Should().OnlyContain(static test => test.Status == MatrixTestStatus.Resolved && test.File == "test/sample.fs");
        rows[0].Tests.Select(static test => test.Line).Should().Equal(2, 3);
        rows[1].Tests.Single().Status.Should().Be(MatrixTestStatus.Unresolved);
    }

    [Fact]
    public void QualifiersSplitAcrossDeclaredFiles_WhenResolved_ShouldLeaveTheDottedNameUnresolved()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteRaw("test/steps.cs", "class Steps");
        tree.WriteRaw("test/alpha.cs", "void Alpha()");
        var document = SpecDocument.Parse("""
            ## 9. Traceability Matrix
            | Claim ID | Scenario | Test | Status |
            | --- | --- | --- | --- |
            | B-004 | Matching | `Steps.Alpha` | Covered |
            """, "src/sample/.spec/README.md");
        var structure = EngineServices.Parser().Parse("""{"schemaVersion":"0.2.0","tests":{"files":["test/**/*.cs"]}}""");
        var files = new SpecDiscovery(new FileSystem()).ReadTestFiles(tree.Root, structure.Tests!.Files, structure.Discovery.Exclusions);

        // When
        var test = new MatrixResolver().Resolve(document, structure, files).Single().Tests.Single();

        // Then
        test.Status.Should().Be(MatrixTestStatus.Unresolved);
        test.File.Should().BeNull();
        test.Line.Should().BeNull();
    }
}
