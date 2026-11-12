using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Mcp;

/// <summary>
/// Runs the built MCP as a process against a fake pinned <c>specht</c> and holds what the tools <c>explain</c>, <c>rules</c> and
/// <c>manifest</c> answer, the arguments they and <c>check</c> (its schema source and <c>strict</c>) pass on, the failures a command
/// reports, a pin moved between two calls and a minted id with the command its call runs (0132-F4 B-003, B-004, B-005, B-010, B-012, B-014,
/// B-022, B-023, B-025, B-026). A fake writes a stated stdout for
/// each command derived from the root it runs on, so an answer is compared byte for byte with what the command wrote. Every test reads
/// the tool list first and fails when the host offers none of the tool, so none passes on the error a missing tool is.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class McpHostReadingIntegrationTests
{
    public static bool IsWindows => OperatingSystem.IsWindows();

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ARuleThePinnedVersionHas_WhenExplainIsCalled_ShouldAnswerWithExactlyTheCommandsOutput()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "explain", new JsonObject { ["rule"] = AcceptanceTool.FakeTools.KnownRule });
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().BeNull();
        AcceptanceTool.McpTextBlocks(answer).Should().Equal(AcceptanceTool.FakeTools.ExplainOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ARepository_WhenRulesIsCalled_ShouldAnswerWithExactlyTheCommandsJsonOutput()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "rules");
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().BeNull();
        AcceptanceTool.McpTextBlocks(answer).Should().Equal(AcceptanceTool.FakeTools.RulesOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ARepository_WhenManifestIsCalled_ShouldAnswerWithExactlyTheCommandsJsonOutput()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "manifest");
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().BeNull();
        AcceptanceTool.McpTextBlocks(answer).Should().Equal(AcceptanceTool.FakeTools.ManifestOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void AShippedDefaultsRequest_WhenManifestIsCalled_ShouldAnswerWithWhatTheCommandPrintsGivenDefaults()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "manifest", new JsonObject { ["defaults"] = true });
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().BeNull();
        AcceptanceTool.McpTextBlocks(answer)
            .Should()
            .Equal(AcceptanceTool.FakeTools.ManifestOutput(AcceptanceTool.FakeTools.Pinned, tree.Root, defaults: true));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ARequestToReadTheSchemasFromDisk_WhenCheckIsCalled_ShouldAnswerWithWhatTheCommandPrintsGivenThatSchemaSource()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check", new JsonObject { ["schemaSource"] = "disk" });
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().BeNull();
        AcceptanceTool.McpTextBlocks(answer)
            .Should()
            .Equal(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root, "disk"));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void AStrictRequest_WhenCheckIsCalled_ShouldAnswerWithWhatTheCommandPrintsGivenStrictAndNotMarkAnError()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check", new JsonObject { ["strict"] = true });
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().BeNull();
        AcceptanceTool.McpTextBlocks(answer)
            .Should()
            .Equal(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root, strict: true));
    }

    [Theory(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    [ClassData(typeof(FailingToolCalls))]
    public void ACommandThatFails_WhenItsToolIsCalled_ShouldAnswerWithAnErrorWhoseTextIsStandardError(
        string tool,
        string? rule,
        string manifest,
        string message)
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        if (manifest == "removed")
        {
            File.Delete(Path.Combine(tree.Root, SpechtTree.ManifestPath));
        }

        if (manifest == "rejected")
        {
            tree.WriteRaw(SpechtTree.ManifestPath, AcceptanceTool.FakeTools.RejectedManifest);
        }

        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, tool, rule is null ? null : new JsonObject { ["rule"] = rule });
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().Be(message);
    }

    [Theory(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    [ClassData(typeof(DashedArguments))]
    public void AnArgumentWhoseValueStartsWithADash_WhenItsToolIsCalled_ShouldRunNoCommandAndAnswerWithAnError(
        string tool,
        string argument,
        string value)
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, tool, new JsonObject { [argument] = value });
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().NotBeNull();
        tools.Runs.Should().BeEmpty();
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ThePinMovedBetweenTwoCalls_WhenRulesIsCalledAgain_ShouldAnswerForTheNewPin()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        tree.Pin("0.1.0");
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var first = session.CallTool(10, "rules");

        // When
        tree.Pin("0.2.1");
        var second = session.CallTool(20, "rules");
        session.Close();

        // Then
        AcceptanceTool.McpTextBlocks(second).Should().Equal(AcceptanceTool.FakeTools.RulesOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
        AcceptanceTool.McpTextBlocks(second).Should().NotEqual(AcceptanceTool.McpTextBlocks(first));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void TwoCallsForANewId_WhenMintIsCalledTwice_ShouldAnswerEachWithExactlyWhatItsOwnMintRunPrinted()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var first = session.CallTool(10, "mint", new JsonObject { ["type"] = "adr" });
        var second = session.CallTool(20, "mint", new JsonObject { ["type"] = "adr" });
        session.Close();

        // Then
        var mints = tools.Runs
            .Select(static (run, index) => (run, position: index + 1))
            .Where(static entry => !entry.run.IsVersionCheck)
            .Select(static entry => entry.position)
            .ToList();
        mints.Should().HaveCount(2);
        AcceptanceTool.McpFailure(first).Should().BeNull();
        AcceptanceTool.McpTextBlocks(first).Should().Equal(AcceptanceTool.FakeTools.MintOutput(AcceptanceTool.FakeTools.Pinned, "adr", mints[0]));
        AcceptanceTool.McpTextBlocks(second).Should().Equal(AcceptanceTool.FakeTools.MintOutput(AcceptanceTool.FakeTools.Pinned, "adr", mints[1]));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ACallNamingAFeature_WhenMintIsCalled_ShouldRunThePinnedMintCommandWithFeatureFollowedByThatFolder()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        session.CallTool(10, "mint", new JsonObject { ["type"] = "claim", ["feature"] = "features/clean" });
        session.Close();

        // Then
        var run = tools.Runs.Where(static logged => !logged.IsVersionCheck).Should().ContainSingle().Subject;
        run.Label.Should().Be(AcceptanceTool.FakeTools.Pinned);
        run.Arguments.Split(' ').Should().HaveCount(4).And.StartWith(["mint", "claim"]).And.ContainInConsecutiveOrder("--feature", "features/clean");
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ACallNamingNoFeature_WhenMintIsCalled_ShouldRunThePinnedMintCommandForTheTypeAlone()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        session.CallTool(10, "mint", new JsonObject { ["type"] = "adr" });
        session.Close();

        // Then
        var run = tools.Runs.Where(static logged => !logged.IsVersionCheck).Should().ContainSingle().Subject;
        run.Label.Should().Be(AcceptanceTool.FakeTools.Pinned);
        run.Arguments.Should().Be("mint adr");
    }
}
