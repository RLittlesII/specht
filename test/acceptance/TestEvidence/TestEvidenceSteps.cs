using System.Linq;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Acceptance.Documentation;
using Specht.Analysis.TestEvidence;

namespace Specht.Acceptance.TestEvidence;

[Binding]
public sealed class TestEvidenceSteps
{
    [Given("a bound xUnit fact in a selected test project without Reqnroll")]
    public void GivenXunitFact()
    {
        UseBoundCase("M:Example.Tests.Sample.Run", "B-001");
        _artifact!["adapter"]!["id"] = "xunit.v3";
        _useRunnerCollector = true;
    }

    [Given("a bound xUnit theory discovering two data rows without Reqnroll")]
    public void GivenXunitTheory()
    {
        UseBoundCase("M:Example.Tests.Sample.Theory", "B-002");
        _artifact!["discovery"]!["cases"]!.AsArray().Clear();
        AddCase("theory-row-1", "M:Example.Tests.Sample.Theory", "row-1", "Theory(1)");
        AddCase("theory-row-2", "M:Example.Tests.Sample.Theory", "row-2", "Theory(2)");
        _artifact["discovery"]!["methods"]![0]!["symbolId"] = "M:Example.Tests.Sample.Theory";
        _artifact["bindings"]![0]!["symbolId"] = "M:Example.Tests.Sample.Theory";
        _artifact["adapter"]!["id"] = "xunit.v3";
        _artifact["results"] = new JsonObject { ["state"] = "absent" };
        _useRunnerCollector = true;
    }

    [Given("a bound Playwright .NET test with conforming runner evidence and no Reqnroll")]
    public void GivenPlaywrightTest()
    {
        UseBoundCase("M:Example.Tests.Sample.Playwright", "B-003");
        _artifact!["adapter"]!["id"] = "playwright.dotnet.xunit.v3";
        _useRunnerCollector = true;
    }

    [Given("a selected test project with missing collection integration or insufficient case and method metadata")]
    public void GivenUnavailableEvidence()
    {
        _artifact = JsonNode.Parse(AcceptanceTestEvidence.ForMethod("M:Example.Tests.Sample.Run"));
        _artifact!["discovery"]!["state"] = "unavailable";
        _artifact["discovery"]!["sessionId"] = null;
        _artifact["discovery"]!["methods"] = new JsonArray();
        _artifact["discovery"]!["cases"] = new JsonArray();
        _artifact["bindings"] = new JsonArray();
        _artifact["results"] = new JsonObject { ["state"] = "absent" };
    }

    [Given("two discovered cases with duplicate display names but distinct runner identities")]
    public void GivenDuplicateNames()
    {
        UseBoundCase("M:Example.Tests.Sample.Theory", "B-005");
        _artifact!["discovery"]!["cases"]!.AsArray().Clear();
        AddCase("runner-case-1", "M:Example.Tests.Sample.Theory", "row-1", "Same display name");
        AddCase("runner-case-2", "M:Example.Tests.Sample.Theory", "row-2", "Same display name");
        _artifact["discovery"]!["methods"]![0]!["symbolId"] = "M:Example.Tests.Sample.Theory";
        _artifact["bindings"]![0]!["symbolId"] = "M:Example.Tests.Sample.Theory";
        _artifact["results"] = new JsonObject { ["state"] = "absent" };
        _useRunnerCollector = true;
    }

    [Given("supplied {string} carrying source, assembly and configuration identities")]
    public void GivenSuppliedArtifact(string kind)
    {
        UseBoundCase("M:Example.Tests.Sample.Run", "B-006");
        if (kind == "discovery")
        {
            _artifact!["sourceFingerprint"] = new string('e', 64);
        }
        else
        {
            _artifact!["assembly"]!["sha256"] = new string('f', 64);
        }
    }

    [Given("conforming runner evidence from {string}")]
    public void GivenConformingRunnerEvidence(string framework)
    {
        UseBoundCase("M:Example.Tests.Sample.Run", "B-007");
        _artifact!["adapter"]!["id"] = framework;
        _useRunnerCollector = true;
    }

