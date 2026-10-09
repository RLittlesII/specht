using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using specht.tool;

namespace specht.acceptance.Init;

/// <summary>
/// Steps for <c>src/specht.tool/Features/Init/.spec/init.feature</c> (0001-F4). Only B-004 is bound: the tool's embedded
/// copies of the newest version against this repository's live <c>.spec/schema/</c> and <c>.spec/templates/</c>, copied
/// into the output. The live manifest is read and given a rule setting in memory, which the comparison must ignore.
/// </summary>
[Binding]
[Scope(Feature = "init")]
public sealed partial class InitSteps
{
    [Given("the tool is built from this repository at one commit")]
    public void GivenTheToolIsBuiltFromThisRepositoryAtOneCommit() => _tool = typeof(ExitCodes).Assembly;

    [Given("this repository's manifest lowers one rule's severity")]
    public void GivenThisRepositorysManifestLowersOneRulesSeverity()
    {
        _liveManifest = JsonNode.Parse(File.ReadAllText(Live(Manifest)))!.AsObject();
        _liveManifest["rules"] = new JsonObject { ["SPEC031"] = "warning" };
    }

    [When("its embedded copies of the newest version are compared with the files under this repository's schema and templates folders")]
    public void WhenItsEmbeddedCopiesOfTheNewestVersionAreComparedWithTheFilesUnderThisRepositorysSchemaAndTemplatesFolders()
    {
        var version = NewestVersion();
        _files = ToolOwnedFiles.ToDictionary(
            static file => file,
            file => (Read(Embedded(file, version)), File.ReadAllBytes(Live(file))));
        var manifest = Read(Embedded(Manifest, version));
        _embeddedManifest = manifest is null ? null : JsonNode.Parse(manifest)!.AsObject();
    }

    [Then("each embedded frontmatter schema and template is byte-identical to its file")]
    public void ThenEachEmbeddedFrontmatterSchemaAndTemplateIsByteIdenticalToItsFile()
    {
        foreach (var (file, (embedded, live)) in Files)
        {
            embedded.Should().NotBeNull($"the tool embeds {file} at the newest version");
            embedded.Should().Equal(live, $"the embedded {file} ships the bytes of .spec/{file}");
        }
    }

    [Then("the embedded manifest equals the live manifest on every tool-owned key")]
    public void ThenTheEmbeddedManifestEqualsTheLiveManifestOnEveryToolOwnedKey()
    {
        _embeddedManifest.Should().NotBeNull($"the tool embeds {Manifest} at the newest version");
        var live = _liveManifest ?? throw new InvalidOperationException("No live manifest was read.");
        foreach (var (key, value) in _embeddedManifest!)
        {
            live.ContainsKey(key).Should().BeTrue($".spec/{Manifest} carries the tool-owned key {key}");
            JsonNode.DeepEquals(value, live[key]).Should().BeTrue($"the embedded and live manifests agree on the tool-owned key {key}");
        }
    }

    private Assembly Tool => _tool ?? throw new InvalidOperationException("No tool assembly was taken.");

    private Dictionary<string, (byte[]? Embedded, byte[] Live)> Files =>
        _files ?? throw new InvalidOperationException("Nothing was compared.");

    private static string Live(string file) => Path.Combine([AppContext.BaseDirectory, ".spec", .. file.Split('/')]);

    private static string Embedded(string file, int version) => file.Replace("/", $"/v{version}/", StringComparison.Ordinal);

    [GeneratedRegex("^schema/v([0-9]+)/")]
    private static partial Regex SchemaVersion();

    private byte[]? Read(string name)
    {
        using var stream = Tool.GetManifestResourceStream(name);
        if (stream is null)
        {
            return null;
        }

        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private int NewestVersion()
    {
        var versions = Tool.GetManifestResourceNames()
            .Select(static name => SchemaVersion().Match(name))
            .Where(static match => match.Success)
            .Select(static match => int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture))
            .ToList();
        versions.Should().NotBeEmpty("the tool embeds at least one schema/v<n>/ resource");
        return versions.Max();
    }

    private const string Manifest = "schema/spec-structure.schema.json";

    private static readonly string[] ToolOwnedFiles =
    [
        "schema/feature-spec.frontmatter.schema.json",
        "schema/task.frontmatter.schema.json",
        "schema/epic.frontmatter.schema.json",
        "templates/feature.md",
        "templates/decision.md",
        "templates/adr.md",
        "templates/lesson.md",
    ];

    private Assembly? _tool;
    private JsonObject? _liveManifest;
    private Dictionary<string, (byte[]? Embedded, byte[] Live)>? _files;
    private JsonObject? _embeddedManifest;
}
