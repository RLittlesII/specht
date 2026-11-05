using System.Diagnostics;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Reqnroll.Infrastructure;

namespace Specht.Acceptance.Mcp;

/// <summary>
/// The steps of Reading through MCP (0132-F4) that item 0330 delivers: the verdict, the tools the server lists and their names (B-001,
/// B-002, B-011, B-013, B-015, B-016, B-017, B-019). Each answer is asked for through the tool <c>check</c>; the other answers have no
/// binding until their items register them.
/// </summary>
public sealed partial class McpHostSteps
{
    [Given("a repository whose manifest pins a shipped schema version")]
    public void GivenARepositoryThatPinsASchemaVersion()
    {
        GivenAToolManifest();
        GivenTheToolIsRestored();
    }

    [Given("two Feature specifications, one of them with a violation the check reports")]
    public void GivenTwoSpecifications() => AcceptanceTool.FakeTools.WriteSpecifications(Tree.Root);

    [Given("a session with the tool's MCP server on that repository")]
    public void GivenASessionOnTheRepository() => _session = AcceptanceTool.McpSession.Start(Tree.Root, Fakes().Environment);

    [Given(@"^the repository pins schema version (\d+\.\d+\.\d+)$")]
    public void GivenTheRepositoryPinsSchemaVersion(string version) => Tree.Pin(version);

    [Given("the verdict has been asked for once")]
    public void GivenTheVerdictWasAskedFor() => WhenTheVerdictIsAskedFor();

    [When(@"^the verdict is asked for(?: again)?$")]
    public void WhenTheVerdictIsAskedFor() => _answers = [.. _answers, Session().CallTool(_nextId += 10, "check")];

    [When("each answer is asked for")]
    public void WhenEachAnswerIsAskedFor()
    {
        _before = AcceptanceTool.Snapshot(Tree.Root);
        _answers = [Session().CallTool(_nextId += 10, "check")];
    }

    [When("the violation the check reports is fixed in its specification")]
    public void WhenTheViolationIsFixed() => AcceptanceTool.FakeTools.FixViolation(Tree.Root);

    [When(@"^the (?:tools|answers) the server offers are listed$")]
    public void WhenTheToolsAreListed() =>
        _offered = Offered(Session().Exchange(AcceptanceTool.McpRequest(_nextId += 10, "tools/list", AcceptanceTool.McpRevision)));

    [When("the server lists what the verdict can be asked for with")]
    public void WhenTheVerdictsArgumentsAreListed() => WhenTheToolsAreListed();

    [Then("the answer is exactly what the check writes as its JSON report for the same repository")]
    public void ThenTheAnswerIsTheReport() =>
        AcceptanceTool.McpTextBlocks(_answers.Single()).Should().Equal(Report(AcceptanceTool.FakeTools.Pinned));

    [Then("the part of the result holding the verdict is exactly what the check writes as its JSON report for the same repository")]
    public void ThenThePartHoldingTheVerdictIsTheReport() =>
        AcceptanceTool.McpTextBlocks(_answers.Single())
            .Where(static block => block.StartsWith(AcceptanceTool.FakeTools.OutputMarker, StringComparison.Ordinal))
            .Should()
            .Equal(Report(AcceptanceTool.FakeTools.Global));

    [Then("the second answer is exactly what the check writes as its JSON report for the edited repository")]
    public void ThenTheSecondAnswerIsTheEditedReport()
    {
        _answers.Should().HaveCount(2);
        AcceptanceTool.McpTextBlocks(_answers[1]).Should().Equal(Report(AcceptanceTool.FakeTools.Pinned));
        AcceptanceTool.McpTextBlocks(_answers[1]).Should().NotEqual(AcceptanceTool.McpTextBlocks(_answers[0]));
    }

    [Then("the answer carries the violation")]
    public void ThenTheAnswerCarriesTheViolation() =>
        string.Concat(AcceptanceTool.McpTextBlocks(_answers.Single())).Should().Contain(AcceptanceTool.FakeTools.Violation);

    [Then("the answer is not marked as an error")]
    public void ThenTheAnswerIsNotAnError() => AcceptanceTool.McpFailure(_answers.Single()).Should().BeNull();

    [Then("each is exactly what its command prints under that pin")]
    public void ThenEachIsItsCommandsOutput() => ThenTheAnswerIsTheReport();

    [Then("no file under the root or elsewhere has been created, modified or deleted")]
    public void ThenNothingWasWritten()
    {
        _answers.Should().NotBeEmpty();
        AcceptanceTool.Snapshot(Tree.Root).Should().BeEquivalentTo(_before);
    }

