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
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        AcceptanceTool.McpTextBlocks(answer)
            .Should()
            .Equal(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void AnUnpinnedRepositoryWithAGlobalTool_WhenCheckIsCalled_ShouldKeepTheCommandsJsonOutputInItsOwnPart()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture()
            .WithPin(tree.Root)
            .WithUnpinned(tree.Root)
            .WithGlobal();
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        AcceptanceTool.McpTextBlocks(answer)
            .Where(static block => block.StartsWith(AcceptanceTool.FakeTools.OutputMarker, StringComparison.Ordinal))
            .Should()
            .Equal(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Global, tree.Root));
    }

    [Fact]
    public void TheListOfTools_WhenRead_ShouldGiveTheVerdictNoArgumentThatNamesASpecificationOrAPath()
    {
        // Given
        using var tree = new SpechtTree();
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var listing = session.Exchange(AcceptanceTool.McpRequest(10, "tools/list", AcceptanceTool.McpRevision));
        session.Close();

        // Then
        var offered = JsonNode.Parse(listing ?? "null")?["result"]?["tools"] as JsonArray;
        offered.Should().NotBeNull($"the host must list its tools; tools/list answered: {listing ?? "nothing"}");
        var arguments = offered!.Select(static tool => tool!.AsObject())
            .Single(static tool => tool["name"]?.GetValue<string>() == "check")["inputSchema"]?["properties"] as JsonObject ?? [];
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
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().BeNull();
        string.Concat(AcceptanceTool.McpTextBlocks(answer)).Should().Contain(AcceptanceTool.FakeTools.Violation);
    }

    [Theory(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    [ClassData(typeof(ShippedSchemaVersions))]
    public void ARepositoryPinnedToAShippedSchemaVersion_WhenCheckIsCalled_ShouldAnswerWithTheCommandsOutputUnderThatPin(string version)
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        tree.Pin(version);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        AcceptanceTool.McpTextBlocks(answer)
            .Should()
            .Equal(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ASpecificationEditedBetweenTwoCalls_WhenCheckIsCalledAgain_ShouldAnswerForTheEditedTree()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
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
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        var before = AcceptanceTool.Snapshot(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        answer.Should().NotBeNull();
        AcceptanceTool.Snapshot(tree.Root).Should().BeEquivalentTo(before);
    }

    [Fact]
    public void TheListOfTools_WhenRead_ShouldOfferNoWriteAndNoWorkQueue()
    {
        // Given
        using var tree = new SpechtTree();
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var listing = session.Exchange(AcceptanceTool.McpRequest(10, "tools/list", AcceptanceTool.McpRevision));
        session.Close();

        // Then
        var offered = JsonNode.Parse(listing ?? "null")?["result"]?["tools"] as JsonArray;
        offered.Should().NotBeNull($"the host must list its tools; tools/list answered: {listing ?? "nothing"}");
        var names = offered!.Select(static tool => tool!["name"]!.GetValue<string>()).ToList();
        names.Should().NotBeEmpty().And.NotContain(["init", "upgrade", "schema_pin", "format", "rules_set", "report"]);
        names.Should().NotContain(static name => name.Contains("queue", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TheListOfTools_WhenRead_ShouldNameEachToolForTheCommandItServesAndTheVerdictCheck()
    {
        // Given
        using var tree = new SpechtTree();
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var listing = session.Exchange(AcceptanceTool.McpRequest(10, "tools/list", AcceptanceTool.McpRevision));
        session.Close();

        // Then
        var offered = JsonNode.Parse(listing ?? "null")?["result"]?["tools"] as JsonArray;
        offered.Should().NotBeNull($"the host must list its tools; tools/list answered: {listing ?? "nothing"}");
        var names = offered!.Select(static tool => tool!["name"]!.GetValue<string>()).ToList();
        names.Should().Contain("check").And.BeSubsetOf(["check", "explain", "rules", "manifest", "facts", "trace", "schema_list"]);
    }

    [Theory(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    [ClassData(typeof(ServerStartingPlaces))]
    public void AServerStartedInTheRepositoryOrElsewhereNamingIt_WhenCheckIsCalled_ShouldAnswerForTheRepository(bool elsewhere)
    {
        // Given
        using var tree = new SpechtTree();
        using var other = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        McpSessionStart start = new McpSessionFixture()
            .WithRoot(elsewhere ? other.Root : tree.Root)
            .WithTools(tools)
            .WithArguments(elsewhere ? ["--root", tree.Root] : []);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        AcceptanceTool.McpTextBlocks(answer).Should().Equal(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
    }
}
