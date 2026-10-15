using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Json.Schema;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Check;

/// <summary>Command schema selection and failures (0001-F7 B-009 to B-011, B-036, B-101 to B-107).</summary>
[Trait("Tier", "Integration")]
public sealed class SchemaSourceIntegrationTests
{
    [Theory]
    [InlineData("0.2.0", "disk", null, "disk", true)]
    [InlineData("0.2.0", "embedded", "disk", "disk", true)]
    [InlineData("0.2.0", "disk", "embedded", "embedded", false)]
    [InlineData("0.1.0", null, "disk", "disk", true)]
    [InlineData("0.2.0", null, null, "embedded", false)]
    public void AValidSourceSelection_WhenChecked_ShouldUseItsSchemasAndReportItsProvenance(
        string pin, string? manifestSource, string? argument, string expectedSource, bool missingKey)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        tree.WriteRaw(SpecTree.FeatureSchemaPath, """{ "required": ["local_key"] }""");
        var path = Path.Combine(tree.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest["schemaVersion"] = pin;
        manifest["identifiers"]!.AsObject().Remove("epic");
        if (manifestSource is not null)
        {
            manifest["schemaSource"] = manifestSource;
        }
        File.WriteAllText(path, manifest.ToJsonString());
        File.Delete(Path.Combine(tree.Root, SpecTree.EpicSchemaPath));
        string[] args = argument is null ? ["--json"] : ["--json", "--schema-source", argument];

        var before = Directory.EnumerateFiles(tree.Root, "*", SearchOption.AllDirectories).ToDictionary(
            file => Path.GetRelativePath(tree.Root, file), file => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))));

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Launch(tree.Root, args);

        // Then
        stderr.Should().BeEmpty();
        exitCode.Should().BeOneOf(0, 1);
        var document = JsonNode.Parse(stdout)!;
        document["schemaSource"]!.GetValue<string>().Should().Be(expectedSource);
        document["schemaVersion"]!.GetValue<string>().Should().Be(pin);
        document["violations"]!.AsArray().Any(static v => v!["message"]!.GetValue<string>().Contains("local_key", StringComparison.Ordinal))
            .Should().Be(missingKey);
        File.ReadAllText(path).Should().Be(manifest.ToJsonString());
        var after = Directory.EnumerateFiles(tree.Root, "*", SearchOption.AllDirectories).ToDictionary(
            file => Path.GetRelativePath(tree.Root, file), file => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))));
        after.Should().BeEquivalentTo(before);
        var schema = JsonSchema.FromText(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "docs", "schema", "report.schema.json")));
        schema.Evaluate(JsonSerializer.Deserialize<JsonElement>(stdout)).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("feature", null)]
    [InlineData("feature", "{")]
    [InlineData("feature", "{ \"type\": 42 }")]
    [InlineData("feature", "{ \"$ref\": \"#/missing\" }")]
    [InlineData("task", null)]
    [InlineData("task", "{")]
    [InlineData("task", "{ \"type\": 42 }")]
    [InlineData("task", "{ \"$ref\": \"#/missing\" }")]
    [InlineData("epic", null)]
    [InlineData("epic", "{")]
    [InlineData("epic", "{ \"type\": 42 }")]
    [InlineData("epic", "{ \"$ref\": \"#/missing\" }")]
    public void AFaultySelectedDiskSchema_WhenChecked_ShouldExitThreeWithRelativeStderrAndEmptyStdout(string kind, string? text)
    {
        // Given
        using var tree = new SpecTree();
        var feature = tree.WriteFeature("0001", "F1");
        if (kind == "task")
        {
            tree.WriteItem(feature, "0001-01", "0001/F1");
        }
        else if (kind == "epic")
        {
            tree.WriteEpic("0001");
        }
        var relative = SpecTree.FrontmatterSchemaFolder + "/" + kind + ".schema.json";
        if (text is null)
        {
            File.Delete(Path.Combine(tree.Root, relative));
        }
        else
        {
            tree.WriteRaw(relative, text);
        }

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Launch(tree.Root, "--json", "--schema-source", "disk");

        // Then
        exitCode.Should().Be(3);
        stdout.Should().BeEmpty();
        stderr.Should().Contain(relative).And.NotContain(tree.Root);
    }

    [Theory]
    [InlineData(null, "disk", false, "upstream")]
    [InlineData(null, "disk", true, null)]
    [InlineData(null, "embedded", false, null)]
    [InlineData("embedded", "disk", false, null)]
    public void ARecordedSource_WhenExplicitlySelected_ShouldKeepItsIntegrityGuarantees(
        string? manifestSource, string argument, bool changed, string? provenance)
    {
        // Given
        using var tree = new SpecTree();
        var path = Path.Combine(tree.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest["spechtSchema"] = new JsonObject
        {
            ["location"] = "https://schemas.example/specht/",
            ["version"] = "v1",
            ["hashes"] = new JsonObject(new[] { "feature", "task", "epic" }.Select(kind =>
                KeyValuePair.Create(kind, (JsonNode?)("sha256:" + Convert.ToHexStringLower(SHA256.HashData(
                    File.ReadAllBytes(Path.Combine(tree.Root, SpecTree.FrontmatterSchemaFolder, kind + ".schema.json")))))))),
        };
        if (manifestSource is not null)
        {
            manifest["schemaSource"] = manifestSource;
        }
        File.WriteAllText(path, manifest.ToJsonString());
        if (changed)
        {
            File.AppendAllText(Path.Combine(tree.Root, SpecTree.FeatureSchemaPath), "\n");
        }

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Launch(tree.Root, "--json", "--schema-source", argument);

        // Then
        if (provenance is not null)
        {
            exitCode.Should().Be(0);
            stderr.Should().BeEmpty();
            JsonNode.Parse(stdout)!["schemaSource"]!.GetValue<string>().Should().Be(provenance);
        }
        else
        {
            exitCode.Should().Be(3);
            stdout.Should().BeEmpty();
            stderr.Should().Contain(changed ? "SHA-256" : "embedded");
        }
    }
}
