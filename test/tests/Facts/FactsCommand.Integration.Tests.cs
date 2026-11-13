using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Json.Schema;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Facts;

/// <summary>
/// The built tool's <c>specht facts --json</c> over synthetic trees on disk (<c>0132-F2</c> B-001 to B-004, B-014, B-017,
/// B-021 to B-023, B-026), launched as a shell launches it, its output validated against the published facts schema at
/// <c>docs/schema/facts.schema.json</c> (OQ-1's default), copied into the output from that one file.
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

    private static EvaluationResults Evaluate(JsonElement document)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "docs", "schema", "facts.schema.json");
        File.Exists(path).Should().BeTrue("the facts schema is published at docs/schema/facts.schema.json (OQ-1)");
        return JsonSchema.FromText(File.ReadAllText(path)).Evaluate(document, new EvaluationOptions { OutputFormat = OutputFormat.List });
    }

    private static string Errors(EvaluationResults result) =>
        string.Join("; ", (result.Details ?? []).Where(static detail => detail.Errors is not null)
            .SelectMany(static detail => detail.Errors!.Select(error => $"{detail.InstanceLocation} {error.Value}")));

    private static IEnumerable<string> MemberPaths(JsonNode? node, string prefix) => node switch
    {
        JsonObject members => members.SelectMany(member => MemberPaths(member.Value, prefix.Length == 0 ? member.Key : $"{prefix}.{member.Key}")
            .Prepend(prefix.Length == 0 ? member.Key : $"{prefix}.{member.Key}")),
        JsonArray items => items.SelectMany(item => MemberPaths(item, prefix + "[]")),
        _ => [],
    };
}
