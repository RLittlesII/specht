using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Tool;

namespace Specht.Tests;

/// <summary>
/// The shipping copy against this repository's live copy (0001-F4 B-004, C-2): the newest version's frontmatter schemas and
/// templates are byte-identical to the files under <c>.spec/</c>, and its manifest equals the live one on every key the
/// embedded manifest carries; keys only the live manifest carries are consumer configuration and outside the comparison.
/// </summary>
[Trait("Tier", "Integration")]
public sealed partial class ShippingCopyIntegrationTests
{
    public static TheoryData<string, string> ToolOwnedFiles { get; } = new()
    {
        { "schema", "feature-spec.frontmatter.schema.json" },
        { "schema", "task.frontmatter.schema.json" },
        { "schema", "epic.frontmatter.schema.json" },
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
        var live = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, ".spec", folder, file));

        // When
        var embedded = Read(name);

        // Then
        embedded.Should().Equal(live, $"{name} ships the bytes of .spec/{folder}/{file}");
    }

    [Fact]
    public void TheNewestEmbeddedManifest_WhenComparedWithTheLiveManifest_ShouldEqualItOnEveryKeyTheEmbeddedManifestCarries()
    {
        // Given
        var name = $"schema/v{NewestVersion()}/spec-structure.schema.json";
        var live = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, ".spec", "schema", "spec-structure.schema.json")))!
            .AsObject();

        // When
        var embedded = JsonNode.Parse(Read(name))!.AsObject();

        // Then
        embedded.Should().NotBeEmpty($"{name} carries the tool-owned keys");
        foreach (var (key, value) in embedded)
        {
            live.ContainsKey(key).Should().BeTrue($".spec/schema/spec-structure.schema.json carries the tool-owned key {key}");
            JsonNode.DeepEquals(value, live[key])
                .Should()
                .BeTrue($"{name} and .spec/schema/spec-structure.schema.json agree on the tool-owned key {key}");
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
