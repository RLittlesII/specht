using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Reconciliation;

public sealed partial class ReconciliationSteps
{
    [Given("a consumer whose claims are declared in Gherkin claim files and whose Feature records have no claims table")]
    public void GivenNativeClaims() => _tree.UseNativeClaims(BoundClaimFile);

    [Given("a passing test bound to each declared claim")]
    public void GivenBoundNativeClaims() { }

    [Given("a native claim carrying the consumer's {string} tag and no test bound to it")]
    public void GivenRetiredNativeClaim(string state) =>
        _tree.UseNativeClaims(BoundClaimFile + "\n  @need/8 @" + state + "\n  Scenario: Reads a retired value\n    Then it is read\n");

    [Given("a claim file set naming a Feature record whose lifecycle is not current")]
    public void GivenNativeClaimsOfARetiredRecord()
    {
        _tree.UseNativeClaims(BoundClaimFile, "superseded");
        _binding = false;
    }

    [Given("a native claim declared on a Scenario in a claim file")]
    public void GivenLocatedNativeClaim() =>
        _tree.UseNativeClaims("Feature: Read\n\n  Background:\n    Given a reader\n\n  @need/7\n  Scenario: Reads a value\n    Then it is read\n");

    [Given("a native claim whose Scenario carries the consumer's planned tag")]
    [Given("a native claim whose every case a claim file marks planned")]
    public void GivenPlannedNativeClaim() => _tree.UseNativeClaims(PlannedClaimFile);

    [Given("its discovered case was skipped and no scenario record names it")]
    public void GivenSkippedNativeCase() => _outcome = "skipped";

    [Given("no test bound to that claim")]
    public void GivenNoTestBoundToNativeClaim() => _binding = false;

    [Given("a selected Feature's claim file that {string}")]
    public void GivenRejectedClaimFile(string defect)
    {
        _tree.UseNativeClaims(defect switch
        {
            "is not valid Gherkin" => "Scenario: Orphan\n  Given a value\n",
            "declares a language other than English" =>
                "# language: fr\nFonctionnalité: Lire\n\n  @need/7\n  Scénario: Lit une valeur\n    Alors elle est lue\n",
            _ => BoundClaimFile,
        });
        _lockClaimFile = defect == "cannot be read";
    }

    [Given("a selected Feature {string}")]
    public void GivenFeatureWithoutClaimFiles(string gap)
    {
        _tree.UseNativeClaims(BoundClaimFile);
        _tree.SelectFeature("cap:list", gap == "whose claim file set matches no file" ? "src/list/.spec/*.feature" : null);
    }

    [Given("a claim file that two claim file sets match")]
    public void GivenClaimFileOfTwoSets()
    {
        _tree.UseNativeClaims(BoundClaimFile);
        _tree.SelectFeature("cap:list", "src/read/.spec/*.feature");
    }

    [Given("a selected Feature whose claim file set matches a claim file")]
    public void GivenSelectedFeatureWithClaimFile() => _tree.UseNativeClaims(BoundClaimFile);

    [Given("that claim file set names a Feature record that cannot be read")]
    public void GivenUnreadableFeatureRecord() => _tree.NameClaimFileSetRecord(UnreadableRecord);

    [Given("a claim file of a current selected Feature whose {string} tag names another identity than its claim file set")]
    public void GivenClaimFileTaggedForAnotherIdentity(string scope)
    {
        _tree.UseNativeClaims((scope == "epic" ? "@epic:ep2" : "@feature:cap:list") + "\n" + BoundClaimFile);
        _tree.ScopeClaimFilesToEpic("ep1");
    }

