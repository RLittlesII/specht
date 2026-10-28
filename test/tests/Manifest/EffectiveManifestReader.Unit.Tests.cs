using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Resources;
using Specht.Rules;
using Specht.Tests.Rules;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Manifest;

/// <summary>Pins the effective document's preservation, defaulting and reparse boundaries (0001-F9 B-003 to B-007).</summary>
[Trait("Tier", "Unit")]
public sealed class EffectiveManifestReaderUnitTests
{
    [Fact]
    public void DeclaredJson_WhenProjected_ShouldKeepItsValuesAndNestedExtensions()
    {
        // Given
        const string text = """
            {
              "schemaVersion":"0.2.0", "$metadata":{"revision":3,"nullable":null},
              "sections":["3. Consumer claims","9. Traceability Matrix","12. Sign-off"],
              "roles":{"claims":"3. Consumer claims"}, "markers":{"missing":"TBD"},
              "identifiers":{"claim":"^REQ-[0-9]+$"}, "frontmatterSchemas":{"feature":"consumer.json"},
              "layouts":[{"name":"consumer","glob":"custom/**/*.md","extension":{"kept":true}}],
              "exclusions":[], "taskFiles":["item.md"], "epicFiles":["epic.md"], "companionFiles":["case.feature"],
              "tables":{"claims":["Consumer ID"]}, "claim":{"tag":"@{claim}","extension":[1,null]},
              "codeAnalysis":{"records":{"claimSource":"gherkin","extension":{"keep":false}}},
              "tests":{"files":["test/**/*.cs"],"declarations":[]}, "owners":{},
              "requiredSpecifications":[], "consumerTemplates":[], "matrix":{"scenarioTitles":false,"extension":7},
              "schemaSource":"disk"
            }
            """;
        var declared = JsonNode.Parse(text)!.AsObject();
        var files = new MockFileSystem(new Dictionary<string, MockFileData> { ["consumer/.specht/manifest.json"] = new(text) });

        // When
        var document = EngineServices.EffectiveManifest("consumer", files);

        // Then
        foreach (var (key, value) in declared)
        {
            if (key is "roles" or "markers" or "identifiers" or "frontmatterSchemas" or "tables" or "claim")
            {
                foreach (var (member, memberValue) in value!.AsObject())
                {
                    JsonNode.DeepEquals(document[key]![member], memberValue).Should().BeTrue($"{key}.{member} was declared");
                }
            }
            else
            {
                JsonNode.DeepEquals(document[key], value).Should().BeTrue($"{key} was declared");
            }
        }
    }

