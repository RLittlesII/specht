using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>
/// A synthetic tree in an in-memory file system, read by the model reader under a schema 0.3.0 manifest that declares
/// the epic grammar or declares none, and evaluated by the composed rules (<c>0001-F5</c> B-005, B-006, B-024 to B-032,
/// B-034, B-035, B-042, B-053, B-054, B-069, B-071; decision 0020). Whether a tree has epics is read from the epic grammar
/// alone (C-10): without it a Feature is identified by its <c>id</c>, edges resolve by <c>id</c>, items number per
/// Feature, no epic file is read and the path identity checks the Feature segment alone; with it a Feature carrying no
/// <c>epic</c> keeps its rendered identity and is reported for the missing epic. A pin below 0.3.0 keeps its reading
/// (B-049). A finding is read under the id the evaluator reports at the pin.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtModelReaderEpicsOptInUnitTests
{
    [Theory]
    [ClassData(typeof(EpicsOptInIdentities))]
    public void AFeatureAtZeroThreeZero_WhenTheTreeIsRead_ShouldTakeTheIdentityTheEpicGrammarGives(
        bool epicGrammar,
        string path,
        string frontmatter,
        string identity)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0" };
        if (epicGrammar)
        {
            manifest["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" };
        }

        // When
        var model = Read(manifest, (path, frontmatter));

        // Then
        model.Features.Select(static feature => feature.Identity).Should().Equal(identity);
    }

    [Theory]
    [ClassData(typeof(EpicGrammarDuplicates))]
    public void TwoSpecificationsDeclaringOneIdAndNoEpic_WhenEvaluatedAtZeroThreeZero_ShouldReportSpec3002OnEachUnderTheIdentityTheEpicGrammarGives(
        bool epicGrammar,
        string identity)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0" };
        if (epicGrammar)
        {
            manifest["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" };
        }

        // When
        var violations = Evaluate(Read(
            manifest,
            ("src/first/.spec/README.md", "---\nid: F2\n---\n"),
            ("src/second/.spec/README.md", "---\nid: F2\n---\n")));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC3002")
            .Select(static violation => (violation.File, violation.Line, violation.Identifier))
            .Should()
            .BeEquivalentTo([("src/first/.spec/README.md", 2, identity), ("src/second/.spec/README.md", 2, identity)]);
    }

    [Theory]
    [ClassData(typeof(EpicLessEdges))]
    public void ADependencyEdgeBetweenFeaturesWithNoEpic_WhenTheTreeIsReadAtZeroThreeZero_ShouldResolveAsTheEpicGrammarDecides(
        bool epicGrammar,
        string localEdge,
        string edge,
        string source,
        string[] resolved,
        string[] unresolved)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["forms"] = new JsonObject { ["localEdge"] = localEdge },
        };
        if (epicGrammar)
        {
            manifest["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" };
        }

        // When
        var graph = Read(
            manifest,
            ("src/first/.spec/README.md", $"---\nid: F1\ndepends_on: ['{edge}']\n---\n"),
            ("src/second/.spec/README.md", "---\nid: F2\n---\n")).DependencyGraph;

        // Then
        graph.DependsOn.Should().ContainKey(source).WhoseValue.Should().BeEquivalentTo(resolved);
        graph.UnresolvedDependsOn.Should().ContainKey(source).WhoseValue.Should().BeEquivalentTo(unresolved);
    }

    [Theory]
    [ClassData(typeof(EdgesNamingNoFeature))]
    public void AnEdgeNamingNoFeatureFromAFeatureWithNoEpic_WhenTheTreeIsReadAndEvaluatedWithoutTheEpicGrammar_ShouldBeReportedBySpec6001AsWritten(
        string edge,
        string featureGrammar)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["feature"] = featureGrammar },
        };

        // When
        var violations = Evaluate(Read(
            manifest,
            ("src/first/.spec/README.md", $"---\nid: F1\ndepends_on: ['{edge}']\n---\n"),
            ("src/second/.spec/README.md", "---\nid: F2\n---\n")));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC6001")
            .Select(static violation => (violation.File, violation.Line, violation.Identifier))
            .Should()
            .Equal(("src/first/.spec/README.md", 3, edge));
    }

    [Theory]
    [ClassData(typeof(BareItemParents))]
    public void AnItemWhoseParentIsABareFeatureId_WhenTheTreeIsReadAndEvaluatedAtZeroThreeZero_ShouldBeReportedByTheParentCheckOnlyUnderTheEpicGrammar(
        bool epicGrammar,
        string feature,
        int[] lines)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0" };
        if (epicGrammar)
        {
            manifest["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" };
        }

        // When
        var violations = Evaluate(Read(
            manifest,
            ("src/area/.spec/README.md", feature),
            ("src/area/.spec/0001-01-item.md", "---\nid: '0001-01'\nparent: F2\ntype: task\n---\n")));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC5003")
            .Select(static violation => violation.Line)
            .Should()
            .Equal(lines);
    }

    [Fact]
    public void ItemsWithAThreeDigitSequenceWithoutTheEpicGrammar_WhenTheTreeIsReadAndEvaluated_ShouldBeReadWholeAndContiguous()
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["task"] = "^(?<epic>[0-9]{4})-(?<sequence>[0-9]{3})$" },
        };

        // When
        var violations = Evaluate(Read(
            manifest,
            ("src/area/.spec/README.md", "---\nid: F1\n---\n"),
            ("src/area/.spec/0001-001-item.md", "---\nid: '0001-001'\nparent: F1\ntype: task\n---\n"),
            ("src/area/.spec/0001-002-item.md", "---\nid: '0001-002'\nparent: F1\ntype: task\n---\n")));

        // Then
        violations.Should().NotContain(static violation => violation.RuleId == "SPEC5003" || violation.RuleId == "SPEC5004");
    }

    [Theory]
    [ClassData(typeof(PerFeatureItems))]
    public void ItemsBesideTwoFeaturesWithoutTheEpicGrammar_WhenTheTreeIsReadAndEvaluated_ShouldBeSequencedPerFeature(
        string task,
        string[] first,
        string[] second,
        string reported)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["task"] = task },
        };
        (string Path, string Text)[] files =
        [
            ("src/first/.spec/README.md", "---\nid: F1\n---\n"),
            ("src/second/.spec/README.md", "---\nid: F2\n---\n"),
            .. first.Select(static id => ($"src/first/.spec/{id}-item.md", $"---\nid: '{id}'\nparent: F1\ntype: task\n---\n")),
            .. second.Select(static id => ($"src/second/.spec/{id}-item.md", $"---\nid: '{id}'\nparent: F2\ntype: task\n---\n")),
        ];

        // When
        var violations = Evaluate(Read(manifest, files));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC5004")
            .Select(static violation => (violation.File, violation.Identifier))
            .Should()
            .Equal(($"src/second/.spec/{reported}-item.md", reported));
    }

    [Theory]
    [ClassData(typeof(SequenceGaps))]
    public void ASequenceGapWithoutTheEpicGrammar_WhenTheTreeIsReadAndEvaluated_ShouldNameTheFeatureAndTheMissingNumberAtTheCapturedWidth(
        string task,
        string first,
        string second,
        string missing)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["task"] = task },
        };

        // When
        var violations = Evaluate(Read(
            manifest,
            ("src/area/.spec/README.md", "---\nid: F1\n---\n"),
            ($"src/area/.spec/{first}-item.md", $"---\nid: '{first}'\nparent: F1\ntype: task\n---\n"),
            ($"src/area/.spec/{second}-item.md", $"---\nid: '{second}'\nparent: F1\ntype: task\n---\n")));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC5004")
            .Select(static violation => (violation.Identifier, violation.Message))
            .Should()
            .Equal((second, $"Feature F1's item sequence skips {missing} - tasks are numbered per Feature, contiguously from 01"));
    }

    [Theory]
    [ClassData(typeof(UncheckedEpicFiles))]
    public void AnEpicFileWithoutTheEpicGrammarAtZeroThreeZero_WhenTheTreeIsReadAndEvaluated_ShouldBeReportedByNeitherFrontmatterRule(string epic)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0" };

        // When
        var violations = Evaluate(Read(
            manifest,
            ("src/area/.spec/README.md", "---\nid: F1\n---\n"),
            ("epics/0001-epic/epic.md", epic)));

        // Then
        violations.Should().NotContain(static violation => violation.File == "epics/0001-epic/epic.md"
            && (violation.RuleId == "SPEC1001" || violation.RuleId == "SPEC1004"));
    }

    [Theory]
    [ClassData(typeof(OlderPins))]
    public void AnOlderPinWithoutTheEpicGrammar_WhenTheTreeIsReadAndEvaluated_ShouldKeepTheEpicInTheIdentityAndCheckEpicFiles(string pin)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = pin };

        // When
        var model = Read(
            manifest,
            ("src/area/.spec/README.md", "---\nepic: '0001'\nid: F2\n---\n"),
            ("epics/0001-epic/epic.md", "---\nid: '0001'\ntype: feature\n---\n# Epic\n"));

        // Then
        model.Features.Select(static feature => feature.Identity).Should().Equal("0001-F2");
        Evaluate(model).Should().Contain(static violation => violation.RuleId == "SPEC004" && violation.File == "epics/0001-epic/epic.md");
    }

    [Theory]
    [ClassData(typeof(MissingEpicLayouts))]
    public void ASpecificationWithNoEpicUnderTheEpicGrammar_WhenTheTreeIsReadAndEvaluatedAtZeroThreeZero_ShouldBeReportedBySpec3001AtItsIdLine(
        string path,
        JsonObject expected)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.0",
            ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" },
        };

        // When
        var violations = Evaluate(Read(manifest, (path, "---\nid: F2\n---\n")));

        // Then
        var finding = violations.Where(static violation => violation.RuleId == "SPEC3001").Should().ContainSingle().Subject;
        (finding.File, finding.Line, finding.Identifier, finding.Message).Should().Be(
            (path, 2, "-F2", "frontmatter has no epic - the manifest declares the epic grammar"));
        JsonNode.DeepEquals(finding.Expected, expected).Should().BeTrue(finding.Expected?.ToJsonString());
    }

    [Fact]
    public void SpecificationsInTheEpicsLayoutWithoutTheEpicGrammar_WhenTheTreeIsReadAndEvaluated_ShouldBeCheckedOnTheFeatureSegmentAlone()
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0" };

        // When
        var violations = Evaluate(Read(
            manifest,
            ("epics/misc-group/F1-feature/spec.md", "---\nepic: '0001'\nid: F1\n---\n"),
            ("epics/0001-epic/F9-feature/spec.md", "---\nepic: '0001'\nid: F2\n---\n")));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC3001")
            .Select(static violation => (violation.File, violation.Line))
            .Should()
            .Equal(("epics/0001-epic/F9-feature/spec.md", 3));
    }

    private static SpechtModel Read(JsonObject manifest, params (string Path, string Text)[] files)
    {
        var fileSystem = new MockFileSystem(
            files.Append((Path: SpechtTree.ManifestPath, Text: manifest.ToJsonString()))
                .ToDictionary(
                    static file => Path.Combine(["repo", .. file.Path.Split('/')]),
                    static file => new MockFileData(file.Text)));

        return EngineServices.Reader(fileSystem).Read(fileSystem.Path.GetFullPath("repo"), null);
    }

    private static IReadOnlyList<SpechtViolation> Evaluate(SpechtModel model) =>
        new SpechtEvaluator(EngineServices.Rules(), new SpechtViolationOrder()).Evaluate(model).Violations;
}
