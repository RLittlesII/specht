using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// Each rule message that names a section, a frontmatter key, a marker or the manifest's own path, over a model read in
/// memory from a synthetic tree under a manifest that renames the value (<c>0001-F5</c> B-009; decision 0019). A section is
/// cited <c>§ N</c> by the leading number of its role's title, or <c>## &lt;title&gt;</c> when the title has none; a key
/// and a marker are named as the manifest names them; the manifest is named by its path under the root. One rule, taken
/// from the composed set, evaluates the model, so each row's one finding is the rule's decision alone.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class RuleMessagesUnitTests
{
    [Theory]
    [ClassData(typeof(RetitledSectionMessages))]
    public void ASectionTheManifestRetitles_WhenARuleCitingItEvaluatesTheModel_ShouldCiteItByItsTitlesLeadingNumberOrElseByItsTitle(
        string ruleId,
        string manifest,
        string specification,
        string? companion,
        string cited,
        string defaultCitation)
    {
        // Given
        (string Path, string Text)[] files = companion is null
            ? [(Specification, specification)]
            : [(Specification, specification), ("src/sample/.spec/sample.feature", companion)];
        var model = Read(manifest, files);
        var rule = EngineServices.Rules().Single(rule => rule.Id == ruleId);

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Should().ContainSingle().Which.Message.Should().Contain(cited).And.NotContain(defaultCitation);
    }

    [Theory]
    [ClassData(typeof(RenamedKeyMessages))]
    public void AFrontmatterKeyTheManifestRenames_WhenARuleNamingItEvaluatesTheModel_ShouldNameItByTheManifestsName(
        string ruleId,
        string keys,
        string path,
        string text,
        string named,
        string defaultName)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" },
            ["keys"] = JsonNode.Parse(keys),
        };
        var model = Read(manifest.ToJsonString(), ("src/area/.spec/README.md", "---\nepic: '0001'\nid: F1\n---\n"), (path, text));
        var rule = EngineServices.Rules().Single(rule => rule.Id == ruleId);

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Should().ContainSingle().Which.Message.Should().Contain(named).And.NotContain(defaultName);
    }

    [Theory]
    [ClassData(typeof(RenamedMarkerMessages))]
    public void AMarkerTheManifestRenames_WhenARuleNamingItEvaluatesTheModel_ShouldNameItByTheManifestsText(
        string ruleId,
        string manifest,
        string specification,
        string named,
        string defaultText)
    {
        // Given
        var model = Read(manifest, (Specification, specification));
        var rule = EngineServices.Rules().Single(rule => rule.Id == ruleId);

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Should().ContainSingle().Which.Message.Should().Contain(named).And.NotContain(defaultText);
    }

    [Fact]
    public void ASectionTheSpecificationLacks_WhenTheSectionContractRuleEvaluatesTheModel_ShouldNameItsTitleAndTheManifestByItsPathUnderTheRoot()
    {
        // Given
        var manifest = """
            {"schemaVersion":"0.3.0","sections":["3. Claims","9. Traceability Matrix","12. Sign-off"],"roles":{"claims":"3. Claims"}}
            """;
        var model = Read(
            manifest,
            (Specification, "---\nepic: \"0001\"\nid: F1\n---\n\n## 9. Traceability Matrix\n\nNone.\n\n## 12. Sign-off\n\nNone.\n"));
        var rule = EngineServices.Rules().Single(static rule => rule.Id == "SPEC010");

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Should().ContainSingle().Which.Message.Should()
            .Contain("'## 3. Claims'")
            .And.Contain(".specht/manifest.json")
            .And.NotContain(new MockFileSystem().Path.GetFullPath("repo"));
    }

    private static SpechtModel Read(string manifest, params (string Path, string Text)[] files)
    {
        var fileSystem = new MockFileSystem(
            files.Append((Path: SpechtTree.ManifestPath, Text: manifest))
                .ToDictionary(
                    static file => Path.Combine(["repo", .. file.Path.Split('/')]),
                    static file => new MockFileData(file.Text)));

        return EngineServices.Reader(fileSystem).Read(fileSystem.Path.GetFullPath("repo"), null);
    }

    private const string Specification = "src/sample/.spec/README.md";
}