    [Given("a bound parameterized test whose cases are available only at execution")]
    public void GivenExecutionOnlyParameterizedTest()
    {
        GivenXunitTheory();
        _artifact!["discovery"]!["state"] = "deferred";
        _artifact["discovery"]!["cases"] = new JsonArray();
        _artifact["results"] = new JsonObject { ["state"] = "absent" };
    }

    [Given("collected test evidence in versioned JSON")]
    public void GivenVersionedEvidence()
    {
        UseBoundCase("M:Example.Tests.Sample.Run", "B-009");
        _useRunnerCollector = true;
    }

    [When("the declared analysis is requested")]
    public void WhenAnalysisIsRequested() => _result = _useRunnerCollector ? CollectRunnerEvidence() : Read();

    [When("case and method identities are mapped")]
    public void WhenIdentitiesAreMapped() => _result = CollectRunnerEvidence();

    [When("it is used for current analysis")]
    public void WhenUsedForCurrentAnalysis()
    {
        var current = new TestEvidenceAnalysisInputs("test/Example.Tests.csproj", "net10.0",
            new string('a', 64), new string('b', 64), new string('c', 64), new string('d', 64));
        _result = new TestEvidenceArtifactReader().Read(_artifact!.ToJsonString(), current);
    }

    [When("the runner has not supplied those cases")]
    public void WhenRunnerHasNotSuppliedCases() => _result = Read();

    [When("its identities are read")]
    public void WhenIdentitiesAreRead() => _result = CollectRunnerEvidence();

    [Then("its discovered test case is linked to that method and claim")]
    public void ThenCaseIsLinkedToMethodAndClaim()
    {
        var artifact = ValidArtifact();
        artifact.Discovery.Cases.Should().ContainSingle().Which.MethodSymbolId.Should()
            .Be(artifact.Discovery.Methods.Single().SymbolId);
        artifact.Bindings.Should().ContainSingle().Which.SymbolId.Should().Be(artifact.Discovery.Methods.Single().SymbolId);
        var binding = artifact.Bindings.Single();
        binding.Role.Should().Be("verifies");
        binding.Target!.Id.Should().Be(binding.Attributes["claim"]);
    }

    [Then("both test cases link to the same bound method without treating them as two methods")]
    public void ThenTheoryRowsShareOneMethod()
    {
        var discovery = ValidArtifact().Discovery;
        discovery.Methods.Should().ContainSingle();
        discovery.Cases.Should().HaveCount(2);
        discovery.Cases.Select(static item => item.MethodSymbolId).Distinct().Should().ContainSingle()
            .Which.Should().Be(discovery.Methods.Single().SymbolId);
    }

    [Then("the discovered case links through its CSharp method to the declared claim")]
    public void ThenPlaywrightCaseLinksToClaim() => ThenCaseIsLinkedToMethodAndClaim();

    [Then("evidence is reported as unavailable or unresolved rather than empty complete coverage")]
    public void ThenEvidenceIsNotComplete()
    {
        var discovery = ValidArtifact().Discovery;
        discovery.State.Should().Be(TestEvidenceDiscoveryState.Unavailable);
        discovery.Cases.Should().BeEmpty();
    }

    [Then("they remain distinct and correlate through runner identity and method metadata")]
    public void ThenDuplicateNamesRemainDistinct()
    {
        var cases = ValidArtifact().Discovery.Cases;
        cases.Select(static item => item.DisplayName).Distinct().Should().ContainSingle();
        cases.Select(static item => item.CaseId).Distinct().Should().HaveCount(2);
        cases.Select(static item => item.MethodSymbolId).Distinct().Should().ContainSingle();
    }

    [Then("its identities are checked against the current analysis inputs")]
    public void ThenIdentityMismatchIsStale() => _result!.IsCurrent.Should().BeFalse();

