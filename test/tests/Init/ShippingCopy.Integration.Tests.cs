using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Tool;

namespace Specht.Tests.Init;

/// <summary>
/// The shipping copy against this repository's live copy (0001-F4 B-004, C-2): the newest version's frontmatter schemas and
/// templates are byte-identical to the files under <c>.specht/schema/frontmatter/</c> and <c>.specht/templates/</c>, and its
/// manifest equals the live one, <c>.specht/manifest.json</c> (<c>0001-F5</c> decision 0011), on every key the
/// embedded manifest carries; keys only the live manifest carries are consumer configuration and outside the comparison.
/// </summary>
[Trait("Tier", "Integration")]
public sealed partial class ShippingCopyIntegrationTests
{
    public static TheoryData<string, string> ToolOwnedFiles { get; } = new()
    {
        { "schema", "frontmatter/feature.schema.json" },
        { "schema", "frontmatter/task.schema.json" },
        { "schema", "frontmatter/epic.schema.json" },
        { "templates", "feature.md" },
        { "templates", "decision.md" },
        { "templates", "adr.md" },
        { "templates", "lesson.md" },
    };

    [Theory]
    [MemberData(nameof(ToolOwnedFiles))]
    public void TheNewestEmbeddedFile_WhenComparedWithTheLiveCopy_ShouldBeByteIdentical(string folder, string file)
    {
        // Given
        var name = $"{folder}/v{NewestVersion()}/{file}";
        var path = Path.Combine([AppContext.BaseDirectory, SpecTree.ConfigurationFolder, folder, .. file.Split('/')]);
        File.Exists(path).Should().BeTrue($"this repository's live copy holds .specht/{folder}/{file}");
        var live = File.ReadAllBytes(path);

        // When
        var embedded = Read(name);

        // Then
        embedded.Should().Equal(live, $"{name} ships the bytes of .specht/{folder}/{file}");
    }

    [Fact]
    public void TheNewestEmbeddedManifest_WhenComparedWithTheLiveManifest_ShouldEqualItOnEveryKeyTheEmbeddedManifestCarries()
    {
        // Given
        var name = $"manifest/v{NewestVersion()}/manifest.json";
        var path = Path.Combine([AppContext.BaseDirectory, .. SpecTree.ManifestPath.Split('/')]);
        File.Exists(path).Should().BeTrue($"this repository's live manifest is {SpecTree.ManifestPath}");
        var live = JsonNode.Parse(File.ReadAllText(path))!.AsObject();

        // When
        var embedded = JsonNode.Parse(Read(name))!.AsObject();

        // Then
        embedded.Should().NotBeEmpty($"{name} carries the tool-owned keys");
        foreach (var (key, value) in embedded)
        {
            live.ContainsKey(key).Should().BeTrue($"{SpecTree.ManifestPath} carries the tool-owned key {key}");
            JsonNode.DeepEquals(value, live[key])
                .Should()
                .BeTrue($"{name} and {SpecTree.ManifestPath} agree on the tool-owned key {key}");
        }
    }

    private static Assembly Tool => typeof(ExitCodes).Assembly;

    private static byte[] Read(string name)
    {
        using var stream = Tool.GetManifestResourceStream(name);
        stream.Should().NotBeNull($"the tool embeds {name}");
        using var buffer = new MemoryStream();
        stream!.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static int NewestVersion()
    {
        var versions = Tool.GetManifestResourceNames()
            .Select(static name => SchemaVersion().Match(name))
            .Where(static match => match.Success)
            .Select(static match => int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture))
            .ToList();
        versions.Should().NotBeEmpty("the tool embeds at least one schema/v<n>/ resource");
        return versions.Max();
    }

    [GeneratedRegex("^schema/v([0-9]+)/")]
    private static partial Regex SchemaVersion();
}
