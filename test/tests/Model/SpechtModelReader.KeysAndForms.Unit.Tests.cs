using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>
/// A synthetic tree in an in-memory file system, read by the model reader under a manifest's key roles, forms and task
/// grammar and evaluated by the composed rules (<c>0001-F5</c> B-004 to B-006, B-049, B-053, B-054; decisions 0010, 0016,
/// 0017 and 0018). Under schema 0.3.0 each key a rule reads is read by the name the manifest gives its kind and role, at
/// that key's line; an edge resolves through the declared forms, and one fitting neither is reported as written; an
/// item's epic and sequence come from the task grammar's named groups, and a gap is padded to the captured width. A pin
/// below 0.3.0 keeps the reading it has today. Each 0.3.0 manifest declares the epic grammar, the case decision 0018
/// leaves to this item. A finding is read under the id the evaluator reports at the pin.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtModelReaderKeysAndFormsUnitTests
{
    [Theory]
    [ClassData(typeof(RenamedFrontmatterKeys))]
    public void AKeyTheManifestRenames_WhenTheTreeIsReadAndEvaluated_ShouldBeReadByTheManifestsNameAtItsOwnLine(
        string keys,
        string feature,
        string item,
        string ruleId,
        string identifier,
        string file,
        int line)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" },
            ["keys"] = JsonNode.Parse(keys),
        };

        // When
        var violations = Check(manifest, ("src/area/.spec/README.md", feature), ("src/area/.spec/0001-01-item.md", item));

        // Then
        violations.Where(violation => violation.RuleId == ruleId)
            .Select(static violation => (violation.Identifier, violation.File, violation.Line))
            .Should()
            .Equal((identifier, file, line));
    }

    [Theory]
    [ClassData(typeof(DeclaredEdgeForms))]
    public void AnEdgeWrittenInADeclaredForm_WhenTheTreeIsReadAndEvaluated_ShouldResolveToTheFeatureItNames(
        string forms,
        string edge,
        string targetEpic,
        string targetIdentity)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" },
            ["forms"] = JsonNode.Parse(forms),
        };

        // When
        var violations = Check(
            manifest,
            ("src/first/.spec/README.md", $"---\nepic: '0001'\nid: F1\ndepends_on: ['{edge}']\n---\n"),
            ("src/second/.spec/README.md", $"---\nepic: '{targetEpic}'\nid: F2\n---\n"));

        // Then
        violations.Should().NotContain(static violation => violation.RuleId == "SPEC6001");
        violations.Where(static violation => violation.RuleId == "SPEC6002")
            .Select(static violation => (violation.File, violation.Line, violation.Identifier))
            .Should()
            .Equal(("src/first/.spec/README.md", 4, targetIdentity));
    }

    [Fact]
    public void AnItemParentWrittenInTheDeclaredIdentityForm_WhenTheTreeIsReadAndEvaluated_ShouldMatchTheFeatureItSitsIn()
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" },
            ["forms"] = new JsonObject { ["identity"] = "{epic}.{id}" },
        };

        // When
        var violations = Check(
            manifest,
            ("src/area/.spec/README.md", "---\nepic: '0001'\nid: F1\n---\n"),
            ("src/area/.spec/0001-01-item.md", "---\nid: '0001-01'\nparent: '0001.F1'\ntype: task\n---\n"));

        // Then
        violations.Should().NotContain(static violation => violation.RuleId == "SPEC5003");
    }

    [Fact]
    public void ItemsWithAThreeDigitSequenceTheTaskGrammarNames_WhenTheTreeIsReadAndEvaluated_ShouldBeReadWholeAndContiguous()
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$", ["task"] = "^(?<epic>[0-9]{4})-(?<sequence>[0-9]{3})$" },
        };

        // When
        var violations = Check(
            manifest,
            ("src/area/.spec/README.md", "---\nepic: '0001'\nid: F1\n---\n"),
            ("src/area/.spec/0001-001-item.md", "---\nid: '0001-001'\nparent: 0001-F1\ntype: task\n---\n"),
            ("src/area/.spec/0001-002-item.md", "---\nid: '0001-002'\nparent: 0001-F1\ntype: task\n---\n"));

        // Then
        violations.Should().NotContain(static violation => violation.RuleId == "SPEC5003" || violation.RuleId == "SPEC5004");
    }

    [Fact]
    public void ItemsWhoseEpicTheTaskGrammarCapturesAfterTheSequence_WhenTheTreeIsReadAndEvaluated_ShouldBeSequencedWithinThatEpic()
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$", ["task"] = "^T(?<sequence>[0-9]{2})-(?<epic>[0-9]{4})$" },
        };

        // When
        var violations = Check(
            manifest,
            ("src/area/.spec/README.md", "---\nepic: '0001'\nid: F1\n---\n"),
            ("src/area/.spec/T01-0001-item.md", "---\nid: 'T01-0001'\nparent: 0001-F1\ntype: task\n---\n"),
            ("src/area/.spec/T03-0001-item.md", "---\nid: 'T03-0001'\nparent: 0001-F1\ntype: task\n---\n"));

        // Then
        violations.Should().NotContain(static violation => violation.RuleId == "SPEC5003");
        violations.Where(static violation => violation.RuleId == "SPEC5004")
            .Should()
            .ContainSingle()
            .Which.Should()
            .Match<SpechtViolation>(static violation => violation.Identifier == "T03-0001"
                && violation.File == "src/area/.spec/T03-0001-item.md"
                && violation.Message.Contains("0001", StringComparison.Ordinal));
    }

    [Theory]
    [ClassData(typeof(SequenceGaps))]
    public void ASequenceGapAtZeroThreeZero_WhenTheTreeIsReadAndEvaluated_ShouldNameTheMissingNumberAtTheCapturedWidth(
        string task,
        string first,
        string second,
        string missing)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$", ["task"] = task },
        };

        // When
        var violations = Check(
            manifest,
            ("src/area/.spec/README.md", "---\nepic: '0001'\nid: F1\n---\n"),
            ($"src/area/.spec/{first}-item.md", $"---\nid: '{first}'\nparent: 0001-F1\ntype: task\n---\n"),
            ($"src/area/.spec/{second}-item.md", $"---\nid: '{second}'\nparent: 0001-F1\ntype: task\n---\n"));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC5004")
            .Should()
            .ContainSingle()
            .Which.Should()
            .Match<SpechtViolation>(violation => violation.Identifier == second
                && Regex.IsMatch(violation.Message, $"(?<![0-9]){missing}(?![0-9])"));
    }

    [Fact]
    public void AnEdgeFittingNeitherFormAtZeroThreeZero_WhenTheTreeIsReadAndEvaluated_ShouldBeReportedUnresolvedByItsOriginalText()
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$", ["feature"] = "^F[0-9]+$" },
        };

        // When
        var violations = Check(manifest, ("src/area/.spec/README.md", "---\nepic: '0001'\nid: F1\ndepends_on: ['F2x']\n---\n"));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC6001")
            .Select(static violation => (violation.File, violation.Line, violation.Identifier))
            .Should()
            .Equal(("src/area/.spec/README.md", 4, "F2x"));
    }

    [Theory]
    [ClassData(typeof(OlderPins))]
    public void AnOlderPinWithAGrouplessTaskGrammarAMalformedEdgeAndAThreeDigitSequence_WhenTheTreeIsReadAndEvaluated_ShouldKeepTodaysVerdict(
        string pin)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = pin,
            ["identifiers"] = new JsonObject { ["task"] = "^[0-9]{4}-[0-9]{3}$", ["feature"] = "^F[0-9]+$" },
        };

        // When
        var violations = Check(
            manifest,
            ("src/area/.spec/README.md", "---\nepic: '0001'\nid: F1\ndepends_on: ['F2x']\n---\n"),
            ("src/area/.spec/0001-001-item.md", "---\nid: '0001-001'\nparent: 0001-F1\ntype: task\n---\n"),
            ("src/area/.spec/0001-003-item.md", "---\nid: '0001-003'\nparent: 0001-F1\ntype: task\n---\n"));

        // Then
        violations.Where(static violation => violation.RuleId is "SPEC043" or "SPEC044" or "SPEC050")
            .Select(static violation => (violation.RuleId, violation.File, violation.Identifier))
            .Should()
            .Equal(
                ("SPEC043", "src/area/.spec/0001-001-item.md", "0001-001"),
                ("SPEC043", "src/area/.spec/0001-003-item.md", "0001-003"),
                ("SPEC050", "src/area/.spec/README.md", "0001-F2x"));
    }

    private static IReadOnlyList<SpechtViolation> Check(JsonObject manifest, params (string Path, string Text)[] files)
    {
        var fileSystem = new MockFileSystem(
            files.Append((Path: SpechtTree.ManifestPath, Text: manifest.ToJsonString()))
                .ToDictionary(
                    static file => Path.Combine(["repo", .. file.Path.Split('/')]),
                    static file => new MockFileData(file.Text)));
        var model = EngineServices.Reader(fileSystem).Read(fileSystem.Path.GetFullPath("repo"), null);

        return new SpechtEvaluator(EngineServices.Rules(), new SpechtViolationOrder()).Evaluate(model).Violations;
    }
}
