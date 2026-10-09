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
internal partial class Build : NukeBuild
{
    // B-001: `./build.sh` with no target name runs Compile, then Test.
    public static int Main() => Execute<Build>(static x => x.Test);

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

    private Target Restore => _ => _
        .Executes(() =>
        {
            // B-010: the tools the build and the hook invoke, at the versions the local tool manifest pins.
            DotNetToolRestore(static s => s.SetProcessWorkingDirectory(RootDirectory));
            DotNetRestore(s => s.SetProjectFile(Solution));
        });

    private Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() => DotNetBuild(s => s
            .SetProjectFile(Solution)
            .SetConfiguration(Configuration)
            .EnableNoRestore()));

    // B-005: every test project in the solution - the unit, integration and acceptance tiers.
    // 0055-F3 B-001, C-3: the same run writes one Cobertura report per test project; the directory is emptied first so
    // no earlier run's report is counted or uploaded.
    private Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            CoverageDirectory.CreateOrCleanDirectory();
            DotNet(
                $"test --solution {Solution.Path} --configuration {Configuration} --no-build " +
                $"--coverage --coverage-output-format cobertura --results-directory {CoverageDirectory}",
                workingDirectory: RootDirectory);
        });

    // B-003, B-017: C# and Markdown, verified and never fixed (C-2, B-004); B-020: given --files, those and no other.
    // Both checks run before the target fails, so one formatter's failure never hides the other's files.
    private Target Format => _ => _
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

    // B-009: exactly one specht.tool.<version>.nupkg under .artifacts/nupkg/.
    private Target Pack => _ => _
        .DependsOn(Compile)
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

    // B-023: until 0001-F2's check command exists there is nothing to run (C-7, decision 0001).
    private Target SpecCheck => static _ => _
        .Executes(static () => Log.Information("SpecCheck: the check is not yet available; 0001-F2's check command does not exist yet"));

    private static string Include(string[]? files) => files is null ? string.Empty : $"--include {Quote(files)}";

    private static string Quote(IEnumerable<string> files) => string.Join(' ', files.Select(static file => $"\"{file}\""));

    // --verify-no-changes reports and writes nothing, so analyzer code fixes never apply (C-2).
    private bool CheckCSharp(string[]? files) =>
        Run(DotNetTasks.DotNetPath, $"format {Solution.Path} --verify-no-changes --no-restore {Include(files)}");

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