    [Theory]
    [ClassData(typeof(EffectiveManifestPins))]
    public void OmittedValues_WhenProjected_ShouldCarryTheExactPinsSelectedDefaults(string pin, string label)
    {
        // Given
        var data = new PinnedDefaultsData();
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["consumer/.specht/manifest.json"] = new($$"""{"schemaVersion":"{{pin}}"}"""),
        });

        // When
        var document = EngineServices.EffectiveManifest("consumer", files, data.Versions);

        // Then
        var defaults = JsonNode.Parse(data.Versions.Find(new SemanticVersion(0, pin == "0.1.0" ? 1 : 2, 0))!.Manifest)!;
        foreach (var key in new[]
        {
            "sections", "tables", "roles", "markers", "frontmatterSchemas", "layouts", "exclusions", "taskFiles", "epicFiles", "companionFiles",
        })
        {
            JsonNode.DeepEquals(document[key], defaults[key]).Should().BeTrue($"{key} comes from {label} defaults");
        }

        document["identifiers"]!["claim"]!.GetValue<string>().Should().Be($"^{label}-[0-9]+$");
        document["identifiers"]!.AsObject().Should().NotContainKey("epic");
        document.Should().NotContainKey("spechtSchema");
    }

    [Theory]
    [ClassData(typeof(EffectiveManifestPins))]
    public void PartialMapsAndTables_WhenProjected_ShouldFollowThePinsMergeBoundary(string pin, string label)
    {
        // Given
        var data = new PinnedDefaultsData();
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["consumer/.specht/manifest.json"] = new($$$"""
                {"schemaVersion":"{{{pin}}}", "roles":{"claims":"3. {{{label}}} claims"},
                 "markers":{"missing":"TBD"}, "frontmatterSchemas":{"feature":"consumer.json"},
                 "identifiers":{"claim":"^REQ-[0-9]+$"}, "tables":{"claims":["Consumer ID"]}}
                """),
        });

        // When
        var document = EngineServices.EffectiveManifest("consumer", files, data.Versions);

        // Then
        document["roles"]!["matrix"]!.GetValue<string>().Should().Be($"9. {label} matrix");
        document["markers"]!["missing"]!.GetValue<string>().Should().Be("TBD");
        document["markers"]!.AsObject().Should().ContainKey("draft");
        document["frontmatterSchemas"]!["feature"]!.GetValue<string>().Should().Be("consumer.json");
        document["frontmatterSchemas"]!["task"]!.GetValue<string>().Should().Be("task.schema.json");
        document["identifiers"]!["claim"]!.GetValue<string>().Should().Be("^REQ-[0-9]+$");
        document["identifiers"]!.AsObject().Should().ContainKey("task").And.NotContainKey("epic");
        JsonNode.DeepEquals(document["tables"]!["claims"], new JsonArray("Consumer ID")).Should().BeTrue();
        document["tables"]!.AsObject().ContainsKey("matrix").Should().Be(pin == "0.2.0");
    }

    [Theory]
    [ClassData(typeof(EffectiveManifestNulls))]
    public void AcceptedNull_WhenProjected_ShouldRemainDeclared(string key)
    {
        // Given
        var input = new JsonObject { ["schemaVersion"] = "0.2.0", [key] = null };
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["consumer/.specht/manifest.json"] = new(input.ToJsonString()),
        });

        // When
        var document = EngineServices.EffectiveManifest("consumer", files);

        // Then
        document.Should().ContainKey(key);
        if (key == "rules")
        {
            document[key]!.AsObject().Should().NotBeEmpty();
        }
        else
        {
            document[key].Should().BeNull();
        }
    }

    [Theory]
    [ClassData(typeof(RuleCatalogueLevels))]
    public void ExplicitAndMissingLevels_WhenProjected_ShouldUseTheSharedPinnedPagePrecedence(SpecRuleLevel level)
    {
        // Given
        var defaults = JsonNode.Parse(new SpechtResources().DefaultManifest)!.AsObject();
        defaults["rules"] = new JsonObject { ["SPEC010"] = "error", ["SPEC020"] = "error" };
        SchemaVersion version = new SchemaVersionFixture().WithManifest(defaults.ToJsonString())
            .WithRuleIds(new HashSet<string>(StringComparer.Ordinal) { "SPEC020", "SPEC010" });
        version = version with
        {
            RulePages = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["SPEC010"] = "## Metadata\n\n| Property | Value |\n| --- | --- |\n| Family | Synthetic |\n| Default severity | Warning |\n",
                ["SPEC020"] = "## Metadata\n\n| Property | Value |\n| --- | --- |\n| Family | Synthetic |\n| Default severity | Off |\n",
            },
        };
        var versions = new SchemaVersions([version]);
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["consumer/.specht/manifest.json"] = new(new JsonObject
            {
                ["rules"] = new JsonObject { ["SPEC010"] = level.ToString().ToLowerInvariant() },
            }.ToJsonString()),
        });

        // When
        var document = EngineServices.EffectiveManifest("consumer", files, versions);

        // Then
        document["rules"]!.AsObject().Select(static entry => entry.Key).Should().Equal("SPEC010", "SPEC020");
        document["rules"]!["SPEC010"]!.GetValue<string>().Should().Be(level.ToString().ToLowerInvariant());
        document["rules"]!["SPEC020"]!.GetValue<string>().Should().Be("off");
        new SpecManifestParser(versions).Parse("{}").Rules.Should().BeEmpty();
    }

    [Fact]
    public void NoPinOrOptionalCapabilities_WhenProjected_ShouldRetainTheOriginalVersionAndDeclarationSemantics()
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData> { ["consumer/.specht/manifest.json"] = new("{}") });
        var parser = EngineServices.Parser();
        var original = parser.Parse("{}");

        // When
        var document = EngineServices.EffectiveManifest("consumer", files);
        var reparsed = parser.Parse(document.ToJsonString());

        // Then
        document["schemaVersion"]!.GetValue<string>().Should().Be("0.1.0");
        document.Should().NotContainKeys("claim", "schemaSource", "spechtSchema", "tests", "matrix", "owners", "requiredSpecifications", "consumerTemplates");
        reparsed.Should().BeEquivalentTo(original, static options => options.Excluding(static structure => structure.Rules));
        reparsed.DeclaresEpicGrammar.Should().BeFalse();
    }

    [Theory]
    [ClassData(typeof(EffectiveManifestDeclarations))]
    public void OptionalDeclarationsAndNestedNulls_WhenReparsed_ShouldKeepTheChecksInterpretation(string text)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData> { ["consumer/.specht/manifest.json"] = new(text) });
        var schemas = EngineServices.Schemas(files);
        var parser = EngineServices.Parser();
        var rules = new RuleEntriesReader(schemas, parser);
        var original = parser.Parse(text);

        // When
        var document = EngineServices.EffectiveManifest("consumer", files);
        var reparsed = parser.Parse(document.ToJsonString());

        // Then
        reparsed.Should().BeEquivalentTo(original, static options => options.Excluding(static structure => structure.Rules));
        rules.Read(reparsed).Should().BeEquivalentTo(rules.Read(original), static options => options.WithStrictOrdering());
        var declared = JsonNode.Parse(text)!.AsObject();
        if (declared["claim"] is JsonObject claim)
        {
            foreach (var (key, value) in claim)
            {
                JsonNode.DeepEquals(document["claim"]![key], value).Should().BeTrue();
            }
        }
    }

    [Fact]
    public void ARecordedSourceWithoutLocalSchemas_WhenInspected_ShouldPreserveItsJsonWithoutLoadingTheSchemas()
    {
        // Given
        var text = new JsonObject
        {
            ["schemaVersion"] = "0.2.0",
            ["schemaSource"] = "disk",
            ["spechtSchema"] = new JsonObject
            {
                ["location"] = "https://schemas.example/invented/",
                ["version"] = "synthetic",
                ["hashes"] = new JsonObject
                {
                    ["feature"] = "sha256:" + new string('a', 64),
                    ["task"] = "sha256:" + new string('b', 64),
                },
            },
        };
        var files = new MockFileSystem(new Dictionary<string, MockFileData> { ["consumer/.specht/manifest.json"] = new(text.ToJsonString()) });
        var pathsBefore = files.AllFiles.ToArray();

        // When
        var document = EngineServices.EffectiveManifest("consumer", files);

        // Then
        JsonNode.DeepEquals(document["spechtSchema"], text["spechtSchema"]).Should().BeTrue();
        document["schemaSource"]!.GetValue<string>().Should().Be("disk");
        files.AllFiles.Should().Equal(pathsBefore);
        files.File.ReadAllText("consumer/.specht/manifest.json").Should().Be(text.ToJsonString());
    }

    [Fact]
    public void AlternatingRootsAndMutatedResults_WhenProjected_ShouldKeepDefaultsAndOtherInvocationsIsolated()
    {
        // Given
        var data = new PinnedDefaultsData();
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["older/.specht/manifest.json"] = new("{}"),
            ["newer/.specht/manifest.json"] = new("{\"schemaVersion\":\"0.2.0\"}"),
        });

        var schemas = EngineServices.Schemas(files, data.Versions);
        var reader = new EffectiveManifestReader(schemas, new RuleEntriesReader(schemas, new SpecManifestParser(data.Versions)), data.Versions);

        // When
        var older = reader.Read("older");
        var newer = reader.Read("newer");
        older["roles"]!["claims"] = "Changed by caller";
        var repeated = reader.Read("older");

        // Then
        newer["roles"]!["claims"]!.GetValue<string>().Should().Be("3. Newer claims");
        repeated["roles"]!["claims"]!.GetValue<string>().Should().Be("3. Older claims");
        JsonNode.Parse(data.Versions.Find(new SemanticVersion(0, 1, 0))!.Manifest)!["roles"]!["claims"]!.GetValue<string>()
            .Should().Be("3. Older claims");
        files.File.ReadAllText("older/.specht/manifest.json").Should().Be("{}");
    }
}