    [Then("nothing it lists names a specification or a path")]
    public void ThenNoArgumentNamesAPath()
    {
        var verdict = Tool("check");
        var arguments = verdict["inputSchema"]?["properties"] as JsonObject ?? [];
        arguments.Select(static argument => argument.Key)
            .Should()
            .NotContain(static name => ArgumentsThatNameAPath.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase)));
    }

    [Then("each is one of the verdict, a rule's text, the rule list, the manifest, the tree's facts, claim coverage, " +
        "the shipped schema versions, and the frontmatter schemas and templates")]
    public void ThenEachIsAnAnswer() => Names().Should().OnlyContain(static name => AnswerTools.Contains(name));

    [Then("none initialises, upgrades, pins, formats, sets a rule's level or writes a report")]
    public void ThenNoneWrites() => Names().Should().NotBeEmpty().And.NotContain(static name => WriteTools.Contains(name));

    [Then("none answers for the work queue")]
    public void ThenNoneAnswersForTheQueue() =>
        Names().Should().NotBeEmpty().And.NotContain(static name => name.Contains("queue", StringComparison.OrdinalIgnoreCase));

    [Then("each tool's name is the name of the command it serves")]
    public void ThenEachNameIsACommand() => Names().Should().NotBeEmpty().And.OnlyContain(static name => AnswerTools.Contains(name));

    [Then("the verdict's tool is named for the check")]
    public void ThenTheVerdictIsNamedForTheCheck() => Names().Should().Contain("check");

    [Then("the answer is marked as an error")]
    public void ThenTheLastAnswerIsAnError()
    {
        if (!scenario.ScenarioInfo.Tags.Contains("B-020", StringComparer.Ordinal))
        {
            throw new PendingStepException();
        }

        LastFailure().Should().NotBeNull();
    }

    [Then("it names where to read how to restore the tool")]
    public void ThenTheLastAnswerPointsToRestoring() => LastFailure().Should().Contain("docs/mcp.md#restore-the-tool");

    [Then("the answer quotes nothing the command wrote")]
    public void ThenTheLastAnswerQuotesNothingTheCommandWrote()
    {
        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = Tree.Root };
        foreach (var argument in new[] { "tool", "run", "specht", "--", "--json" })
        {
            start.ArgumentList.Add(argument);
        }

        foreach (var (name, value) in Fakes().Environment)
        {
            start.Environment[name] = value;
        }

        var (stdout, stderr, exitCode) = AcceptanceTool.Run(start);
        exitCode.Should().Be(1);
        stdout.Should().BeEmpty();
        var written = stderr.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        written.Should().NotBeEmpty();
        var answer = string.Join('\n', AcceptanceTool.McpTextBlocks(_answers[^1]));
        written.Should().AllSatisfy(line => answer.Should().NotContain(line));
    }


    private static JsonArray Offered(string? listing) =>
        JsonNode.Parse(listing ?? "null")?["result"]?["tools"] as JsonArray
        ?? throw new Xunit.Sdk.XunitException($"The host did not list its tools; tools/list answered: {listing ?? "nothing"}");

    private AcceptanceTool.McpSession Session() => _session ?? throw new InvalidOperationException("No session was started.");

    private string Report(string label) => AcceptanceTool.FakeTools.JsonOutput(label, Tree.Root);

    private IReadOnlyList<string> Names() => _offered.Select(static tool => tool!["name"]!.GetValue<string>()).ToList();

    private JsonObject Tool(string name) =>
        _offered.Select(static tool => tool!.AsObject()).SingleOrDefault(tool => tool["name"]?.GetValue<string>() == name)
        ?? throw new Xunit.Sdk.XunitException($"The host offers no '{name}' tool; it lists {string.Join(", ", Names())}.");

    private string? LastFailure() => AcceptanceTool.McpFailure(_answers[^1]);

    private static readonly string[] ArgumentsThatNameAPath = ["spec", "path", "file", "feature", "directory"];

    private static readonly HashSet<string> AnswerTools =
        new(["check", "explain", "rules", "manifest", "facts", "trace", "schema_list"], StringComparer.Ordinal);

    private static readonly HashSet<string> WriteTools = new(["init", "upgrade", "schema_pin", "format", "rules_set", "report"], StringComparer.Ordinal);

    private int _nextId;
    private JsonArray _offered = [];
}
