using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Mcp;

/// <summary>
/// The steps of Reading through MCP (0132-F4) that item 0433 delivers: a minted id through the tool <c>mint</c> (B-023) and the mint
/// command its call runs, for the Feature it names (B-025) or for none (B-026). The fake's mint prints its run's place in the world's log,
/// so the answer is compared with what the one run that call made printed and no other.
/// </summary>
public sealed partial class McpHostSteps
{
    [Given(@"^the repository's manifest opts (ADR|claim) ids in$")]
    public void GivenTheManifestOptsATypeIn(string type)
    {
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpechtTree.ManifestPath)))!.AsObject();
        manifest["mint"] = new JsonObject
        {
            [type == "ADR" ? "adr" : "claim"] = JsonNode.Parse(
                """{ "strategy": "nanoid", "nanoid": { "alphabet": "0123456789abcdefghjkmnpqrstvwxyz", "size": 8 } }"""),
        };
        Tree.WriteRaw(SpechtTree.ManifestPath, manifest.ToJsonString());
    }

    [When(@"^a new ADR id is asked for(?: naming no Feature)?$")]
    public void WhenANewAdrIdIsAskedFor() => Mint("adr", null);

    [When("a new claim id is asked for naming the folder of one of the two Feature specifications")]
    public void WhenANewClaimIdIsAskedForNamingAFeature() => Mint("claim", FeatureFolder);

    [Then("the answer is exactly what the pinned tool's mint command printed for that call")]
    public void ThenTheAnswerIsWhatTheMintRunPrinted()
    {
        var position = Fakes().Runs
            .Select(static (run, index) => (run, position: index + 1))
            .Where(static entry => !entry.run.IsVersionCheck)
            .Should()
            .ContainSingle()
            .Subject
            .position;
        AcceptanceTool.McpTextBlocks(_answers.Single())
            .Should()
            .Equal(AcceptanceTool.FakeTools.MintOutput(AcceptanceTool.FakeTools.Pinned, _mintType, position));
    }

    [Then("the pinned tool's mint command is run for claim ids in that Feature's folder")]
    public void ThenTheMintIsRunForTheFeature()
    {
        var run = Work().Should().ContainSingle().Subject;
        run.Label.Should().Be(AcceptanceTool.FakeTools.Pinned);
        run.Arguments.Split(' ').Should().HaveCount(4).And.StartWith(["mint", "claim"]).And.ContainInConsecutiveOrder("--feature", FeatureFolder);
    }

    [Then("the pinned tool's mint command is run for ADR ids naming no Feature")]
    public void ThenTheMintIsRunForNoFeature()
    {
        var run = Work().Should().ContainSingle().Subject;
        run.Label.Should().Be(AcceptanceTool.FakeTools.Pinned);
        run.Arguments.Should().Be("mint adr");
    }

    private void Mint(string type, string? feature)
    {
        _mintType = type;
        var arguments = new JsonObject { ["type"] = type };
        if (feature is not null)
        {
            arguments["feature"] = feature;
        }

        Ask("mint", arguments, feature is null ? ["mint", type] : ["mint", type, "--feature", feature]);
    }

    private const string FeatureFolder = "features/clean";

    private string _mintType = string.Empty;
}
