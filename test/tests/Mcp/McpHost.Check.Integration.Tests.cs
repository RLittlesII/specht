using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Mcp;

/// <summary>
/// Runs the built MCP as a process against a fake pinned <c>specht</c> and holds what the tool <c>check</c> answers and what the host
/// lists (0132-F4 B-001, B-002, B-011, B-013, B-015, B-016, B-017, B-019; 0132-F3 B-009). A fake writes a stated stdout for
/// <c>--json</c> derived from the root it runs on, so an answer is compared byte for byte with what the command wrote. Every test that
/// calls <c>check</c> reads the tool list first and fails when the host offers none, so none passes on the error a missing tool is.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class McpHostCheckIntegrationTests
{
    public static bool IsWindows => OperatingSystem.IsWindows();

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ARepositoryWithAViolation_WhenCheckIsCalled_ShouldAnswerWithExactlyTheCommandsJsonOutput()
    {
        // Given
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: true);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        AcceptanceTool.McpTextBlocks(answers.Single())
            .Should()
            .Equal(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void AnUnpinnedRepositoryWithAGlobalTool_WhenCheckIsCalled_ShouldKeepTheCommandsJsonOutputInItsOwnPart()
    {
        // Given
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: true);
        tools.Unpin(tree.Root, removed: false);
        tools.InstallGlobal(AcceptanceTool.FakeTools.GlobalVersion);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        AcceptanceTool.McpTextBlocks(answers.Single())
            .Where(static block => block.StartsWith(AcceptanceTool.FakeTools.OutputMarker, StringComparison.Ordinal))
            .Should()
            .Equal(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Global, tree.Root));
    }

    [Fact]
    public void TheListOfTools_WhenRead_ShouldGiveTheVerdictNoArgumentThatNamesASpecificationOrAPath()
    {
        // Given
        using var tree = new SpechtTree();

        // When
        var offered = Offered(tree.Root);

        // Then
        var arguments = offered.Single(static tool => tool["name"]?.GetValue<string>() == "check")["inputSchema"]?["properties"] as JsonObject ?? [];
        arguments.Select(static argument => argument.Key)
            .Should()
            .NotContain(static name => new[] { "spec", "path", "file", "feature", "directory" }
                .Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ATreeThatTheCheckReportsViolationsFor_WhenCheckIsCalled_ShouldAnswerWithoutMarkingAnError()
    {
        // Given
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: true);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        AcceptanceTool.McpFailure(answers.Single()).Should().BeNull();
        string.Concat(AcceptanceTool.McpTextBlocks(answers.Single())).Should().Contain(AcceptanceTool.FakeTools.Violation);
    }

    [Theory(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    [ClassData(typeof(ShippedSchemaVersions))]
    public void ARepositoryPinnedToAShippedSchemaVersion_WhenCheckIsCalled_ShouldAnswerWithTheCommandsOutputUnderThatPin(string version)
    {
        // Given
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: true);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        tree.Pin(version);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        AcceptanceTool.McpTextBlocks(answers.Single())
            .Should()
            .Equal(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ASpecificationEditedBetweenTwoCalls_WhenCheckIsCalledAgain_ShouldAnswerForTheEditedTree()
    {
        // Given
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: true);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        using var session = AcceptanceTool.McpSession.Start(tree.Root, tools.Environment);
        var first = session.CallTool(10, "check");

        // When
        AcceptanceTool.FakeTools.FixViolation(tree.Root);
        var second = session.CallTool(20, "check");
        session.Close();

        // Then
        AcceptanceTool.McpTextBlocks(second).Should().Equal(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
        AcceptanceTool.McpTextBlocks(second).Should().NotEqual(AcceptanceTool.McpTextBlocks(first));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ACallToCheck_WhenAnswered_ShouldCreateModifyAndDeleteNoFileUnderTheRoot()
    {
        // Given
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: true);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        var before = AcceptanceTool.Snapshot(tree.Root);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        answers.Should().NotBeEmpty();
        AcceptanceTool.Snapshot(tree.Root).Should().BeEquivalentTo(before);
    }

    [Fact]
    public void TheListOfTools_WhenRead_ShouldOfferNoWriteAndNoWorkQueue()
    {
        // Given
        using var tree = new SpechtTree();

        // When
        var names = Names(Offered(tree.Root));

        // Then
        names.Should().NotBeEmpty().And.NotContain(["init", "upgrade", "schema_pin", "format", "rules_set", "report"]);
        names.Should().NotContain(static name => name.Contains("queue", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TheListOfTools_WhenRead_ShouldNameEachToolForTheCommandItServesAndTheVerdictCheck()
    {
        // Given
        using var tree = new SpechtTree();

        // When
        var names = Names(Offered(tree.Root));

        // Then
        names.Should().Contain("check").And.OnlyContain(static name => CommandNames.Contains(name));
    }

    [Theory(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    [ClassData(typeof(ServerStartingPlaces))]
    public void AServerStartedInTheRepositoryOrElsewhereNamingIt_WhenCheckIsCalled_ShouldAnswerForTheRepository(bool elsewhere)
    {
        // Given
        using var tree = new SpechtTree();
        using var other = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: true);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        using var session = AcceptanceTool.McpSession.Start(
            elsewhere ? other.Root : tree.Root,
            tools.Environment,
            elsewhere ? ["--root", tree.Root] : []);

        // When
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        AcceptanceTool.McpTextBlocks(answer).Should().Equal(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
    }

    private static IReadOnlyList<string> Names(IReadOnlyList<JsonObject> offered) => offered.Select(static tool => tool["name"]!.GetValue<string>()).ToList();

    private static IReadOnlyList<JsonObject> Offered(string root)
    {
        using var session = AcceptanceTool.McpSession.Start(root);
        var listing = session.Exchange(AcceptanceTool.McpRequest(10, "tools/list", AcceptanceTool.McpRevision));
        session.Close();

        return (JsonNode.Parse(listing ?? "null")?["result"]?["tools"] as JsonArray
            ?? throw new Xunit.Sdk.XunitException($"The host did not list its tools; tools/list answered: {listing ?? "nothing"}"))
            .Select(static tool => tool!.AsObject())
            .ToList();
    }

    private static IReadOnlyList<JsonObject> Call(string root, AcceptanceTool.FakeTools tools, int calls)
    {
        using var session = AcceptanceTool.McpSession.Start(root, tools.Environment);
        var answers = Enumerable.Range(0, calls).Select(index => session.CallTool(10 + (index * 10), "check")).ToList();
        session.Close();

        return answers;
    }

    private static readonly string[] CommandNames = ["check", "explain", "rules", "manifest", "facts", "trace", "schema_list"];
}
