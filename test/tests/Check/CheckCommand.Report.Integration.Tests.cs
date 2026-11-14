using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Security.Cryptography;
using System.Text.Json;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Specht.Composition;
using Specht.Evaluation;
using Specht.Manifest;
using Specht.Model;
using Specht.Report;
using Specht.Tests.Shared;
using Specht.Tool;
using Specht.Tool.Features.Check;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Specht.Tests.Check;

/// <summary>
/// The check command given <c>--report &lt;path&gt;</c>, through Spectre's command tester (<c>0001-F3</c> B-003, B-004,
/// B-022 to B-025, B-044 to B-046, C-7; <c>0001-F2</c> B-012; decision 0002; item <c>0036</c>). Over a report built in
/// memory, the file system is a <see cref="MockFileSystem"/> whose current directory is the test's working directory; a
/// test without <c>--root</c> has that directory as the root too, and the B-046 test names another root, so a relative
/// report path resolved from the root lands somewhere the test can see. A run that checks nothing is a run delegate that
/// throws what the engine throws for that input (B-044, B-045). Over the real check and a <see cref="SpechtTree"/> on
/// disk, the report path is under the tree's root.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class CheckCommandReportIntegrationTests
{
    [Theory]
    [ClassData(typeof(ReportPaths))]
    public void AReport_WhenCheckedWithAReportPath_ShouldWriteItsDocumentToThatPath(string because, string path)
    {
        // Given
        SpechtReport report = new SpechtReportFixture().WithViolations(new SpechtViolationFixture());
        var fileSystem = WorkingDirectoryFileSystem();
        var written = fileSystem.Path.GetFullPath(path);
        fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(written)!);

        // When
        var result = Check(fileSystem, _ => report, "--report", path);

        // Then
        fileSystem.File.Exists(written).Should().BeTrue(because + "; stdout: " + result.Output);
        fileSystem.File.ReadAllText(written).Should().Be(Document(report), because);
    }

    [Fact]
    public void AReportPathUnderDirectoriesThatDoNotExist_WhenChecked_ShouldCreateThemAndWriteTheDocument()
    {
        // Given
        SpechtReport report = new SpechtReportFixture().WithViolations(new SpechtViolationFixture());
        var fileSystem = WorkingDirectoryFileSystem();
        var directory = fileSystem.Path.GetFullPath(fileSystem.Path.Combine("reports", "new"));
        fileSystem.Directory.Exists(directory).Should().BeFalse();

        // When
        var result = Check(fileSystem, _ => report, "--report", "reports/new/specht-report.json");

        // Then
        fileSystem.Directory.Exists(directory).Should().BeTrue(result.Output);
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(directory, "specht-report.json")).Should().Be(Document(report));
    }

    [Fact]
    public void AReportPathHoldingAnEarlierRunsDocument_WhenChecked_ShouldHoldOnlyThisRunsDocument()
    {
        // Given
        SpechtReport earlier = new SpechtReportFixture().WithViolations(
            new SpechtViolationFixture(),
            new SpechtViolationFixture().WithFile("b/spec.md"),
            new SpechtViolationFixture().WithFile("c/spec.md"));
        SpechtReport report = new SpechtReportFixture().WithViolations(new SpechtViolationFixture());
        var fileSystem = WorkingDirectoryFileSystem();
        var written = fileSystem.Path.GetFullPath("specht-report.json");
        fileSystem.File.WriteAllText(written, Document(earlier));

        // When
        var result = Check(fileSystem, _ => report, "--report", "specht-report.json");

        // Then
        fileSystem.File.ReadAllText(written).Should().Be(Document(report), result.Output);
    }

    [Fact]
    public void AReport_WhenCheckedWithAReportPathAndWithoutJson_ShouldPrintTheLinesAndSummaryOfARunWithoutIt()
    {
        // Given
        SpechtReport report = new SpechtReportFixture().WithViolations(
            new SpechtViolationFixture().WithFile("b/spec.md").WithLine(9),
            new SpechtViolationFixture().WithSeverity(SpechtSeverity.Warning).WithLine(0));
        var without = Check(WorkingDirectoryFileSystem(), _ => report);

        // When
        var result = Check(WorkingDirectoryFileSystem(), _ => report, "--report", "specht-report.json");

        // Then
        without.Output.Should().Contain(report.Violations[0].ToString());
        result.Output.Should().Be(without.Output);
    }

    [Fact]
    public void AReport_WhenCheckedWithJsonAndAReportPath_ShouldPrintTheDocumentItWrites()
    {
        // Given
        SpechtReport report = new SpechtReportFixture().WithViolations(
            new SpechtViolationFixture().WithIdentifier("B-002"),
            new SpechtViolationFixture().WithSeverity(SpechtSeverity.Warning).WithLine(0));
        var fileSystem = WorkingDirectoryFileSystem();
        var written = fileSystem.Path.GetFullPath("specht-report.json");

        // When
        var result = Check(fileSystem, _ => report, "--json", "--report", "specht-report.json");

        // Then
        fileSystem.File.Exists(written).Should().BeTrue(result.Output);
        fileSystem.File.ReadAllText(written).Should().Be(result.Output);
    }

    [Theory]
    [ClassData(typeof(ReportPathVerdicts))]
    public void AReportOfSeverities_WhenCheckedWithADocumentForm_ShouldExitWithTheCodeOfTheRunWithoutIt(
        SpechtSeverity[] severities,
        string[] args,
        string[] form)
    {
        // Given
        SpechtReport report = new SpechtReportFixture().WithViolations(
            [.. severities.Select(static severity => (SpechtViolation) new SpechtViolationFixture().WithSeverity(severity))]);
        var without = Check(WorkingDirectoryFileSystem(), _ => report, args).ExitCode;

        // When
        var result = Check(WorkingDirectoryFileSystem(), _ => report, [.. args, .. form]);

        // Then
        result.ExitCode.Should().Be(without, result.Output);
    }

    [Theory]
    [ClassData(typeof(ReportPathFaults))]
    public void AReportPathHoldingAnEarlierRunsDocument_WhenACheckExitsWithoutChecking_ShouldHoldNoFile(Exception fault, int expectedExit)
    {
        // Given
        SpechtReport earlier = new SpechtReportFixture().WithViolations(new SpechtViolationFixture());
        var fileSystem = WorkingDirectoryFileSystem();
        var written = fileSystem.Path.GetFullPath("specht-report.json");
        fileSystem.File.WriteAllText(written, Document(earlier));

        // When
        var result = Check(fileSystem, _ => throw fault, "--report", "specht-report.json");

        // Then
        result.ExitCode.Should().Be(expectedExit, result.Output);
        fileSystem.File.Exists(written).Should().BeFalse("a run that exits {0} checked nothing; stdout: {1}", expectedExit, result.Output);
    }

    [Theory]
    [ClassData(typeof(ReportPathFaults))]
    public void AReportPathUnderDirectoriesThatDoNotExist_WhenACheckExitsWithoutChecking_ShouldCreateNoneOfThem(Exception fault, int expectedExit)
    {
        // Given
        var fileSystem = WorkingDirectoryFileSystem();
        var directory = fileSystem.Path.GetFullPath("reports");
        fileSystem.Directory.Exists(directory).Should().BeFalse();

        // When
        var result = Check(fileSystem, _ => throw fault, "--report", "reports/new/specht-report.json");

        // Then
        result.ExitCode.Should().Be(expectedExit, result.Output);
        fileSystem.Directory.Exists(directory).Should().BeFalse("a run that exits {0} checked nothing; stdout: {1}", expectedExit, result.Output);
    }

    [Fact]
    public void ARelativeReportPath_WhenCheckedWithARootOtherThanTheWorkingDirectory_ShouldWriteTheDocumentUnderTheWorkingDirectory()
    {
        // Given
        SpechtReport report = new SpechtReportFixture().WithViolations(new SpechtViolationFixture());
        var fileSystem = WorkingDirectoryFileSystem();
        var root = fileSystem.Path.GetFullPath("root");
        fileSystem.Directory.CreateDirectory(root);
        var written = fileSystem.Path.GetFullPath("specht-report.json");

        // When
        var result = Check(fileSystem, _ => report, "--root", "root", "--report", "specht-report.json");

        // Then
        fileSystem.File.Exists(fileSystem.Path.Combine(root, "specht-report.json")).Should().BeFalse(result.Output);
        fileSystem.File.Exists(written).Should().BeTrue(result.Output);
        fileSystem.File.ReadAllText(written).Should().Be(Document(report));
    }

    [Theory]
    [ClassData(typeof(ReportPathCheckData))]
    public void ACheckWithAReportPath_WhenRun_ShouldWriteOnlyThatFileHoldingOneReportDocument(string input, int expectedExit)
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteFeature("0001", "F1");
        tree.WriteRaw(".hidden/keep.bin", "unchanged hidden content");
        tree.WriteRaw("unrelated.txt", "unchanged unrelated content");
        if (input == "violation")
        {
            tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "nearly" });
        }

        const string reportPath = "reports/specht-report.json";
        var expected = Document(tree.Run());
        var before = Snapshot(tree);

        // When
        var result = Check(
            new FileSystem(),
            static directory => EngineServices.Check().Run(directory, null),
            "--root",
            tree.Root,
            "--report",
            Path.Combine(tree.Root, reportPath));

        // Then
        result.ExitCode.Should().Be(expectedExit, result.Output);
        var after = Snapshot(tree);
        after.Keys.Should().BeEquivalentTo(before.Keys.Append(reportPath), result.Output);
        after.Remove(reportPath);
        after.Should().BeEquivalentTo(before);
        var text = File.ReadAllText(Path.Combine(tree.Root, reportPath));
        text.Invoking(static json => JsonDocument.Parse(json).Dispose()).Should().NotThrow();
        text.Should().Be(expected);
    }

    private static string Document(SpechtReport report) => new SpechtReportSerializer().Serialize(new SpechtReportDocumentMapper().Map(report));

    private static MockFileSystem WorkingDirectoryFileSystem()
    {
        var workingDirectory = Directory.GetCurrentDirectory();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), workingDirectory);
        fileSystem.Directory.CreateDirectory(workingDirectory);

        return fileSystem;
    }

    private static Dictionary<string, string> Snapshot(SpechtTree tree) =>
        Directory.EnumerateFiles(tree.Root, "*", new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = 0 })
            .ToDictionary(file => Path.GetRelativePath(tree.Root, file).Replace(Path.DirectorySeparatorChar, '/'),
                static file => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))), StringComparer.Ordinal);

    private static CommandAppResult Check(IFileSystem fileSystem, Func<string, SpechtReport> run, params string[] args)
    {
        var services = new ServiceCollection();
        services.AddSingleton(fileSystem);
        services.AddSpechtEngine();
        services.AddSingleton<Func<string, SchemaSourceSelection?, SpechtReport>>((root, _) => run(root));

        var app = new CommandAppTester(new TypeRegistrar(services), console: new TestConsole().Width(80));
        app.SetDefaultCommand<CheckCommand>();

        return app.Run(args);
    }
}
