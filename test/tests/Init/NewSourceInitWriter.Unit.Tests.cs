using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tool.Features.Init;

namespace Specht.Tests.Init;

/// <summary>New source recording, validation and preparation (0001-F4 B-035 to B-040).</summary>
[Trait("Tier", "Unit")]
public sealed class NewSourceInitWriterUnitTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public async Task ANewSource_WhenInitRuns_ShouldRecordItAndFetchOnlyAbsentSelectedSchemas(bool schemas, bool templates, bool preserve)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("repo");
        const string feature = "repo/.specht/schema/frontmatter/feature.schema.json";
        if (preserve)
        {
            fileSystem.AddFile(feature, new MockFileData("existing bytes"));
        }

        var requests = new List<string>();
        using var client = new HttpClient(new Handler(request =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Content) };
        }));
        var writer = new InitWriter(Copy, fileSystem, new UpstreamSchemaFetcher(client));

        // When
        await writer.Write("repo", schemas, templates, CancellationToken.None, Location, "release-label", Hashes);

        // Then
        var manifest = SpecManifest.Load(fileSystem, "repo");
        manifest.SpechtSchema.Should().NotBeNull();
        manifest.SpechtSchema!.Location.Should().Be(Location);
        manifest.SpechtSchema.Version.Should().Be("release-label");
        manifest.SpechtSchema.Files.Select(static file => file.Kind).Should().Equal("feature", "task");
        manifest.SpechtSchema.Files.Should().AllSatisfy(file => file.Hash.Should().Be(Hash));
        var writtenManifest = JsonNode.Parse(fileSystem.File.ReadAllText("repo/.specht/manifest.json"))!;
        writtenManifest["schemaVersion"]!.GetValue<string>().Should().Be("0.1.0");
        writtenManifest["identifiers"]!["feature"]!.GetValue<string>().Should().Be("^F[0-9]+$");
        requests.Should().BeEquivalentTo(schemas
            ? preserve ? [Location + "task.schema.json"] : new[] { Location + "feature.schema.json", Location + "task.schema.json" }
            : Array.Empty<string>());
        fileSystem.File.Exists("repo/.specht/templates/feature.md").Should().Be(templates);
        fileSystem.File.Exists("repo/.specht/schema/frontmatter/epic.schema.json").Should().BeFalse();
        if (preserve)
        {
            fileSystem.File.ReadAllText(feature).Should().Be("existing bytes");
        }
        else if (schemas)
        {
            fileSystem.File.ReadAllBytes(feature).Should().Equal(Content);
        }
    }

    [Theory]
    [InlineData("existing", true, true)]
    [InlineData("missing source", true, true)]
    [InlineData("missing version", true, true)]
    [InlineData("missing hash", true, true)]
    [InlineData("missing hash", false, false)]
    [InlineData("missing hash", false, true)]
    [InlineData("invalid source", true, true)]
    [InlineData("invalid version", true, true)]
    [InlineData("invalid hash", true, true)]
    [InlineData("malformed pair", true, true)]
    [InlineData("duplicate kind", true, true)]
    public async Task InvalidSourceOptions_WhenInitRuns_ShouldRefuseBeforeAnyFetchOrWrite(string fault, bool schemas, bool templates)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("repo");
        if (fault == "existing")
        {
            fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData("{}"));
        }

        var before = fileSystem.AllFiles.ToDictionary(static path => path, fileSystem.File.ReadAllBytes);
        var requests = 0;
        using var client = new HttpClient(new Handler(_ =>
        {
            requests++;
            throw new HttpRequestException("No request expected.");
        }));
        var writer = new InitWriter(Copy, fileSystem, new UpstreamSchemaFetcher(client));
        var location = fault == "missing source" ? null : fault == "invalid source" ? "private-value" : Location;
        var version = fault == "missing version" ? null : fault == "invalid version" ? "" : "release-label";
        string[] hashes = fault switch
        {
            "missing hash" => [Hashes[0]],
            "invalid hash" => [Hashes[0], "task=private-value"],
            "malformed pair" => [Hashes[0], "private-value"],
            "duplicate kind" => [.. Hashes, Hashes[0]],
            _ => Hashes,
        };

        // When
        var write = () => writer.Write("repo", schemas, templates, CancellationToken.None, location, version, hashes);

        // Then
        var exception = (await write.Should().ThrowExactlyAsync<SpechtManifestException>()).Which;
        exception.Message.Should().NotContain("private-value").And.NotContain(fileSystem.Path.GetFullPath("repo"));
        exception.Message.Should().MatchRegex(fault switch
        {
            "existing" => @"\.specht/manifest\.json",
            "missing source" or "invalid source" => "upstream|location",
            "missing version" or "invalid version" => "version",
            _ => "hash|kind|task",
        });
        requests.Should().Be(0);
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, bytes) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes);
        }
    }

    [Theory]
    [InlineData("unreachable")]
    [InlineData("mismatch")]
    [InlineData("later mismatch")]
    public async Task ANewSourceFetchFails_WhenEveryPartIsSelected_ShouldLeaveTheRootUnchanged(string failure)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("repo");
        var requests = 0;
        using var client = new HttpClient(new Handler(_ =>
        {
            requests++;
            if (failure == "unreachable")
            {
                throw new HttpRequestException("Synthetic failure.");
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(failure == "later mismatch" && requests == 1 ? Content : "wrong bytes"u8.ToArray()),
            };
        }));
        var writer = new InitWriter(Copy, fileSystem, new UpstreamSchemaFetcher(client));

        // When
        var write = () => writer.Write("repo", true, true, CancellationToken.None, Location, "release-label", Hashes);

        // Then
        (await write.Should().ThrowExactlyAsync<SpechtUpstreamSchemaException>()).Which.Message.Should().Contain(Location);
        requests.Should().Be(failure == "later mismatch" ? 2 : 1);
        fileSystem.AllFiles.Should().BeEmpty();
        fileSystem.Directory.Exists("repo/.specht").Should().BeFalse();
    }

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }

    private const string Location = "https://schemas.example/specht/";
    private static readonly byte[] Content = [0xEF, 0xBB, 0xBF, 0x7B, 0x0D, 0x0A, 0x7D];
    private static readonly string Hash = "sha256:" + Convert.ToHexStringLower(SHA256.HashData(Content));
    private static readonly string[] Hashes = ["feature=" + Hash, "task=" + Hash];
    private static readonly Dictionary<string, byte[]> Copy = new()
    {
        ["manifest/v0.1.0/manifest.json"] = "{\"schemaVersion\":\"0.1.0\",\"identifiers\":{\"feature\":\"^F[0-9]+$\",\"epic\":\"^[0-9]{4}$\"}}"u8.ToArray(),
        ["schema/v0.1.0/frontmatter/feature.schema.json"] = "{}"u8.ToArray(),
        ["schema/v0.1.0/frontmatter/task.schema.json"] = "{}"u8.ToArray(),
        ["schema/v0.1.0/frontmatter/epic.schema.json"] = "{}"u8.ToArray(),
        ["templates/v0.1.0/feature.md"] = "# Synthetic template"u8.ToArray(),
    };
}
