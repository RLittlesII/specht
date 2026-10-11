using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// Every gate is a target here (0055-F1 C-1); a target writes only under .artifacts/, bin/ and obj/ (C-5).
internal partial class SpechtBuild : NukeBuild
{
    // B-001: `./build.sh` with no target name runs Compile, then Test.
    public static int Main() => Execute<SpechtBuild>(static x => x.Build);

    private static string NpxPath => ToolPathResolver.GetPathExecutable("npx");

    private AbsolutePath ArtifactsDirectory => RootDirectory / ".artifacts";

    private AbsolutePath PackageDirectory => ArtifactsDirectory / "nupkg";

    private AbsolutePath CoverageDirectory => ArtifactsDirectory / "coverage";

    // B-019, C-3: the Markdown formatter runs at the exact version package.json pins, never a version fetched as latest.
    private string PrettierVersion
    {
        get
        {
            using var package = JsonDocument.Parse(File.ReadAllText(RootDirectory / "package.json"));
            return package.RootElement.GetProperty("devDependencies").GetProperty("prettier").GetString()!;
        }
    }

    // B-025: until 0001-F5's rule settings exist, the check's violations print and never fail the target (C-7, 0001-F2 A-2).
    private Target Specht => definition => definition
        .Executes(() =>
        {
            var clean = true;
            AnnotateChangedFiles(DotNet(
                "run --project src/tool -- --root .",
                RootDirectory,
                logger: ProcessTasks.DefaultLogger,
                exitHandler: process => clean = process.ExitCode == 0));
            Log.Information("Specht: {Verdict}; the check does not gate until 0001-F5's rule settings exist", clean ? "clean" : "violations reported");
        });

    // B-003, B-017: C# and Markdown, verified and never fixed (C-2, B-004); B-020: given --files, those and no other.
    // Both checks run before the target fails, so one formatter's failure never hides the other's files.
    private Target Format => definition => definition
        .DependsOn(Restore)
        .Executes(() =>
        {
            var csharp = Files?.Where(static file => file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)).ToArray();
            var markdown = Files?.Where(static file => file.EndsWith(".md", StringComparison.OrdinalIgnoreCase)).ToArray();

            var failed = new List<string>();

            if (csharp is not { Length: 0 } && !CheckCSharp(csharp))
            {
                failed.Add("C#");
            }

            if (markdown is not { Length: 0 } && !CheckMarkdown(markdown))
            {
                failed.Add("Markdown");
            }

            if (failed.Count > 0)
            {
                Assert.Fail($"Unformatted {string.Join(" and ", failed)}; the files are named above");
            }
        });

    private Target Restore => definition => definition
        .DependsOn(Specht)
        .Executes(() =>
        {
            // B-010: the tools the build and the hook invoke, at the versions the local tool manifest pins.
            DotNetToolRestore(static s => s.SetProcessWorkingDirectory(RootDirectory));
            DotNetRestore(s => s.SetProjectFile(Solution));
        });

    private Target Compile => definition => definition
        .DependsOn(Restore)
        .DependsOn(Format)
        .Executes(() => DotNetBuild(s => s
            .SetProjectFile(Solution)
            .SetConfiguration(Configuration)
            .EnableNoRestore()));

    // B-005: every test project in the solution - the unit, integration and acceptance tiers.
    // 0055-F3 B-001, C-3: the same run writes one Cobertura report per test project; the directory is emptied first so
    // no earlier run's report is counted or uploaded.
    private Target Test => definition => definition
        .DependsOn(Compile)
        .DependsOn(UnitTest)
        .DependsOn(IntegrationTest)
        .DependsOn(AcceptanceTest);

    // B-006: the classes trait-tagged Tier=Unit in every *.tests project; 0055-F3 B-001, C-7.
    private Target UnitTest => definition => definition
        .DependsOn(Compile)
        .Executes(() => TestTier("Unit"));

    // B-007: the classes trait-tagged Tier=Integration in every *.tests project; 0055-F3 B-001, C-7.
    private Target IntegrationTest => definition => definition
        .OnlyWhenStatic(static () => !OperatingSystem.IsWindows())
        .DependsOn(UnitTest)
        .Executes(() => TestTier("Integration"));

    // B-008: every scenario the acceptance project links, unfiltered; 0055-F3 B-001, C-7.
    private Target AcceptanceTest => definition => definition
        .OnlyWhenStatic(static () => !OperatingSystem.IsWindows())
        .DependsOn(IntegrationTest)
        .Executes(() =>
        {
            var coverage = TierCoverageDirectory("Acceptance");
            DotNet(
                $"test --project {RootDirectory / "test" / "acceptance" / "acceptance.csproj"} --configuration {Configuration} --no-build " +
                $"--coverage --coverage-output-format cobertura --results-directory {coverage}",
                workingDirectory: RootDirectory);
        });

    private Target Build => definition => definition
        .DependsOn(Test);

    // B-009: exactly one tool.<version>.nupkg under .artifacts/nupkg/.
    private Target Pack => definition => definition
        .DependsOn(Build)
        .Produces(PackageDirectory / "*.nupkg")
        .Executes(() =>
        {
            PackageDirectory.CreateOrCleanDirectory();
            DotNetPack(s => s
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(PackageDirectory)
                .EnableNoBuild());
        });

    private static string Include(string[]? files) => files is null ? string.Empty : $"--include {Quote(files)}";

    private static string Quote(IEnumerable<string> files) => string.Join(' ', files.Select(static file => $"\"{file}\""));

    private AbsolutePath TierCoverageDirectory(string tier)
    {
        var directory = CoverageDirectory / tier.ToLowerInvariant();
        directory.CreateOrCleanDirectory();
        return directory;
    }

    private void TestTier(string tier)
    {
        var coverage = TierCoverageDirectory(tier);
        foreach (var project in Solution.GetTestProjects())
        {
            DotNet(
                $"test --project {project.FilePath} --configuration {Configuration} --no-build --filter-trait \"Tier={tier}\" " +
                $"--coverage --coverage-output-format cobertura --results-directory {coverage}",
                workingDirectory: RootDirectory);
        }
    }

    // --verify-no-changes reports and writes nothing, so analyzer code fixes never apply (C-2).
    private bool CheckCSharp(string[]? files) =>
        Run(DotNetPath, $"format {Solution.Path} --verify-no-changes --no-restore {Include(files)}");

    private bool CheckMarkdown(string[]? files)
    {
        var prettier = $"--yes prettier@{PrettierVersion}";
        Log.Information("Markdown formatter: prettier {Version}", Npx($"{prettier} --version").Single(static line => line.Type == OutputType.Std).Text);
        return Run(NpxPath, $"{prettier} --check {Quote(files ?? ["**/*.md"])}");
    }

    private IReadOnlyCollection<Output> Npx(string arguments) =>
        ProcessTasks.StartProcess(NpxPath, arguments, RootDirectory, logOutput: false).AssertZeroExitCode().Output;

    private bool Run(string tool, string arguments)
    {
        using var process = ProcessTasks.StartProcess(tool, arguments, RootDirectory);
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Parameter("Files for Format to check, relative to the root - Default is every C# file in the solution and every Markdown file")]
    private readonly string[]? Files;

    [Solution]
    private readonly Solution Solution = null!;
}
