using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Json.Schema;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Trace;

/// <summary>
/// The built tool's <c>specht trace</c> over synthetic trees on disk (<c>0001-F13</c> B-001 to B-003, B-007, B-019),
/// launched as a shell launches it: with <c>--json</c>, one document valid against the published trace schema at
/// <c>docs/schema/trace.schema.json</c>, copied into the output from that one file, over a tree holding a resolved, an
/// unresolved and a skipped test, a claim with no traceability row and a specification with no identity; a resolved test's
/// file relative to the root, from the test files <c>tests.files</c> declares; and without <c>--json</c>, the same lines on
/// every run, in discovery order and then claims-table order.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class TraceCommandIntegrationTests
{
    [Fact]
    public void ATreeWithResolvedUnresolvedSkippedAndRowlessClaims_WhenTracedAsJson_ShouldWriteOneDocumentValidAgainstThePublishedTraceSchema()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        DeclareTestFiles(tree);
        tree.WriteRaw("test/TraceTests.cs", "public sealed class TraceTests\n{\n    public void TracesOne() { }\n}\n");
        tree.WriteCoLocatedFeature("src/first", "0001", "F1", null, SpechtTree.SectionsWith(
            "3. Acceptance Criteria",
            "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                + "| B-001 | It traces one. | brd | Active |\n| B-002 | It traces two. | brd | Active |\n"
                + "| B-003 | It traces three. | brd | Active |\n| B-004 | It traces four. | brd | Active |\n")
            .Select(static section => section.StartsWith("## 9. Traceability Matrix\n", StringComparison.Ordinal)
                ? "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n"
                    + "| B-001 | It does the thing | `TracesOne`, `TracesNone` | Partial |\n"
                    + "| B-002 | It does the thing | `TracesOne` | Waived |\n"
                    + "| B-004 | It does the thing | Missing | Missing |\n"
                : section)
            .ToList());
        tree.WriteSpecification("src/third/.spec/README.md", "0001", "F3", new Dictionary<string, string> { ["epic"] = "null" });

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "trace", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        using var document = JsonDocument.Parse(Stdout);
        document.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        var path = Path.Combine(AppContext.BaseDirectory, "docs", "schema", "trace.schema.json");
        File.Exists(path).Should().BeTrue("the trace schema is published at docs/schema/trace.schema.json (B-002)");
        var result = JsonSchema.FromText(File.ReadAllText(path))
            .Evaluate(document.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });
        result.IsValid.Should().BeTrue(string.Join("; ", (result.Details ?? []).Where(static detail => detail.Errors is not null)
            .SelectMany(static detail => detail.Errors!.Select(error => $"{detail.InstanceLocation} {error.Value}"))));
    }

    [Fact]
    public void ATestAFileInANestedFolderHolds_WhenTracedAsJson_ShouldCarryThatFilesPathRelativeToTheRootAndTheTestsLine()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        DeclareTestFiles(tree);
        tree.WriteRaw("test/unit/deep/TraceTests.cs", "namespace Trace;\n\npublic sealed class TraceTests\n{\n    public void TracesOne() { }\n}\n");
        tree.WriteCoLocatedFeature("src/first", "0001", "F1", null, SpechtTree.SectionsWith(
            "9. Traceability Matrix",
            "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n"
                + "| B-001 | It does the thing | `TracesOne` | Covered |\n"));

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "trace", "--json");

        // Then
        ExitCode.Should().Be(0, Stderr);
        var test = JsonNode.Parse(Stdout)!["specifications"]!.AsArray().Should().ContainSingle().Which!["claims"]!.AsArray()
            .Should().ContainSingle().Which!["tests"]!.AsArray().Should().ContainSingle().Which!;
        (test["name"]!.GetValue<string>(), test["resolution"]!.GetValue<string>(), test["file"]!.GetValue<string>(), test["line"]!.GetValue<int>())
            .Should().Be(("TracesOne", "resolved", "test/unit/deep/TraceTests.cs", 5));
    }

    [Fact]
    public void TwoSpecifications_WhenTracedTwiceAsLines_ShouldPrintTheSameLinesInDiscoveryOrderThenClaimsTableOrder()
    {
        // Given
        using var tree = new SpechtTree();
        tree.Pin("0.2.0");
        tree.WriteCoLocatedFeature("src/first", "0001", "F1", null, SpechtTree.SectionsWith(
            "3. Acceptance Criteria",
            "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                + "| B-002 | It traces two. | brd | Active |\n| B-001 | It traces one. | brd | Active |\n"));
        tree.WriteCoLocatedFeature("src/second", "0001", "F2");
        var discovered = EngineServices.Reader().Read(tree.Root, null).Features.Select(static feature => feature.RelativePath).ToArray();
        discovered.Should().Equal("src/first/.spec/README.md", "src/second/.spec/README.md");

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "trace");
        var again = AcceptanceTool.Launch(tree.Root, "trace");

        // Then
        ExitCode.Should().Be(0, Stderr);
        again.ExitCode.Should().Be(0, again.Stderr);
        again.Stdout.Should().Be(Stdout);
        Stdout.ReplaceLineEndings("\n").Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(static line => string.Join('\t', line.Split('\t').Take(2)))
            .Should().Equal("src/first/.spec/README.md\tB-002", "src/first/.spec/README.md\tB-001", "src/second/.spec/README.md\tB-001");
    }

    private static void DeclareTestFiles(SpechtTree tree)
    {
        var path = Path.Combine(tree.Root, SpechtTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest["tests"] = new JsonObject { ["files"] = new JsonArray("test/**/*.cs") };
        File.WriteAllText(path, manifest.ToJsonString());
    }
}
