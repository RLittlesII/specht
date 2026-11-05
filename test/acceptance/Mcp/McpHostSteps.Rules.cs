using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Mcp;

/// <summary>
/// The steps of Reading through MCP (0132-F4) that item 0331 delivers: a rule's text, the rule list and the manifest through the tools
/// <c>explain</c>, <c>rules</c> and <c>manifest</c>, the arguments a command takes (B-003, B-004, B-005, B-010), a failure the command
/// reports (B-012), a pin moved between two calls (B-014) and a value that starts with a dash (B-022).
/// </summary>
public sealed partial class McpHostSteps
{
    [Given("a rule id the pinned version does not have")]
    public void GivenAnUnknownRule()
    {
        _rule = AcceptanceTool.FakeTools.UnknownRule;
        _expectedExit = 4;
    }

    [Given("the rule list has been asked for once")]
    public void GivenTheRuleListWasAskedFor() => WhenTheRuleListIsAskedFor();

    [When("the text of a rule the pinned version has is asked for")]
    public void WhenARuleIsExplained() => Explain(AcceptanceTool.FakeTools.KnownRule);

    [When("that rule's text is asked for")]
    public void WhenThatRuleIsExplained() => Explain(_rule);

    [When(@"^the rule list is asked for(?: again)?$")]
    public void WhenTheRuleListIsAskedFor() => Ask("rules", null, "rules", "--json");

    [When("the manifest is asked for")]
    public void WhenTheManifestIsAskedFor()
    {
        Ask("manifest", null, "manifest", "--json");
        _expected = () => AcceptanceTool.FakeTools.ManifestOutput(AcceptanceTool.FakeTools.Pinned, Tree.Root);
    }

    [When("the manifest is asked for with the request for the tool's shipped defaults")]
    public void WhenTheManifestIsAskedForWithDefaults()
    {
        Ask("manifest", new JsonObject { ["defaults"] = true }, "manifest", "--defaults", "--json");
        _expected = () => AcceptanceTool.FakeTools.ManifestOutput(AcceptanceTool.FakeTools.Pinned, Tree.Root, defaults: true);
    }

    [When("the verdict is asked for with the request to read the frontmatter schemas from disk")]
    public void WhenTheVerdictIsAskedForWithSchemasFromDisk()
    {
        Ask("check", new JsonObject { ["schemaSource"] = "disk" }, "--json", "--schema-source", "disk");
        _expected = () => Report(AcceptanceTool.FakeTools.Pinned, "disk");
    }

    [When("the verdict is asked for with the request to treat every violation as an error")]
    public void WhenTheVerdictIsAskedForStrictly()
    {
        Ask("check", new JsonObject { ["strict"] = true }, "--json", "--strict");
        _expected = () => Report(AcceptanceTool.FakeTools.Pinned, strict: true);
    }

    [When(@"^(?:that rule's text|the verdict) is asked for with (a rule id|a schema source) that starts with a dash$")]
    public void WhenAskedForWithAValueThatStartsWithADash(string argument)
    {
        if (argument == "a rule id")
        {
            Ask("explain", new JsonObject { ["rule"] = DashedValue }, "--explain", DashedValue);
        }
        else
        {
            Ask("check", new JsonObject { ["schemaSource"] = DashedValue }, "--json", "--schema-source", DashedValue);
        }
    }

    [When("the pin is moved to another shipped version")]
    public void WhenThePinIsMoved()
    {
        var current = JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpechtTree.ManifestPath)))!["schemaVersion"]!.GetValue<string>();
        Tree.Pin(current == "0.1.0" ? "0.2.1" : "0.1.0");
    }

    [Then("the answer is exactly what explaining that rule prints")]
    public void ThenTheAnswerIsTheRulePage() =>
        AcceptanceTool.McpTextBlocks(_answers.Single()).Should().Equal(AcceptanceTool.FakeTools.ExplainOutput(AcceptanceTool.FakeTools.Pinned, Tree.Root));

    [Then("the answer is exactly what listing the rules as JSON prints for the same repository")]
    public void ThenTheAnswerIsTheRuleList() =>
        AcceptanceTool.McpTextBlocks(_answers.Single()).Should().Equal(AcceptanceTool.FakeTools.RulesOutput(AcceptanceTool.FakeTools.Pinned, Tree.Root));

    [Then("the answer is exactly what printing the manifest as JSON prints for the same repository")]
    public void ThenTheAnswerIsTheManifest() =>
        AcceptanceTool.McpTextBlocks(_answers.Single()).Should().Equal(AcceptanceTool.FakeTools.ManifestOutput(AcceptanceTool.FakeTools.Pinned, Tree.Root));

    [Then("the answer is exactly what the command prints given that argument")]
    public void ThenTheAnswerIsTheCommandsOutputGivenTheArgument() =>
        AcceptanceTool.McpTextBlocks(_answers.Single()).Should().Equal(_expected());

    [Then("no command is run")]
    public void ThenNoCommandIsRun() => Fakes().Runs.Should().BeEmpty();

    [Then("its text is the message the command writes on standard error")]
    public void ThenItsTextIsStandardError()
    {
        var (_, stderr, exitCode) = RunPinnedTool(_command);
        exitCode.Should().Be(_expectedExit);
        stderr.Should().NotBeEmpty();
        LastFailure().Should().Be(stderr);
    }

    [Then("the second answer is exactly what listing the rules prints under the new pin")]
    public void ThenTheSecondAnswerIsTheRuleListUnderTheNewPin()
    {
        _answers.Should().HaveCount(2);
        AcceptanceTool.McpTextBlocks(_answers[1]).Should().Equal(AcceptanceTool.FakeTools.RulesOutput(AcceptanceTool.FakeTools.Pinned, Tree.Root));
        AcceptanceTool.McpTextBlocks(_answers[1]).Should().NotEqual(AcceptanceTool.McpTextBlocks(_answers[0]));
    }

    private void Explain(string rule) => Ask("explain", new JsonObject { ["rule"] = rule }, "--explain", rule);

    private const string DashedValue = "--dashed-value";
}
