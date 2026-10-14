using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Specht.Composition;
using Specht.Tests.Shared;
using Specht.Tool;
using Specht.Tool.Features.Init;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Specht.Tests.Init;

/// <summary>Init folds an upstream refusal into stderr and exit 3 without a listing or writes (0001-F4 B-015, B-016).</summary>
[Trait("Tier", "Integration")]
public sealed class UpstreamInitCommandIntegrationTests
{
    [Theory]
    [InlineData(false, "does not match its recorded SHA-256.")]
    [InlineData(true, "could not be reached.")]
    public void ARefusedUpstreamSource_WhenInitRuns_ShouldNameItOnStandardErrorExitThreeAndWriteNothing(bool unreachable, string reason)
    {
        // Given
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetFullPath("repo");
        var hash = "sha256:" + Convert.ToHexStringLower(SHA256.HashData("expected bytes"u8.ToArray()));
        var manifest = new JsonObject
        {
            ["spechtSchema"] = new JsonObject
            {
                ["location"] = "https://schemas.example/specht/",
                ["version"] = "2026.10",
                ["hashes"] = new JsonObject { ["feature"] = hash, ["task"] = hash },
            },
        }.ToJsonString();
        fileSystem.AddFile(Path.Combine(root, ".specht", "manifest.json"), new MockFileData(manifest));
        var before = fileSystem.AllFiles.ToDictionary(static path => path, fileSystem.File.ReadAllBytes);
        using var client = new HttpClient(new SyntheticHandler(unreachable));
        using var errors = new StringWriter();
        var services = new ServiceCollection();
        services.AddSingleton<IFileSystem>(fileSystem);
        services.AddSingleton(client);
        services.AddSpechtEngine().AddInit();
        services.AddSingleton<TextWriter>(errors);
        var app = new CommandAppTester(new TypeRegistrar(services), console: new TestConsole().Width(80));
        app.Configure(static config => config.AddInit());

        // When
        var result = app.Run("init", "--root", root, "--all");

        // Then
        result.ExitCode.Should().Be(ExitCodes.InvalidManifest);
        result.Output.Should().BeEmpty();
        errors.ToString().Should().Be($"specht: upstream schema source 'https://schemas.example/specht/feature.schema.json' {reason}{Environment.NewLine}");
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, bytes) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes);
        }
    }

    private sealed class SyntheticHandler(bool unreachable) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (unreachable)
            {
                throw new HttpRequestException("Synthetic source is unreachable.");
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent("wrong bytes"u8.ToArray()) });
        }
    }
}