    [Given("a consumer reading native claims whose {string}")]
    public void GivenMalformedNativeClaimKey(string defect)
    {
        _tree.UseNativeClaims(BoundClaimFile);
        _tree.EditManifest(manifest =>
        {
            var records = manifest["codeAnalysis"]!["records"]!;
            switch (defect)
            {
                case "claim source is not Gherkin":
                    records["claimSource"] = "markdown";
                    break;
                case "claim tag has no claim placeholder":
                    manifest["claimTag"]!["tag"] = "@claim";
                    break;
                case "scope tag has no placeholder of its own":
                    manifest["codeAnalysis"]!["scenarioScopes"] = new JsonObject { ["featureTag"] = "@feature" };
                    break;
                case "planned tag list holds a tag without a leading @":
                    records["plannedTags"] = new JsonArray("ignore");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(defect), defect, null);
            }
        });
    }

    [Given("a consumer reading native claims whose claim file set {string}")]
    public void GivenMalformedClaimFileSetEntry(string defect)
    {
        _tree.UseNativeClaims(BoundClaimFile);
        _tree.EditManifest(manifest =>
        {
            var sets = manifest["codeAnalysis"]!["records"]!["claimFileSets"]!.AsArray();
            switch (defect)
            {
                case "has no glob":
                    sets[0]!.AsObject().Remove("glob");
                    break;
                case "names a Feature outside its grammar":
                    sets[0]!["feature"] = "Cap:Read";
                    break;
                case "names an epic though the manifest declares none":
                    sets[0]!["epic"] = "0201";
                    break;
                case "names a Feature another claim file set names":
                    sets.Add(new JsonObject { ["glob"] = "src/list/.spec/*.feature", ["feature"] = "cap:read" });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(defect), defect, null);
            }
        });
    }

    [Given("a consumer whose claims are declared in Feature records and whose manifest maps no claims section")]
    public void GivenMarkdownClaimsWithoutClaimsSection() =>
        _tree.EditManifest(static manifest => manifest["codeAnalysis"]!["mappings"]!.AsObject().Remove("/claimsSection"));

    [Given("a declared test project whose current evidence passed every required case")]
    public void GivenPassingTestProject() { }

    [Given("a declared production project that is not a test project")]
    [Given("no current evidence carries a claim's implementation bindings")]
    public void GivenProductionProject() => _tree.DeclareProduction("src/app/app.csproj");

    [Given("no current evidence for the production project")]
    public void GivenNoProductionEvidence() { }

    [Given("the consumer has explicitly enabled the implementation link requirement")]
    public void GivenImplementationRequired() => _tree.Require("implementation", true);

    [Then("every claim the claim files declare is reported with its test chain")]
    public void ThenNativeChain()
    {
        Report()["claims"]!.AsArray().Select(static claim => claim!["id"]!.GetValue<string>()).Should().Equal("need/7");
        Claim()["links"]!["test"]!["state"]!.GetValue<string>().Should().Be("satisfied");
        Claim()["cases"]!.AsArray().Select(static item => item!.GetValue<string>()).Should().Equal("case-1");
    }

    [Then("the claim is not selected and no evidence is required for it")]
    public void ThenRetiredNativeClaimNotSelected()
    {
        _result.ExitCode.Should().Be(0);
        Report()["claims"]!.AsArray().Select(static claim => claim!["id"]!.GetValue<string>()).Should().Equal("need/7");
        Kinds().Should().NotContain("missingRequiredLink");
    }

    [Then("none of that Feature's native claims is selected")]
    public void ThenNoNativeClaimSelected()
    {
        _result.ExitCode.Should().Be(0);
        Report()["claims"]!.AsArray().Should().BeEmpty();
        Kinds().Should().NotContain("missingRequiredLink");
    }

    [Then("the claim's source is that claim file's path and the line of that Scenario")]
    public void ThenNativeClaimLocation() =>
        (Claim()["path"]!.GetValue<string>(), Claim()["line"]!.GetValue<int>()).Should().Be((EvidenceTree.ClaimFilePath, 7));

    [Then("the case is not reported as planned")]
    public void ThenNativeCaseNotPlanned() =>
        (Row("test/tests.csproj", "case-1")["planned"]?.GetValue<bool>() ?? false).Should().BeFalse();

    [Then("the default trace gate fails for that case")]
    public void ThenNativeCaseFails()
    {
        _result.ExitCode.Should().Be(1);
        MissingResults().Should().ContainSingle().Which.Should().Contain("case-1");
    }

    [Then("the command fails with invalid input naming that claim file")]
    public void ThenClaimFileRejected() => ThenInvalidInputNaming(EvidenceTree.ClaimFilePath);

    [Then("the command fails with invalid input naming the manifest")]
    public void ThenManifestRejected() => ThenInvalidInputNaming(".specht/manifest.json");

    [Then("the command fails with invalid input naming that Feature record")]
    public void ThenFeatureRecordRejected() => ThenInvalidInputNaming(UnreadableRecord);

    [Then("no compilation is reported missing for the production project")]
    public void ThenProductionProjectNotMissing() => Report()["findings"]!.AsArray().Should().NotContain(static item =>
        item!["kind"]!.GetValue<string>() == "missingCompilation" && item["path"]!.GetValue<string>() == "src/app/app.csproj");

    [Then("that claim's implementation link is reported as a required gap")]
    public void ThenImplementationLinkGap()
    {
        Claim()["links"]!["implementation"]!["state"]!.GetValue<string>().Should().Be("missing");
        Kinds().Should().Contain("missingRequiredLink");
    }

    private void ThenInvalidInputNaming(string path)
    {
        _result.ExitCode.Should().Be(3);
        Report()["findings"]!.AsArray().Should().Contain(item => item!["kind"]!.GetValue<string>() == "invalidInput"
            && item["path"]!.GetValue<string>() == path);
    }

    private const string BoundClaimFile = "Feature: Read\n\n  @need/7\n  Scenario: Reads a value\n    Then it is read\n";
    private const string PlannedClaimFile = "Feature: Read\n\n  @need/7 @ignore\n  Scenario: Reads a value\n    Then it is read\n";
    private const string UnreadableRecord = "src/read/.spec/missing.md";

    private bool _lockClaimFile;
    private FileStream? _claimFileLock;
}
