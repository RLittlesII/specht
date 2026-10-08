using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// Every gate is a target here (0055-F1 C-1); a target writes only under .artifacts/, bin/ and obj/ (C-5).
internal class Build : NukeBuild
{
    // B-001: `./build.sh` with no target name runs Compile, then Test.
    public static int Main() => Execute<Build>(static x => x.Test);

    private AbsolutePath ArtifactsDirectory => RootDirectory / ".artifacts";

    private AbsolutePath PackageDirectory => ArtifactsDirectory / "nupkg";

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
    private Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() => DotNet(
            $"test --solution {Solution.Path} --configuration {Configuration} --no-build",
            workingDirectory: RootDirectory));

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

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Solution]
    private readonly Solution Solution = null!;
}
