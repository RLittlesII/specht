using System.Diagnostics;
using System.IO.Compression;
using System.Xml.Linq;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class CodeAnalysisPackageIntegrationTests
{
    [Fact]
    public async Task DeploymentProject_WhenPacked_ShouldCarryCompilerAssetsAndNoRuntimeDependencies()
    {
        if (!AppContext.TargetFrameworkName!.EndsWith("v10.0", StringComparison.Ordinal))
        {
            Assert.Skip("The package integration test runs once against the repository's .NET 10 consumer runtime.");
        }

        // Given
        var root = FindRepositoryRoot();
        var output = Path.Combine(root, ".artifacts", "codeanalysis-package-tests", Guid.NewGuid().ToString("N"));
        var consumer = Path.Combine(Path.GetTempPath(), "specht-codeanalysis-consumer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(output);

        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            process.StartInfo.ArgumentList.Add("pack");
            process.StartInfo.ArgumentList.Add("src/codeanalysis/codeanalysis.csproj");
            process.StartInfo.ArgumentList.Add("--configuration");
            process.StartInfo.ArgumentList.Add("Debug");
            process.StartInfo.ArgumentList.Add("--output");
            process.StartInfo.ArgumentList.Add(output);
            process.StartInfo.ArgumentList.Add("--no-restore");

            // When
            process.Start();
            var stdout = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            var stderr = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            var result = await stdout + await stderr;

            // Then
            process.ExitCode.Should().Be(0, result);
            var package = Directory.GetFiles(output, "Specht.CodeAnalysis.*.nupkg").Should().ContainSingle().Subject;
            var version = Path.GetFileNameWithoutExtension(package)["Specht.CodeAnalysis.".Length..];
            using var archive = ZipFile.OpenRead(package);
            var entries = archive.Entries.Select(static entry => entry.FullName).ToArray();
            entries.Should().Contain("analyzers/dotnet/cs/Specht.Analysis.dll");
            entries.Should().Contain("analyzers/dotnet/cs/Specht.Codefixes.dll");
            entries.Should().Contain("analyzers/dotnet/cs/JsonSchema.Net.dll");
            entries.Should().NotContain(static entry => entry.StartsWith("analyzers/dotnet/cs/Microsoft.CodeAnalysis", StringComparison.Ordinal));
            entries.Should().Contain("build/Specht.CodeAnalysis.props");
            entries.Should().Contain("build/Specht.CodeAnalysis.targets");

            var nuspec = archive.Entries.Single(static entry => entry.FullName.EndsWith(".nuspec", StringComparison.Ordinal));
            using var stream = nuspec.Open();
            var metadata = XDocument.Load(stream).Descendants().Single(static element => element.Name.LocalName == "metadata");
            metadata.Descendants().Where(static element => element.Name.LocalName == "dependency").Should().BeEmpty();
            entries.Should().NotContain(static entry => entry.StartsWith("lib/", StringComparison.Ordinal));

            // And the version remains aligned with the tool release.
            var toolPackage = await Pack(root, "src/tool/tool.csproj", output, "specht.tool");
            Path.GetFileNameWithoutExtension(toolPackage)["specht.tool.".Length..].Should().Be(version);

            // And a consumer build receives real manifest, Feature and schema contents through AdditionalFiles.
            Directory.CreateDirectory(Path.Combine(consumer, ".specht", "schema", "frontmatter"));
            Directory.CreateDirectory(Path.Combine(consumer, "src", "Feature", ".spec"));
            Directory.CreateDirectory(Path.Combine(consumer, ".spec", "adr"));
            await File.WriteAllTextAsync(Path.Combine(consumer, "Consumer.csproj"), $"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
                  <ItemGroup><PackageReference Include="Specht.CodeAnalysis" Version="{version}" /></ItemGroup>
                  <ItemGroup><SpechtTestEvidence Include="evidence/Sample.json" /></ItemGroup>
                  <Target Name="PrintSpechtInputs" BeforeTargets="CoreCompile">
                    <Message Importance="high" Text="SPECHT-ROOT=$(SpechtRoot)" />
                    <Message Importance="high" Text="SPECHT-INPUT:%(AdditionalFiles.SpechtInputKind)|%(AdditionalFiles.SpechtRelativePath)" />
                    <Message Importance="high" Text="SPECHT-ANALYZER:%(Analyzer.Identity)" />
                  </Target>
                </Project>
                """, TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(consumer, ".specht", "manifest.json"), """
                {
                  "revision":"consumer-r7",
                  "idPatterns":{"feature":"^F[0-9]+$","claim":"^B-[0-9]+$","adr":"^ADR-[0-9]+$"},
                  "claimHeading":"Claims",
                  "lifecycleMarkers":{"approved":"approved","withdrawn":"Withdrawn","superseded":"Superseded"},
                  "codeAnalysis":{
                    "projects":[{"path":"Consumer.csproj","targetFrameworks":["net10.0"]}],
                    "features":[{"feature":"F1"}],
                    "records":{
                      "adrProfiles":[{
                        "glob":".spec/adr/*.md","owner":"repository",
                        "identity":{"headingLevel":1,"separator":": "},
                        "lifecycle":{"section":"Status","firstWord":true}
                      }]
                    },
                    "mappings":{"/schemaVersion":"/revision","/identifiers":"/idPatterns","/claimsSection":"/claimHeading","/markers":"/lifecycleMarkers"}
                  }
                }
                """, TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(consumer, ".specht", "schema", "frontmatter", "feature.schema.json"), """
                {"type":"object","required":["id","spec_status"],"properties":{"id":{"type":"string"},"spec_status":{"type":"string"}}}
                """, TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(consumer, "src", "Feature", ".spec", "README.md"), """
                ---
                id: F1
                spec_status: draft
                ---
                ## Claims
                | ID | Status |
                | --- | --- |
                | B-001 | Active |
                """, TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(consumer, "Consumer.cs"), "internal sealed class Consumer;", TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(
                Path.Combine(consumer, "src", "Feature", ".spec", "adoption.feature"), "Feature: Consumer adoption\n",
                TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(
                Path.Combine(consumer, ".spec", "adr", "ADR-0001.md"), "# ADR-0001: Package inputs\n\n## Status\n\nAccepted.\n",
                TestContext.Current.CancellationToken);
            Directory.CreateDirectory(Path.Combine(consumer, "evidence"));
            await File.WriteAllTextAsync(Path.Combine(consumer, "evidence", "Sample.json"), "{}", TestContext.Current.CancellationToken);

            var restore = await Run(root, "restore", Path.Combine(consumer, "Consumer.csproj"), "--source", output, "--packages",
                Path.Combine(consumer, "packages"), "--source",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages"));
            restore.ExitCode.Should().Be(0, restore.Output);
            var designTime = await Run(root, "msbuild", Path.Combine(consumer, "Consumer.csproj"), "-target:PrintSpechtInputs",
                "-property:DesignTimeBuild=true");
            designTime.ExitCode.Should().Be(0, designTime.Output);
            designTime.Output.Should().Contain($"SPECHT-ROOT={consumer}");
            designTime.Output.Should().Contain("SPECHT-INPUT:configuration|.specht/manifest.json");
            designTime.Output.Should().Contain("SPECHT-INPUT:feature|src/Feature/.spec/README.md");
            designTime.Output.Should().Contain("SPECHT-INPUT:schema:feature|.specht/schema/frontmatter/feature.schema.json");
            designTime.Output.Should().Contain("SPECHT-INPUT:scenario|src/Feature/.spec/adoption.feature");
            designTime.Output.Should().Contain("SPECHT-INPUT:adr|.spec/adr/ADR-0001.md");
            designTime.Output.Should().Contain("SPECHT-ANALYZER:").And.Contain("JsonSchema.Net.dll");

            // And declared test evidence is delivered under the input kind the analyzer's test-method recognition reads.
            designTime.Output.Should().Contain("SPECHT-INPUT:test-evidence|evidence/Sample.json");

            // And standard compiler suppression controls presentation without changing input delivery.
            await File.WriteAllTextAsync(Path.Combine(consumer, "src", "Feature", ".spec", "README.md"), "invalid frontmatter\n",
                TestContext.Current.CancellationToken);
            var overridden = await Run(root, "build", Path.Combine(consumer, "Consumer.csproj"), "--no-restore", "--verbosity", "normal",
                "-p:UseSharedCompilation=false", "-p:NoWarn=SPCT0001");
            overridden.ExitCode.Should().Be(0, overridden.Output);
            overridden.Output.Should().NotContain("error SPCT0001");
            overridden.Output.Should().NotContain("AD0001");
            overridden.Output.Should().Contain("SPECHT-INPUT:feature|src/Feature/.spec/README.md");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
            if (Directory.Exists(consumer))
            {
                Directory.Delete(consumer, recursive: true);
            }
        }
    }

    private static async Task<string> Pack(string root, string project, string output, string packageId)
    {
        var result = await Run(root, "pack", project, "--configuration", "Debug", "--output", output, "--no-restore");
        result.ExitCode.Should().Be(0, result.Output);
        return Directory.GetFiles(output, packageId + ".*.nupkg").Should().ContainSingle().Subject;
    }

    private static async Task<(int ExitCode, string Output)> Run(string root, params string[] arguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();
        var stdout = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        return (process.ExitCode, await stdout + await stderr);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "specht.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
