using System;
using System.IO;
using System.Linq;
using System.Security;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

internal partial class SpechtBuild
{
    // 0055-F1 B-028: the normal build checks the package consumers against this checkout's analyzer.
    private Target CodeAnalysis => definition => definition
        .DependsOn(Compile)
        .Executes(CheckPackagedCodeAnalysis);

    private void CheckPackagedCodeAnalysis()
    {
        var artifacts = RootDirectory / ".artifacts" / "analysis-gate";
        var packages = artifacts / "packages";
        var cache = artifacts / "cache";
        var config = artifacts / "nuget.config";
        packages.CreateOrCleanDirectory();
        cache.CreateOrCleanDirectory();

        var globalPackages = DotNet("nuget locals global-packages --list", RootDirectory, logOutput: false)
            .Single(static line => line.Text.StartsWith("global-packages: ", StringComparison.Ordinal))
            .Text["global-packages: ".Length..];
        var packageSource = SecurityElement.Escape(packages.ToString());
        var installedSource = SecurityElement.Escape(globalPackages);
        File.WriteAllText(config, $$"""
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
                <add key="dogfood" value="{{packageSource}}" />
                <add key="installed" value="{{installedSource}}" />
              </packageSources>
              <packageSourceMapping>
                <clear />
                <packageSource key="dogfood"><package pattern="Specht.CodeAnalysis" /></packageSource>
                <packageSource key="installed"><package pattern="*" /></packageSource>
              </packageSourceMapping>
            </configuration>
            """);

        var deployment = RootDirectory / "src" / "codeanalysis" / "codeanalysis.csproj";
        DotNetPack(settings => settings
            .SetProject(deployment)
            .SetConfiguration(Configuration)
            .SetOutputDirectory(packages)
            .EnableNoBuild()
            .EnableNoRestore());

        var package = Directory.GetFiles(packages, "Specht.CodeAnalysis.*.nupkg").Single();
        var version = Path.GetFileNameWithoutExtension(package)["Specht.CodeAnalysis.".Length..];

        var previousCache = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        var previousServer = Environment.GetEnvironmentVariable("DOTNET_CLI_USE_MSBUILD_SERVER");
        var previousNodes = Environment.GetEnvironmentVariable("MSBUILDDISABLENODEREUSE");
        try
        {
            Environment.SetEnvironmentVariable("NUGET_PACKAGES", cache);
            Environment.SetEnvironmentVariable("DOTNET_CLI_USE_MSBUILD_SERVER", "0");
            Environment.SetEnvironmentVariable("MSBUILDDISABLENODEREUSE", "1");

            foreach (var project in new[]
                     {
                         RootDirectory / "src" / "analyzers" / "analyzers.csproj",
                         RootDirectory / "src" / "specht" / "specht.csproj",
                         RootDirectory / "src" / "tool" / "tool.csproj",
                     })
            {
                DotNet($"restore \"{project}\" --configfile \"{config}\" -maxcpucount:1 -p:NuGetAudit=false " +
                       $"-p:SpechtCodeAnalysisDogfood=true -p:SpechtCodeAnalysisDogfoodVersion={version}", RootDirectory);
                DotNet($"build \"{project}\" --configuration {Configuration} --no-restore --disable-build-servers " +
                       $"-maxcpucount:1 -p:ReportAnalyzer=true -p:SpechtCodeAnalysisDogfood=true " +
                       $"-p:SpechtCodeAnalysisDogfoodVersion={version}", RootDirectory);
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("NUGET_PACKAGES", previousCache);
            Environment.SetEnvironmentVariable("DOTNET_CLI_USE_MSBUILD_SERVER", previousServer);
            Environment.SetEnvironmentVariable("MSBUILDDISABLENODEREUSE", previousNodes);
            // Tests and packing use the ordinary project graph after this gate.
            DotNetRestore(settings => settings.SetProjectFile(Solution));
        }
    }
}
