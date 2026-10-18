using System.Diagnostics;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using AwesomeAssertions;

namespace Specht.Build.Tests;

/// <summary>The working-package boundary and fresh installation policy (0001-F2 B-014, B-046, B-047).</summary>
[Trait("Tier", "Unit")]
public sealed class SelfCheckToolUnitTests
{
    [Theory]
    [ClassData(typeof(SelfCheckFailures))]
    public async Task APreparationOrCheckerFailure_WhenChecking_ShouldStopAtThatStageAndPreserveItsCode(int stage, int code)
    {
        // Given
        var fileSystem = new MockFileSystem();
        var commands = new List<ProcessStartInfo>();
        var checker = new SelfCheckTool(fileSystem, async command =>
        {
            commands.Add(command);
            if (commands.Count == 1 && stage > 0)
            {
                using var bytes = new MemoryStream();
                using (var zip = new ZipArchive(bytes, ZipArchiveMode.Create, leaveOpen: true))
                {
                    await using var writer = new StreamWriter(zip.CreateEntry("tool.nuspec").Open());
                    await writer.WriteAsync("<package><metadata><id>specht.tool</id><version>1.2.3-gabc</version></metadata></package>");
                }

                await fileSystem.File.WriteAllBytesAsync("repo/.artifacts/self-check/feed/tool.nupkg", bytes.ToArray());
            }

            return commands.Count - 1 == stage ? code : 0;
        });

        // When
        var result = await checker.RunAsync("repo", "Debug");

        // Then
        result.Should().Be(code);
        commands.Should().HaveCount(stage + 1);
        if (stage == 0)
        {
            fileSystem.File.Exists("repo/.artifacts/self-check/.config/dotnet-tools.json").Should().BeFalse();
        }
    }

    [Fact]
    public async Task WorkingEditsAtTheSameVersion_WhenCheckedAgain_ShouldInstallTheNewBytesWithFreshCachesAndKeepTheConsumerPin()
    {
        // Given
        var fileSystem = new MockFileSystem();
        const string consumer = "{\"tools\":{\"specht.tool\":{\"version\":\"0.1.131\"}}}";
        fileSystem.AddFile("repo/.config/dotnet-tools.json", new MockFileData(consumer));
        var installed = new List<string>();
        var commands = new List<ProcessStartInfo>();
        var revision = "first";
        var checker = new SelfCheckTool(fileSystem, async command =>
        {
            commands.Add(command);
            if (command.ArgumentList[0] == "pack")
            {
                fileSystem.File.Exists("repo/.artifacts/self-check/packages/stale").Should().BeFalse();
                using var bytes = new MemoryStream();
                using (var zip = new ZipArchive(bytes, ZipArchiveMode.Create, leaveOpen: true))
                {
                    await using (var writer = new StreamWriter(zip.CreateEntry("tool.nuspec").Open()))
                    {
                        await writer.WriteAsync("<package><metadata><id>specht.tool</id><version>1.2.3-gabc</version></metadata></package>");
                    }

                    await using var payload = new StreamWriter(zip.CreateEntry("payload").Open());
                    await payload.WriteAsync(revision);
                }

                await fileSystem.File.WriteAllBytesAsync("repo/.artifacts/self-check/feed/tool.nupkg", bytes.ToArray());
            }
            else if (command.ArgumentList[1] == "install")
            {
                command.ArgumentList.Should().ContainInConsecutiveOrder("--version", "1.2.3-gabc");
                command.ArgumentList.Should().ContainInConsecutiveOrder("--configfile", "nuget.config");
                command.WorkingDirectory.Should().Be(fileSystem.Path.GetFullPath("repo/.artifacts/self-check"));
                command.Environment["NUGET_PACKAGES"].Should().Be(fileSystem.Path.GetFullPath("repo/.artifacts/self-check/packages"));
                command.Environment["DOTNET_CLI_HOME"].Should().Be(fileSystem.Path.GetFullPath("repo/.artifacts/self-check/cli-home"));
                var manifest = JsonNode.Parse(await fileSystem.File.ReadAllTextAsync("repo/.artifacts/self-check/.config/dotnet-tools.json"))!;
                manifest["isRoot"]!.GetValue<bool>().Should().BeTrue();
                manifest["tools"]!.AsObject().Should().BeEmpty();
                var sources = XDocument.Parse(await fileSystem.File.ReadAllTextAsync("repo/.artifacts/self-check/nuget.config"));
                sources.Root!.Element("packageSources")!.Elements("add").Should().ContainSingle()
                    .Which.Attribute("value")!.Value.Should().Be("feed");
                sources.Root.Element("packageSources")!.Element("clear").Should().NotBeNull();
                using var bytes = new MemoryStream(await fileSystem.File.ReadAllBytesAsync("repo/.artifacts/self-check/feed/tool.nupkg"));
                using var zip = new ZipArchive(bytes);
                using var reader = new StreamReader(zip.GetEntry("payload")!.Open());
                installed.Add(await reader.ReadToEndAsync());
            }

            return 0;
        });

        // When
        (await checker.RunAsync("repo", "Release")).Should().Be(0);
        fileSystem.AddFile("repo/.artifacts/self-check/packages/stale", new MockFileData("old installation"));
        revision = "second";
        (await checker.RunAsync("repo", "Release")).Should().Be(0);

        // Then
        installed.Should().Equal("first", "second");
        commands.Should().HaveCount(6);
        commands[0].ArgumentList.Should().ContainInConsecutiveOrder("--configuration", "Release");
        commands[2].ArgumentList.Should().Equal("tool", "run", "specht", "--", "--root", "../..");
        commands[2].Environment["NUGET_PACKAGES"].Should().Be(commands[1].Environment["NUGET_PACKAGES"]);
        (await fileSystem.File.ReadAllTextAsync("repo/.config/dotnet-tools.json", TestContext.Current.CancellationToken)).Should().Be(consumer);
    }
}
