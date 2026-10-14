using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Security.Cryptography;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Specht.Composition;
using Specht.Tests.Shared;
using Specht.Tool;
using Specht.Tool.Features.Init;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Specht.Tests.Init;

/// <summary>CLI source binding and refusal exit codes (0001-F4 B-035, B-037 to B-040).</summary>
[Trait("Tier", "Integration")]
public sealed class NewSourceInitCommandIntegrationTests
{
    [Theory]
    [InlineData("record", 0)]
    [InlineData("fetch", 0)]
    [InlineData("missing hash", 3)]
    [InlineData("existing", 3)]
    [InlineData("mismatch", 3)]
    public void SourceArguments_WhenInitRuns_ShouldBindRepeatedHashesAndFoldTheOutcome(string mode, int exit)
    {
        // Given
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetFullPath("repo");
        fileSystem.AddDirectory(root);
        if (mode == "existing")
        {
            fileSystem.AddFile(Path.Combine(root, ".specht", "manifest.json"), new MockFileData("{}"));
        }

        var hash = "sha256:" + Convert.ToHexStringLower(SHA256.HashData("{}"u8.ToArray()));
        var requests = 0;
        using var client = new HttpClient(new Handler(_ =>
        {
            requests++;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(mode == "mismatch" ? "wrong"u8.ToArray() : "{}"u8.ToArray()),
            };
        }));
        using var errors = new StringWriter();
        var services = new ServiceCollection();
        services.AddSingleton<IFileSystem>(fileSystem);
        services.AddSingleton(client);
        services.AddSpechtEngine().AddInit();
        services.AddSingleton<TextWriter>(errors);
        var app = new CommandAppTester(new TypeRegistrar(services), console: new TestConsole().Width(80));
        app.Configure(static config => config.AddInit());
        var options = new List<string>
        {
            "init", "--root", root, "--upstream", "https://schemas.example/specht/",
            "--upstream-version", "release-label", "--upstream-hash", "feature=" + hash,
        };
        if (mode != "missing hash")
        {
            options.AddRange(["--upstream-hash", "task=" + hash]);
        }

        if (mode is "fetch" or "mismatch")
        {
            options.Add("--all");
        }

        // When
        var result = app.Run(options.ToArray());

        // Then
        result.ExitCode.Should().Be(exit, result.Output + errors);
        if (exit == 0)
        {
            errors.ToString().Should().BeEmpty();
            var source = EngineServices.Manifest(fileSystem, root).SpechtSchema;
            source.Should().NotBeNull();
            source!.Files.Should().HaveCount(2).And.AllSatisfy(file => file.Hash.Should().Be(hash));
            result.Output.Should().Contain("written .specht/manifest.json");
            requests.Should().Be(mode == "fetch" ? 2 : 0);
        }
        else
        {
            result.Output.Should().BeEmpty();
            errors.ToString().Should().NotBeEmpty().And.NotContain(root);
            requests.Should().Be(mode == "mismatch" ? 1 : 0);
            if (mode == "existing")
            {
                fileSystem.File.ReadAllText(Path.Combine(root, ".specht", "manifest.json")).Should().Be("{}");
                errors.ToString().Should().Contain(".specht/manifest.json");
                fileSystem.AllFiles.Should().HaveCount(1);
            }
            else
            {
                fileSystem.AllFiles.Should().BeEmpty();
            }
        }
    }

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }
}