    [Then("the common evidence contract is used without a framework-name allowlist")]
    public void ThenFrameworkUsesCommonContract() => ValidArtifact().AdapterId.Should().NotBeNullOrEmpty();

    [Then("case enumeration remains incomplete")]
    public void ThenEnumerationIsDeferred() => ValidArtifact().Discovery.State.Should().Be(TestEvidenceDiscoveryState.Deferred);

    [Then("no cases or case counts are invented")]
    public void ThenNoCasesInvented() => ValidArtifact().Discovery.Cases.Should().BeEmpty();

    [Then("runner case IDs and method metadata are available")]
    public void ThenRunnerIdsAvailable()
    {
        var artifact = ValidArtifact();
        artifact.Discovery.Cases.Should().ContainSingle().Which.CaseId.Should().Be("acceptance-case");
        artifact.Discovery.Methods.Should().ContainSingle().Which.SymbolId.Should().Be("M:Example.Tests.Sample.Run");
    }

    [Then("project, framework, source, assembly, configuration, adapter version and run identities are available")]
    public void ThenAllIdentityMetadataIsAvailable()
    {
        var artifact = ValidArtifact();
        artifact.ProjectPath.Should().Be("test/tests.csproj");
        artifact.TargetFramework.Should().Be("net10.0");
        artifact.SourceFingerprint.Should().HaveLength(64);
        artifact.AssemblySha256.Should().HaveLength(64);
        artifact.ConfigurationFingerprint.Should().HaveLength(64);
        artifact.AdapterVersion.Should().Be("1");
        artifact.Discovery.SessionId.Should().Be("acceptance-session");
        artifact.Results.RunId.Should().Be("acceptance-run");
    }

    private void UseBoundCase(string symbolId, string claimId)
    {
        _artifact = JsonNode.Parse(AcceptanceTestEvidence.ForMethod(symbolId));
        _artifact!["bindings"]![0]!["attributes"]!["claim"] = claimId;
        _artifact["bindings"]![0]!["target"]!["id"] = claimId;
    }

    private void AddCase(string caseId, string methodId, string? rowId, string? displayName) =>
        _artifact!["discovery"]!["cases"]!.AsArray().Add(new JsonObject
        {
            ["caseId"] = caseId,
            ["methodSymbolId"] = methodId,
            ["rowId"] = rowId,
            ["displayName"] = displayName,
        });

    private TestEvidenceReadResult Read() => new TestEvidenceArtifactReader().Read(_artifact!.ToJsonString());

    private TestEvidenceReadResult CollectRunnerEvidence()
    {
        var reader = new TestEvidenceArtifactReader();
        var source = reader.Read(_artifact!.ToJsonString()).Artifact!;
        var collection = new TestEvidenceRunnerCollector().Collect(
            source.AdapterId,
            source.AdapterVersion,
            source.Discovery.Methods,
            source.Discovery.Cases.Select(static item => new TestEvidenceRunnerCaseRecord(
                item.CaseId, item.MethodSymbolId, item.RowId, item.DisplayName)).ToArray(),
            source.Discovery.SessionId,
            source.Discovery.State is TestEvidenceDiscoveryState.Complete,
            source.Results.RunId,
            source.Results.Cases.Select(static item => new TestEvidenceRunnerResultRecord(
                item.CaseId, item.Outcome, item.Detail)).ToArray());
        collection.IsValid.Should().BeTrue();
        var collected = source with
        {
            AdapterId = collection.AdapterId,
            AdapterVersion = collection.AdapterVersion,
            Discovery = collection.Discovery,
            Results = collection.Results,
        };
        return reader.Read(new TestEvidenceArtifactWriter().Write(collected));
    }

    private TestEvidenceArtifact ValidArtifact()
    {
        _result.Should().NotBeNull();
        _result!.IsValid.Should().BeTrue();
        return _result.Artifact!;
    }

    private JsonNode? _artifact;
    private TestEvidenceReadResult? _result;
    private bool _useRunnerCollector;
}
