using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tool.Features.Init;

namespace Specht.Tests.Init;

/// <summary>The upstream selection and validation before any init write (0001-F4 B-014 to B-016, B-027, B-034).</summary>
[Trait("Tier", "Unit")]
public sealed class UpstreamInitWriterUnitTests
{
    public static TheoryData<bool, bool, bool, bool, bool> OfflineSelections => new()
    {
        { false, true, true, false, false },
        { false, false, true, false, false },
        { false, true, false, false, true },
        { true, false, false, false, true },
        { true, false, true, false, true },
        { true, true, true, true, true },
    };

    [Fact]
    public async Task AnUpstreamSourceWithCustomFileNames_WhenSchemasAreSelected_ShouldSelectTheSourcesMissingFiles()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("repo");
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(Manifest()));
        byte[] present = [1, 2, 3];
        fileSystem.AddFile("repo/.specht/schema/frontmatter/custom-feature.json", new MockFileData(present));
        var addresses = new List<string>();
        using var client = new HttpClient(new SyntheticHandler(request =>
        {
            addresses.Add(request.RequestUri!.AbsoluteUri);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Content) };
        }));

        // When
        var listing = await new InitWriter(ShippingCopy, fileSystem, new UpstreamSchemaFetcher(client))
            .Write("repo", schemas: true, templates: false, CancellationToken.None);

        // Then
        listing.Should().Equal(
            (".specht/manifest.json", false),
            (".specht/schema/frontmatter/custom-epic.json", true),
            (".specht/schema/frontmatter/custom-feature.json", false),
            (".specht/schema/frontmatter/custom-task.json", true));
        addresses.Should().BeEquivalentTo(
            "https://schemas.example/specht/custom-epic.json",
            "https://schemas.example/specht/custom-task.json");
        fileSystem.File.ReadAllBytes("repo/.specht/schema/frontmatter/custom-feature.json").Should().Equal(present);
        fileSystem.File.ReadAllBytes("repo/.specht/schema/frontmatter/custom-task.json").Should().Equal(Content);
        fileSystem.File.ReadAllBytes("repo/.specht/schema/frontmatter/custom-epic.json").Should().Equal(Content);
        fileSystem.File.ReadAllText("repo/.specht/manifest.json").Should().Be(Manifest());
        fileSystem.AllFiles.Should().HaveCount(4);
    }

    [Theory]
    [InlineData("hash")]
    [InlineData("unreachable")]
    [InlineData("http")]
    public async Task ALaterUpstreamFileFailsValidation_WhenEveryPartIsSelected_ShouldWriteNoFile(string failure)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("repo");
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(Manifest()));
        var before = fileSystem.AllFiles.ToDictionary(static path => path, fileSystem.File.ReadAllBytes);
        var requests = 0;
        using var client = new HttpClient(new SyntheticHandler(_ =>
        {
            requests++;
            if (requests == 1)
            {
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Content) };
            }

            return failure switch
            {
                "hash" => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent("{}"u8.ToArray()) },
                "http" => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
                _ => throw new HttpRequestException("Synthetic source is unreachable."),
            };
        }));
        var writer = new InitWriter(ShippingCopy, fileSystem, new UpstreamSchemaFetcher(client));

        // When
        var write = () => writer.Write("repo", schemas: true, templates: true, CancellationToken.None);

        // Then
        (await write.Should().ThrowExactlyAsync<SpechtUpstreamSchemaException>()).Which.Message.Should().Contain("schemas.example");
        requests.Should().Be(2, "the failure follows one successfully validated download");
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, bytes) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes);
        }
    }

    [Theory]
    [MemberData(nameof(OfflineSelections))]
    public async Task ARunWithoutWorkForTheSource_WhenSelected_ShouldAttemptNoNetworkCall(
        bool source, bool schemas, bool templates, bool present, bool manifestPresent)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("repo");
        if (source)
        {
            fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(Manifest()));
        }
        else if (manifestPresent)
        {
            fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData("{}"));
        }

        if (present)
        {
            foreach (var kind in new[] { "feature", "task", "epic" })
            {
                fileSystem.AddFile($"repo/.specht/schema/frontmatter/custom-{kind}.json", new MockFileData("local bytes"));
            }
        }

        var requests = 0;
        using var client = new HttpClient(new SyntheticHandler(_ =>
        {
            requests++;
            throw new HttpRequestException("Network unavailable.");
        }));

        // When
        await new InitWriter(ShippingCopy, fileSystem, new UpstreamSchemaFetcher(client))
            .Write("repo", schemas, templates, CancellationToken.None);

        // Then
        requests.Should().Be(0);
        if (present)
        {
            foreach (var kind in new[] { "feature", "task", "epic" })
            {
                fileSystem.File.ReadAllText($"repo/.specht/schema/frontmatter/custom-{kind}.json").Should().Be("local bytes");
            }
        }
    }

    [Fact]
    public async Task AnUpstreamFileOutsideTheSchemaFolder_WhenSchemasAreSelected_ShouldRejectItBeforeAnyFetchOrWrite()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("repo");
        var manifest = JsonNode.Parse(Manifest())!;
        manifest["frontmatterSchemas"]!["feature"] = "../../outside.json";
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest.ToJsonString()));
        var before = fileSystem.AllFiles.ToDictionary(static path => path, fileSystem.File.ReadAllBytes);
        var requests = 0;
        using var client = new HttpClient(new SyntheticHandler(_ =>
        {
            requests++;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Content) };
        }));
        var writer = new InitWriter(ShippingCopy, fileSystem, new UpstreamSchemaFetcher(client));

        // When
        var write = () => writer.Write("repo", schemas: true, templates: true, CancellationToken.None);

        // Then
        var exception = (await write.Should().ThrowExactlyAsync<SpechtManifestException>()).Which;
        exception.Message.Should().Contain(".specht/manifest.json").And.NotContain(fileSystem.Path.GetFullPath("repo"));
        requests.Should().Be(0);
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, bytes) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes);
        }
    }

    [Fact]
    public async Task APresentUpstreamSchemaRemovedDuringAnotherFetch_WhenInitWrites_ShouldKeepTheOriginalSkipDecision()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(Manifest()));
        const string present = "repo/.specht/schema/frontmatter/custom-feature.json";
        fileSystem.AddFile(present, new MockFileData("local bytes"));
        using var client = new HttpClient(new SyntheticHandler(_ =>
        {
            fileSystem.File.Delete(present);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Content) };
        }));

        // When
        var listing = await new InitWriter(ShippingCopy, fileSystem, new UpstreamSchemaFetcher(client))
            .Write("repo", schemas: true, templates: false, CancellationToken.None);

        // Then
        listing.Should().Contain((".specht/schema/frontmatter/custom-feature.json", false));
        fileSystem.File.Exists(present).Should().BeFalse("a skipped file must not be recreated from an empty download buffer");
    }

    private static string Manifest()
    {
        var hash = "sha256:" + Convert.ToHexStringLower(SHA256.HashData(Content));
        return new JsonObject
        {
            ["identifiers"] = new JsonObject { ["feature"] = "^F[0-9]+$", ["epic"] = "^[0-9]{4}$" },
            ["frontmatterSchemas"] = new JsonObject
            {
                ["feature"] = "custom-feature.json",
                ["task"] = "custom-task.json",
                ["epic"] = "custom-epic.json",
            },
            ["spechtSchema"] = new JsonObject
            {
                ["location"] = "https://schemas.example/specht/",
                ["version"] = "2026.10",
                ["hashes"] = new JsonObject { ["feature"] = hash, ["task"] = hash, ["epic"] = hash },
            },
        }.ToJsonString();
    }

    private sealed class SyntheticHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }

    private static readonly byte[] Content = [0xEF, 0xBB, 0xBF, 0x7B, 0x0D, 0x0A, 0x7D];
    private static readonly Dictionary<string, byte[]> ShippingCopy = new()
    {
        ["manifest/v0.1.0/manifest.json"] = "{}"u8.ToArray(),
        ["schema/v0.1.0/frontmatter/feature.schema.json"] = "{}"u8.ToArray(),
        ["schema/v0.1.0/frontmatter/task.schema.json"] = "{}"u8.ToArray(),
        ["templates/v0.1.0/feature.md"] = "# Synthetic template"u8.ToArray(),
    };


}
