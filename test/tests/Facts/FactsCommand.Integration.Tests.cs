using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Json.Schema;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Facts;

/// <summary>
/// The built tool's <c>specht facts --json</c> over synthetic trees on disk (<c>0132-F2</c> B-001 to B-008, B-011 to
/// B-015, B-017 to B-023, B-025, B-026), launched as a shell launches it, with or without a root-relative path, its
/// output validated against the published facts schema at <c>docs/schema/facts.schema.json</c> (OQ-1's default), copied
/// into the output from that one file.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class FactsCommandIntegrationTests
{
    [Fact]
    public void FeaturesInBothLayouts_WhenFactsAreGivenAsJson_ShouldListEverySpecificationInTheChecksDiscoveryOrder()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/zeta", "0001", "F2");
        tree.WriteFeature("0001", "F1");
        tree.WriteCoLocatedFeature("src/alpha", "0001", "F3");
        var discovered = EngineServices.Reader().Read(tree.Root, null).Features.Select(static feature => feature.RelativePath).ToArray();

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        discovered.Should().HaveCount(3);
        JsonNode.Parse(Stdout)!["specifications"]!.AsArray().Select(static specification => specification!["path"]!.GetValue<string>())
            .Should().Equal(discovered);
    }

    [Fact]
    public void FeaturesWithAndWithoutAnIdentity_WhenFactsAreGivenAsJson_ShouldWriteOneDocumentValidAgainstThePublishedFactsSchema()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/first", "0001", "F1", new Dictionary<string, string> { ["depends_on"] = "[\"F2\"]" });
        tree.WriteCoLocatedFeature("src/second", "0001", "F2", new Dictionary<string, string> { ["blocks"] = "[\"F1\"]" });
        tree.WriteSpecification("src/third/.spec/README.md", "0001", "F3", new Dictionary<string, string> { ["epic"] = "null", ["spec_status"] = "null" });

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        using var document = JsonDocument.Parse(Stdout);
        document.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        var result = Evaluate(document.RootElement);
        result.IsValid.Should().BeTrue(Errors(result));
    }

    [Fact]
    public void AFeature_WhenFactsAreGivenAsJson_ShouldCarryItsRootRelativePathQualifiedIdAndBothStatuses()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/facts/area", "0132", "F2", new Dictionary<string, string>
        {
            ["spec_status"] = "approved",
            ["status"] = "in-progress",
        });

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        var specification = JsonNode.Parse(Stdout)!["specifications"]!.AsArray().Should().ContainSingle().Which!;
        specification["path"]!.GetValue<string>().Should().Be("src/facts/area/.spec/README.md");
        specification["id"]!.GetValue<string>().Should().Be("0132-F2");
        specification["specStatus"]!.GetValue<string>().Should().Be("approved");
        specification["status"]!.GetValue<string>().Should().Be("in-progress");
    }

    [Fact]
    public void FeaturesThatListEachOther_WhenFactsAreGivenAsJson_ShouldCarryEachListAsTheFrontmatterWritesIt()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/first", "0001", "F1", new Dictionary<string, string>
        {
            ["depends_on"] = "[\"F2\", \"F9\"]",
            ["children"] = "[\"0400\"]",
        });
        tree.WriteCoLocatedFeature("src/second", "0001", "F2", new Dictionary<string, string> { ["blocks"] = "[\"F1\"]" });

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        var specifications = JsonNode.Parse(Stdout)!["specifications"]!.AsArray()
            .ToDictionary(static specification => specification!["path"]!.GetValue<string>(), static specification => specification!);
        var first = specifications["src/first/.spec/README.md"];
        var second = specifications["src/second/.spec/README.md"];
        first["dependsOn"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("F2", "F9");
        first["blocks"]!.AsArray().Should().BeEmpty();
        first["children"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("0400");
        second["dependsOn"]!.AsArray().Should().BeEmpty();
        second["blocks"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("F1");
        second["children"]!.AsArray().Should().BeEmpty();
    }

    [Fact]
    public void ATreeTheCheckReportsAViolationFor_WhenFactsAreGivenAsJson_ShouldCarryNoViolationCountOrVerdict()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/area", "0001", "F1", new Dictionary<string, string> { ["spec_status"] = "nearly" });
        var check = AcceptanceTool.Launch(tree.Root, "--json");
        check.ExitCode.Should().Be(1, check.Stderr);
        var violations = JsonNode.Parse(check.Stdout)!["violations"]!.AsArray();
        violations.Should().NotBeEmpty();

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        MemberPaths(JsonNode.Parse(Stdout), string.Empty).Select(static path => path[(path.LastIndexOf('.') + 1)..]).Should()
            .NotContain(["violations", "errorCount", "warningCount", "rulesEvaluated", "itemCount", "ruleId", "severity"]);
        foreach (var violation in violations)
        {
            Stdout.Should().NotContain(violation!["ruleId"]!.GetValue<string>());
            Stdout.Should().NotContain(violation["message"]!.GetValue<string>());
        }
    }

    [Theory]
    [ClassData(typeof(FactsVerdictMembers))]
    public void AFactsDocumentGivenAVerdictMember_WhenValidatedAgainstThePublishedFactsSchema_ShouldBeRejected(
        bool onSpecification, string member, string value)
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/area", "0001", "F1");
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");
        ExitCode.Should().Be(0, Stderr);
        var document = JsonNode.Parse(Stdout)!;
        using (var given = JsonDocument.Parse(document.ToJsonString()))
        {
            var unchanged = Evaluate(given.RootElement);
            unchanged.IsValid.Should().BeTrue(Errors(unchanged));
        }

        var target = onSpecification ? document["specifications"]![0]!.AsObject() : document.AsObject();
        target[member] = JsonNode.Parse(value);

        // When
        using var json = JsonDocument.Parse(document.ToJsonString());
        var result = Evaluate(json.RootElement);

        // Then
        result.IsValid.Should().BeFalse($"a {member} member on the {(onSpecification ? "specification" : "document")} is not a fact");
    }

    [Theory]
    [ClassData(typeof(FactsPinPairs))]
    public void TheSameSpecificationsUnderTwoShippedPins_WhenFactsAreGivenAsJson_ShouldCarryTheSameMembers(string firstPin, string secondPin)
    {
        // Given
        using var first = new SpechtTree();
        using var second = new SpechtTree();
        first.Pin(firstPin);
        second.Pin(secondPin);
        foreach (var tree in new[] { first, second })
        {
            tree.WriteCoLocatedFeature("src/first", "0001", "F1", new Dictionary<string, string> { ["depends_on"] = "[\"F2\"]" });
            tree.WriteCoLocatedFeature("src/second", "0001", "F2", new Dictionary<string, string> { ["blocks"] = "[\"F1\"]", ["status"] = "null" });
            tree.WriteSpecification("src/third/.spec/README.md", "0001", "F3", new Dictionary<string, string> { ["epic"] = "null" });
        }

        // When
        var runs = new[] { first, second }.Select(static tree => AcceptanceTool.Launch(tree.Root, "facts", "--json")).ToArray();

        // Then
        runs.Should().AllSatisfy(static run => run.ExitCode.Should().Be(0, run.Stderr));
        var members = runs
            .Select(static run => MemberPaths(JsonNode.Parse(run.Stdout), string.Empty).Distinct().Order(StringComparer.Ordinal).ToArray())
            .ToArray();
        members[0].Should().Contain("specifications[].path");
        members[1].Should().Equal(members[0]);
    }

    [Theory]
    [ClassData(typeof(FactsBrokenRoots))]
    public void ABrokenRoot_WhenFactsAreGiven_ShouldExitWithTheCodeTheCheckGives(bool rootExists, string? manifest, int exitCode)
    {
        // Given
        using var tree = new SpechtTree();
        if (manifest is null)
        {
            File.Delete(Path.Combine(tree.Root, SpechtTree.ManifestPath));
        }
        else
        {
            tree.WriteRaw(SpechtTree.ManifestPath, manifest);
        }

        var root = rootExists ? "." : "absent";
        var check = AcceptanceTool.Launch(tree.Root, "--root", root);

        // When
        var (_, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--root", root, "--json");

        // Then
        check.ExitCode.Should().Be(exitCode, check.Stderr);
        ExitCode.Should().Be(exitCode, Stderr);
    }

    [Fact]
    public void TheSameTreeUnderTwoRoots_WhenFactsAreGivenAsJsonTwice_ShouldGiveByteIdenticalOutput()
    {
        // Given
        using var first = new SpechtTree();
        using var second = new SpechtTree();
        foreach (var tree in new[] { first, second })
        {
            tree.Pin("0.2.0");
            tree.WriteCoLocatedFeature("src/first", "0001", "F1", new Dictionary<string, string> { ["depends_on"] = "[\"F2\"]" });
            tree.WriteFeature("0001", "F2", new Dictionary<string, string> { ["blocks"] = "[\"F1\"]" });
        }

        // When
        var runs = new[] { first, first, second }.Select(static tree => AcceptanceTool.Launch(tree.Root, "facts", "--json")).ToArray();

        // Then
        runs.Should().AllSatisfy(static run => run.ExitCode.Should().Be(0, run.Stderr));
        runs[0].Stdout.Should().NotBeEmpty();
        runs[1].Stdout.Should().Be(runs[0].Stdout);
        runs[2].Stdout.Should().Be(runs[0].Stdout);
    }

    [Fact]
    public void ATreeBesideOtherFiles_WhenFactsAreGivenAsJson_ShouldCreateModifyOrDeleteNoFileUnderTheRootOrBesideIt()
    {
        // Given
        using var tree = new SpechtTree(["repo"]);
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/area", "0001", "F1");
        var outside = Path.GetDirectoryName(tree.Root)!;
        File.WriteAllText(Path.Combine(outside, "sentinel.txt"), "unchanged");
        var before = Directory.GetFiles(outside, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(outside, path).Replace('\\', '/'), static path => File.ReadAllBytes(path));

        // When
        var (_, Stderr, ExitCode) = AcceptanceTool.Launch(outside, "facts", "--root", "repo", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        Directory.GetFiles(outside, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(outside, path).Replace('\\', '/'), static path => File.ReadAllBytes(path))
            .Should().BeEquivalentTo(before);
    }

    [Fact]
    public void ATreeWithAnEpicFileTheCheckDiscovers_WhenFactsAreGivenAsJson_ShouldGiveTheFeaturesAndNotTheEpic()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteEpic("0001");
        tree.WriteFeature("0001", "F1");
        tree.WriteCoLocatedFeature("src/area", "0001", "F2");
        var model = EngineServices.Reader().Read(tree.Root, null);
        model.Epics.Select(static epic => epic.RelativePath).Should().Equal("epics/0001-epic/epic.md");

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        JsonNode.Parse(Stdout)!["specifications"]!.AsArray().Select(static specification => specification!["path"]!.GetValue<string>())
            .Should().Equal(model.Features.Select(static feature => feature.RelativePath)).And.NotContain("epics/0001-epic/epic.md");
    }

    [Fact]
    public void AFeatureWithClaims_WhenFactsAreGivenAsJson_ShouldCarryEachClaimsIdTextStatusAndTheLineItIsOn()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        var path = tree.WriteCoLocatedFeature("src/area", "0001", "F1", sections: SpechtTree.SectionsWith(
            "3. Acceptance Criteria",
            "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                + "| B-001 | Given a tree, this Feature gives its facts. | the owner | Active |\n"
                + "| B-002 | Given a flag, this Feature writes one document. | the owner | Proposed |\n"));
        var lines = File.ReadAllLines(path);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        var specification = JsonNode.Parse(Stdout)!["specifications"]!.AsArray().Should().ContainSingle().Which!;
        specification["claims"].Should().BeOfType<JsonArray>().Which.Select(static claim => (
                claim!["id"]!.GetValue<string>(),
                claim["text"]!.GetValue<string>(),
                claim["status"]!.GetValue<string>(),
                claim["line"]!.GetValue<int>()))
            .Should().Equal(
                ("B-001", "Given a tree, this Feature gives its facts.", "Active", LineOf(lines, "| B-001 |")),
                ("B-002", "Given a flag, this Feature writes one document.", "Proposed", LineOf(lines, "| B-002 |")));
    }

    [Fact]
    public void AFeatureWithConstraints_WhenFactsAreGivenAsJson_ShouldCarryEachConstraintsIdTextWhatItRulesOutAndTheLineItIsOn()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        var path = tree.WriteCoLocatedFeature("src/area", "0001", "F1", sections: SpechtTree.SectionsWith(
            "4. Constraints",
            "## 4. Constraints\n\n| ID | Constraint | Rules Out |\n| -- | ---------- | --------- |\n"
                + "| C-1 | Offline and read-only. | A network call. |\n"
                + "| C-2 | One parser. | A second parser. |\n"));
        var lines = File.ReadAllLines(path);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        var specification = JsonNode.Parse(Stdout)!["specifications"]!.AsArray().Should().ContainSingle().Which!;
        specification["constraints"].Should().BeOfType<JsonArray>().Which.Select(static constraint => (
                constraint!["id"]!.GetValue<string>(),
                constraint["text"]!.GetValue<string>(),
                constraint["rulesOut"]!.GetValue<string>(),
                constraint["line"]!.GetValue<int>()))
            .Should().Equal(
                ("C-1", "Offline and read-only.", "A network call.", LineOf(lines, "| C-1 |")),
                ("C-2", "One parser.", "A second parser.", LineOf(lines, "| C-2 |")));
    }

    [Theory]
    [ClassData(typeof(FactsRetiredClaimStatuses))]
    public void AClaimWhoseStatusIsTheWithdrawnOrSupersededMarker_WhenFactsAreGivenAsJson_ShouldStillGiveItWithThatStatus(string status)
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        var path = tree.WriteCoLocatedFeature("src/area", "0001", "F1", sections: SpechtTree.SectionsWith(
            "3. Acceptance Criteria",
            "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                + "| B-001 | It does the thing. | brd | Active |\n"
                + $"| B-002 | It did the other thing. | brd | {status} |\n"));
        var lines = File.ReadAllLines(path);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        JsonNode.Parse(Stdout)!["specifications"]![0]!["claims"].Should().BeOfType<JsonArray>().Which
            .Select(static claim => (claim!["id"]!.GetValue<string>(), claim["status"]!.GetValue<string>(), claim["line"]!.GetValue<int>()))
            .Should().Equal(("B-001", "Active", LineOf(lines, "| B-001 |")), ("B-002", status, LineOf(lines, "| B-002 |")));
    }

    [Fact]
    public void TwoClaimsSharingAnIdTheCheckReports_WhenFactsAreGivenAsJson_ShouldExitZeroAndGiveBoth()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        var path = tree.WriteCoLocatedFeature("src/area", "0001", "F1", sections: SpechtTree.SectionsWith(
            "3. Acceptance Criteria",
            "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                + "| B-001 | It does the thing. | brd | Active |\n"
                + "| B-001 | It does the thing twice. | brd | Active |\n"));
        var lines = File.ReadAllLines(path);
        var first = LineOf(lines, "| B-001 | It does the thing. |");
        var second = LineOf(lines, "| B-001 | It does the thing twice. |");
        var check = AcceptanceTool.Launch(tree.Root, "--json");
        check.ExitCode.Should().Be(1, check.Stderr);
        JsonNode.Parse(check.Stdout)!["violations"]!.AsArray().Select(static violation => violation!["line"]!.GetValue<int>()).Should().Contain(second);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        JsonNode.Parse(Stdout)!["specifications"]![0]!["claims"].Should().BeOfType<JsonArray>().Which
            .Select(static claim => (claim!["id"]!.GetValue<string>(), claim["text"]!.GetValue<string>(), claim["line"]!.GetValue<int>()))
            .Should().Equal(("B-001", "It does the thing.", first), ("B-001", "It does the thing twice.", second));
    }

    [Fact]
    public void AClaimWithATraceabilityRowAndATaggedScenario_WhenFactsAreGivenAsJson_ShouldCarryNoRowScenarioOrTest()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        var path = tree.WriteCoLocatedFeature("src/area", "0001", "F1", sections: SpechtTree.SectionsWith(
            "9. Traceability Matrix",
            "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n"
                + "| B-001 | Crossing the zebra | `ZebraTests.Crossing` | Covered |\n"));
        tree.WriteFeatureFile(path, "Feature: it\n\n  @B-001\n  Scenario: Crossing the zebra\n    Given a thing\n");
        var check = AcceptanceTool.Launch(tree.Root, "--json");
        check.ExitCode.Should().Be(0, check.Stdout);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        JsonNode.Parse(Stdout)!["specifications"]![0]!["claims"].Should().BeOfType<JsonArray>().Which.Should().ContainSingle()
            .Which!["id"]!.GetValue<string>().Should().Be("B-001");
        Stdout.Should().NotContainEquivalentOf("zebra").And.NotContain("Covered");
    }

    [Theory]
    [ClassData(typeof(FactsCoverageMembers))]
    public void AClaimGivenACoverageMember_WhenValidatedAgainstThePublishedFactsSchema_ShouldBeRejected(string member, string value)
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/area", "0001", "F1");
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");
        ExitCode.Should().Be(0, Stderr);
        var document = JsonNode.Parse(Stdout)!;
        var claim = document["specifications"]![0]!["claims"].Should().BeOfType<JsonArray>().Which.Should().ContainSingle().Which!.AsObject();
        using (var given = JsonDocument.Parse(document.ToJsonString()))
        {
            var unchanged = Evaluate(given.RootElement);
            unchanged.IsValid.Should().BeTrue(Errors(unchanged));
        }

        claim[member] = JsonNode.Parse(value);

        // When
        using var json = JsonDocument.Parse(document.ToJsonString());
        var result = Evaluate(json.RootElement);

        // Then
        result.IsValid.Should().BeFalse($"a {member} member on a claim is coverage, not a fact");
    }

    [Fact]
    public void AFeatureWithOutOfScopeRows_WhenFactsAreGivenAsJson_ShouldCarryEachRowsNumberItemReasonAndTheLineItIsOn()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        var path = tree.WriteCoLocatedFeature("src/area", "0001", "F1", sections: SpechtTree.SectionsWith(
            "5. Out of Scope",
            "## 5. Out of Scope\n\n| # | Item | Exclusion Reason |\n| - | ---- | ---------------- |\n"
                + "| 1 | Any other input | Owned by a sibling Feature |\n"
                + "| 2 | Any write | Not asked for |\n"));
        var lines = File.ReadAllLines(path);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        using (var document = JsonDocument.Parse(Stdout))
        {
            var result = Evaluate(document.RootElement);
            result.IsValid.Should().BeTrue(Errors(result));
        }

        JsonNode.Parse(Stdout)!["specifications"]!.AsArray().Should().ContainSingle().Which!["outOfScope"].Should().BeOfType<JsonArray>().Which
            .Select(static row => (
                row!["number"]!.GetValue<string>(),
                row["item"]!.GetValue<string>(),
                row["reason"]!.GetValue<string>(),
                row["line"]!.GetValue<int>()))
            .Should().Equal(
                ("1", "Any other input", "Owned by a sibling Feature", LineOf(lines, "| 1 | Any other input |")),
                ("2", "Any write", "Not asked for", LineOf(lines, "| 2 | Any write |")));
    }

    [Theory]
    [ClassData(typeof(FactsOpenQuestionResolutions))]
    public void AnOpenQuestionsResolution_WhenFactsAreGivenAsJson_ShouldCarryItsIdQuestionBlocksResolutionLineAndWhetherItIsResolved(
        string resolution, bool resolved)
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        var path = tree.WriteCoLocatedFeature("src/area", "0001", "F1", sections: SpechtTree.SectionsWith(
            "11. Open Questions",
            "## 11. Open Questions\n\n| ID | Question | Blocks | Resolution |\n| -- | -------- | ------ | ---------- |\n"
                + "| OQ-1 | Which output comes last? | B-001 | Resolved 2026-10-30 by the owner: the last |\n"
                + $"| OQ-2 | Which input comes first? | B-001 | {resolution} |\n"));
        var lines = File.ReadAllLines(path);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        using (var document = JsonDocument.Parse(Stdout))
        {
            var result = Evaluate(document.RootElement);
            result.IsValid.Should().BeTrue(Errors(result));
        }

        JsonNode.Parse(Stdout)!["specifications"]!.AsArray().Should().ContainSingle().Which!["openQuestions"].Should().BeOfType<JsonArray>().Which
            .Select(static question => (
                question!["id"]!.GetValue<string>(),
                question["question"]!.GetValue<string>(),
                question["blocks"]!.GetValue<string>(),
                question["resolution"]!.GetValue<string>(),
                question["resolved"]!.GetValue<bool>(),
                question["line"]!.GetValue<int>()))
            .Should().Equal(
                ("OQ-1", "Which output comes last?", "B-001", "Resolved 2026-10-30 by the owner: the last", true, LineOf(lines, "| OQ-1 |")),
                ("OQ-2", "Which input comes first?", "B-001", resolution, resolved, LineOf(lines, "| OQ-2 |")));
    }

    [Theory]
    [ClassData(typeof(FactsOutOfScopeFaults))]
    public void AnOutOfScopeSectionMissingOrWithATableTheParseCannotRead_WhenFactsAreGivenAsJson_ShouldGiveNoOutOfScopeRowsAndEveryOtherRow(
        bool sectionPresent)
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        var sections = SpechtTree.SectionsWith(
                "11. Open Questions",
                "## 11. Open Questions\n\n| ID | Question | Blocks | Resolution |\n| -- | -------- | ------ | ---------- |\n"
                    + "| OQ-1 | Which input comes first? | B-001 | Open |\n")
            .Select(static section => section.StartsWith("## 5. Out of Scope\n", StringComparison.Ordinal)
                ? "## 5. Out of Scope\n\n| # | Item | Exclusion Reason |\n| 1 | Other things | Owned elsewhere |\n"
                : section)
            .Where(section => sectionPresent || !section.StartsWith("## 5. Out of Scope\n", StringComparison.Ordinal))
            .ToList();
        var path = tree.WriteCoLocatedFeature("src/area", "0001", "F1", sections: sections);
        var lines = File.ReadAllLines(path);
        var read = EngineServices.Reader().Read(tree.Root, null).Features.Should().ContainSingle().Which.Document.Section("5. Out of Scope");
        if (sectionPresent)
        {
            read.Should().NotBeNull();
            read!.Rows.Should().BeEmpty();
        }
        else
        {
            read.Should().BeNull();
        }

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        var specification = JsonNode.Parse(Stdout)!["specifications"]!.AsArray().Should().ContainSingle().Which!;
        specification["outOfScope"].Should().BeOfType<JsonArray>().Which.Should().BeEmpty();
        specification["claims"].Should().BeOfType<JsonArray>().Which
            .Select(static claim => (claim!["id"]!.GetValue<string>(), claim["line"]!.GetValue<int>()))
            .Should().Equal(("B-001", LineOf(lines, "| B-001 | It does the thing. |")));
        specification["constraints"].Should().BeOfType<JsonArray>().Which
            .Select(static constraint => (constraint!["id"]!.GetValue<string>(), constraint["line"]!.GetValue<int>()))
            .Should().Equal(("C-1", LineOf(lines, "| C-1 |")));
        specification["openQuestions"].Should().BeOfType<JsonArray>().Which
            .Select(static question => (question!["id"]!.GetValue<string>(), question["line"]!.GetValue<int>()))
            .Should().Equal(("OQ-1", LineOf(lines, "| OQ-1 |")));
    }

    [Fact]
    public void RecordsBesideSpecificationsInBothLayouts_WhenFactsAreGivenAsJson_ShouldCarryEachRecordsKindNumberTitleStatusAndPathInOrdinalOrderOfPath()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/area", "0001", "F1");
        tree.WriteFeature("0001", "F2");
        tree.WriteRaw("src/area/.spec/lessons/0001-a-lesson.md", "---\ntitle: \"Lesson 0001: a lesson\"\ntype: lesson\n---\n\n# Lesson 0001\n");
        tree.WriteRaw(
            "src/area/.spec/decisions/0002-later.md",
            "---\ntitle: \"Decision 0002: later\"\ntype: decision\n---\n\n# Decision 0002\n\n## Status\n\nAccepted.\n");
        tree.WriteRaw("src/area/.spec/decisions/0001-first.md", "---\ntitle: \"Decision 0001: first\"\ntype: decision\n---\n\n# Decision 0001\n");
        tree.WriteRaw(
            "src/area/.spec/adr/0001-read-each-record-as-written.md",
            "---\ntitle: \"ADR-0001: Read each record as written\"\ntype: adr\n---\n\n# ADR-0001\n\n## Status\n\nAccepted, 2026-10-12, by the owner.\n");
        tree.WriteRaw(
            "epics/0001-epic/F2-feature/adr/0001-a-legacy-record.md",
            "---\ntitle: \"ADR-0001: A legacy record\"\ntype: adr\n---\n\n# ADR-0001\n\n## Status\n\nproposed - awaiting the owner\n");

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        using (var document = JsonDocument.Parse(Stdout))
        {
            var result = Evaluate(document.RootElement);
            result.IsValid.Should().BeTrue(Errors(result));
        }

        var specifications = JsonNode.Parse(Stdout)!["specifications"]!.AsArray()
            .ToDictionary(static specification => specification!["path"]!.GetValue<string>(), static specification => specification!);
        specifications.Keys.Should().BeEquivalentTo(["src/area/.spec/README.md", "epics/0001-epic/F2-feature/spec.md"]);
        specifications["src/area/.spec/README.md"]["records"].Should().BeOfType<JsonArray>().Which.Select(static record => (
                record!["kind"]!.GetValue<string>(),
                record["number"]!.GetValue<string>(),
                record["title"]!.GetValue<string>(),
                record["status"]?.GetValue<string>(),
                record["path"]!.GetValue<string>()))
            .Should().Equal(
                ("adr", "0001", "ADR-0001: Read each record as written", "Accepted", "src/area/.spec/adr/0001-read-each-record-as-written.md"),
                ("decision", "0001", "Decision 0001: first", null, "src/area/.spec/decisions/0001-first.md"),
                ("decision", "0002", "Decision 0002: later", null, "src/area/.spec/decisions/0002-later.md"),
                ("lesson", "0001", "Lesson 0001: a lesson", null, "src/area/.spec/lessons/0001-a-lesson.md"));
        specifications["epics/0001-epic/F2-feature/spec.md"]["records"].Should().BeOfType<JsonArray>().Which.Select(static record => (
                record!["kind"]!.GetValue<string>(),
                record["number"]!.GetValue<string>(),
                record["title"]!.GetValue<string>(),
                record["status"]?.GetValue<string>(),
                record["path"]!.GetValue<string>()))
            .Should().Equal(("adr", "0001", "ADR-0001: A legacy record", "proposed", "epics/0001-epic/F2-feature/adr/0001-a-legacy-record.md"));
    }

    [Fact]
    public void RecordsWithNoOrAnUnreadableFrontmatterNoStatusSectionOrNoLeadingWord_WhenFactsAreGivenAsJson_ShouldGiveEachWithOnlyThatMemberNull()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/area", "0001", "F1");
        tree.WriteRaw(
            "src/area/.spec/adr/0001-no-status-section.md",
            "---\ntitle: \"ADR-0001: No status\"\ntype: adr\n---\n\n# ADR-0001\n\n## Context\n\nAccepted.\n");
        tree.WriteRaw("src/area/.spec/adr/0002-an-unreadable-frontmatter.md", "---\ntitle: [unclosed\n---\n\n# ADR-0002\n\n## Status\n\nAccepted.\n");
        tree.WriteRaw("src/area/.spec/decisions/-a-leading-dash.md", "---\ntitle: \"Decision: a leading dash\"\ntype: decision\n---\n\n# Decision\n");
        tree.WriteRaw("src/area/.spec/lessons/0001-no-frontmatter.md", "# Lesson 0001\n\nNo frontmatter.\n");

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        using (var document = JsonDocument.Parse(Stdout))
        {
            var result = Evaluate(document.RootElement);
            result.IsValid.Should().BeTrue(Errors(result));
        }

        JsonNode.Parse(Stdout)!["specifications"]!.AsArray().Should().ContainSingle().Which!["records"].Should().BeOfType<JsonArray>().Which
            .Select(static record => (
                record!["kind"]!.GetValue<string>(),
                record["number"]?.GetValue<string>(),
                record["title"]?.GetValue<string>(),
                record["status"]?.GetValue<string>(),
                record["path"]!.GetValue<string>()))
            .Should().Equal(
                ("adr", "0001", "ADR-0001: No status", null, "src/area/.spec/adr/0001-no-status-section.md"),
                ("adr", "0002", null, "Accepted", "src/area/.spec/adr/0002-an-unreadable-frontmatter.md"),
                ("decision", null, "Decision: a leading dash", null, "src/area/.spec/decisions/-a-leading-dash.md"),
                ("lesson", "0001", null, null, "src/area/.spec/lessons/0001-no-frontmatter.md"));
    }

    [Fact]
    public void RecordsBesideASpecification_WhenTheTreeIsCheckedAndItsFactsAreGiven_ShouldDiscoverNoRecordAsASpecificationAndChangeNoVerdict()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/area", "0001", "F1");
        var before = AcceptanceTool.Launch(tree.Root, "--json");
        tree.WriteRaw("src/area/.spec/adr/0001-an-adr.md", "---\ntitle: \"ADR-0001: An ADR\"\ntype: feature\nid: F9\n---\n\n## Status\n\nAccepted.\n");
        tree.WriteRaw("src/area/.spec/decisions/0001-a-decision.md", "---\nepic: \"0001\"\nid: F1\n---\n\n# Specification: F1\n");
        tree.WriteRaw("src/area/.spec/lessons/0001-a-lesson.md", "# Lesson\n");
        var discovered = EngineServices.Reader().Read(tree.Root, null).Features.Select(static feature => feature.RelativePath).ToArray();

        // When
        var after = AcceptanceTool.Launch(tree.Root, "--json");
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "--json");

        // Then
        discovered.Should().Equal("src/area/.spec/README.md");
        after.ExitCode.Should().Be(before.ExitCode, after.Stderr);
        after.Stdout.Should().Be(before.Stdout);
        ExitCode.Should().Be(0, Stderr);
        var specification = JsonNode.Parse(Stdout)!["specifications"]!.AsArray().Should().ContainSingle().Which!;
        specification["path"]!.GetValue<string>().Should().Be("src/area/.spec/README.md");
        specification["records"].Should().BeOfType<JsonArray>().Which.Select(static record => record!["path"]!.GetValue<string>()).Should().Equal(
            "src/area/.spec/adr/0001-an-adr.md", "src/area/.spec/decisions/0001-a-decision.md", "src/area/.spec/lessons/0001-a-lesson.md");
    }

    [Fact]
    public void ASpecificationsPath_WhenFactsAreGivenForIt_ShouldGiveThatSpecificationOnly()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/tool", "0001", "F1");
        tree.WriteCoLocatedFeature("src/tool/Features/Facts", "0001", "F2");
        tree.WriteCoLocatedFeature("src/toolbox", "0001", "F3");
        EngineServices.Reader().Read(tree.Root, null).Features.Should().HaveCount(3);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", "src/tool/.spec/README.md", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        JsonNode.Parse(Stdout)!["specifications"]!.AsArray().Select(static specification => specification!["path"]!.GetValue<string>())
            .Should().Equal("src/tool/.spec/README.md");
    }

    [Fact]
    public void ADirectoryRelativeToTheRoot_WhenFactsAreGivenForIt_ShouldGiveEverySpecificationUnderItByWholePathSegmentsInDiscoveryOrder()
    {
        // Given
        using var tree = new SpechtTree(["repo"]);
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/tool", "0001", "F1");
        tree.WriteCoLocatedFeature("src/tool/Features/Facts", "0001", "F2");
        tree.WriteCoLocatedFeature("src/toolbox", "0001", "F3");
        var discovered = EngineServices.Reader().Read(tree.Root, null).Features.Select(static feature => feature.RelativePath).ToArray();
        discovered.Should().HaveCount(3);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(Path.GetDirectoryName(tree.Root)!, "facts", "src/tool", "--root", "repo", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        JsonNode.Parse(Stdout)!["specifications"]!.AsArray().Select(static specification => specification!["path"]!.GetValue<string>())
            .Should().Equal(discovered.Where(static path => path.StartsWith("src/tool/", StringComparison.Ordinal)))
            .And.HaveCount(2);
    }

    [Theory]
    [ClassData(typeof(FactsUnselectingPaths))]
    public void APathSelectingNoSpecification_WhenFactsAreGivenForIt_ShouldWriteNothingOnStdoutAndExitNotFound(string path)
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/tool", "0001", "F1");
        tree.WriteCoLocatedFeature("src/tool/Features/Facts", "0001", "F2");
        tree.WriteCoLocatedFeature("src/toolbox", "0001", "F3");
        EngineServices.Reader().Read(tree.Root, null).Features.Should().HaveCount(3);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "facts", path, "--json");

        // Then
        ExitCode.Should().Be(4, Stderr);
        Stdout.Should().BeEmpty();
        Stderr.Should().NotBeNullOrWhiteSpace();
    }

    private static EvaluationResults Evaluate(JsonElement document)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "docs", "schema", "facts.schema.json");
        File.Exists(path).Should().BeTrue("the facts schema is published at docs/schema/facts.schema.json (OQ-1)");
        return JsonSchema.FromText(File.ReadAllText(path)).Evaluate(document, new EvaluationOptions { OutputFormat = OutputFormat.List });
    }

    private static string Errors(EvaluationResults result) =>
        string.Join("; ", (result.Details ?? []).Where(static detail => detail.Errors is not null)
            .SelectMany(static detail => detail.Errors!.Select(error => $"{detail.InstanceLocation} {error.Value}")));

    private static int LineOf(string[] lines, string prefix) =>
        Array.FindIndex(lines, line => line.StartsWith(prefix, StringComparison.Ordinal)) + 1;

    private static IEnumerable<string> MemberPaths(JsonNode? node, string prefix) => node switch
    {
        JsonObject members => members.SelectMany(member => MemberPaths(member.Value, prefix.Length == 0 ? member.Key : $"{prefix}.{member.Key}")
            .Prepend(prefix.Length == 0 ? member.Key : $"{prefix}.{member.Key}")),
        JsonArray items => items.SelectMany(item => MemberPaths(item, prefix + "[]")),
        _ => [],
    };
}
