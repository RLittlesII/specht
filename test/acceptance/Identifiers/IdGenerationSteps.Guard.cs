using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Identifiers;

/// <summary>
/// Steps for item <c>0440</c> (<c>0001-F19</c> B-078 to B-080). "It takes the claims of a Feature that declares no prefix from
/// that Feature's Gherkin files" writes <c>features/one</c> with no claim prefix and sets <c>claim.source</c> to
/// <c>gherkin</c> with one claim file set, glob <c>features/one/.spec/*.feature</c>, whose identity is that Feature's, epic
/// <c>0001</c> and feature <c>F1</c>, and whose <c>record</c> is its <c>.spec/README.md</c>, so the set names the Feature
/// <c>--feature</c> names by both. "It takes claims under another Feature's identity from Gherkin files whose record is the
/// specification of a Feature that declares no prefix" writes the same Feature and the same set under feature <c>F2</c>, so the
/// set names <c>features/one</c> by its <c>record</c> alone (B-078, amended 2026-11-14). An element "in those files" that
/// declares the first candidate is written to a second claim file, <c>held.feature</c>, and the check's own reader must build
/// that claim from the set before the request is made. "No claim of that Feature" and "no claim in those files" are no claim the
/// check's reader builds from that set. A <c>mint</c> member that is no id type (B-079,
/// B-080) is added to the manifest's <c>mint</c> key beside nothing else, with no grammar under <c>identifiers</c>, and its
/// request launches the built tool.
/// </summary>
public sealed partial class IdGenerationSteps
{
    [Given("^its id settings opt the name \"(.*)\" in to Nano ID with a size of (\\d+)$")]
    public void GivenItsIdSettingsOptTheNameInToNanoIdWithASizeOf(string name, int size)
    {
        var manifest = ReadManifest();
        if (manifest["mint"] is not JsonObject mint)
        {
            mint = [];
            manifest["mint"] = mint;
        }

        mint[name] = new JsonObject { ["strategy"] = "nanoid", ["nanoid"] = new JsonObject { ["size"] = size } };
        WriteManifest(manifest);
    }

    [When("^an id of the type \"(.*)\" is requested$")]
    public void WhenAnIdOfTheTypeIsRequested(string type) => _request = Request(type, forFeature: false);

    [Given("^it takes the claims of a Feature that declares no prefix from that Feature's Gherkin files$")]
    public void GivenItTakesTheClaimsOfAFeatureThatDeclaresNoPrefixFromThatFeaturesGherkinFiles() => TakeClaimsUnder("F1");

    [Given("^it takes claims under another Feature's identity from Gherkin files whose record is the specification of a Feature that declares no prefix$")]
    public void GivenItTakesClaimsUnderAnotherFeaturesIdentityFromGherkinFilesWhoseRecordIsTheSpecificationOfAFeatureThatDeclaresNoPrefix() =>
        TakeClaimsUnder("F2");

    [Given("^(a scenario|a rule|the Feature) in those files already declares the claim id that would be minted first$")]
    public async Task GivenAnElementInThoseFilesAlreadyDeclaresTheClaimIdThatWouldBeMintedFirst(string element)
    {
        var first = await FirstCandidateForTheFeature("claim");
        Tree.WriteRaw(
            $"{Feature}/.spec/held.feature",
            element switch
            {
                "a scenario" => $"Feature: Held\n\n  @{first}\n  Scenario: It is held\n    Given a thing\n",
                "a rule" => $"Feature: Held\n\n  @{first}\n  Rule: It is held\n\n    Scenario: It is held\n      Given a thing\n",
                _ => $"@{first}\nFeature: Held\n\n  Scenario: It is held\n    Given a thing\n",
            });

        DeclaredClaims().Should().Contain(first);
    }

    [Then("^the minted id is one no claim (?:of that Feature|in those files) is declared with$")]
    public void ThenTheMintedIdIsOneNoClaimOfThatFeatureIsDeclaredWith()
    {
        var id = Minted();

        DeclaredClaims().Should().NotBeEmpty().And.NotContain(id);
    }

    private void TakeClaimsUnder(string feature)
    {
        _frontmatter = null;
        Tree.WriteCoLocatedFeature(Feature, "0001", "F1", _frontmatter);
        var manifest = ReadManifest();
        if (manifest["claim"] is not JsonObject claim)
        {
            claim = [];
            manifest["claim"] = claim;
        }

        claim["source"] = "gherkin";
        claim["files"] = new JsonArray(new JsonObject
        {
            ["glob"] = $"{Feature}/.spec/*.feature",
            ["epic"] = "0001",
            ["feature"] = feature,
            ["record"] = $"{Feature}/.spec/README.md",
        });
        WriteManifest(manifest);
    }

    private string[] DeclaredClaims() =>
        [.. EngineServices.Reader().Read(Tree.Root, null).ClaimSets
            .Where(static set => set.FileSet is { Glob: $"{Feature}/.spec/*.feature" })
            .SelectMany(static set => set.Rows)
            .Select(static claim => claim.Id)];
}
