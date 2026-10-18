using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using Reqnroll;
using Specht.Analysis.Bindings;

namespace Specht.Acceptance.Bindings;

[Binding]
[Scope(Tag = "0201-F1")]
public sealed class BindingSteps
{
    [Given("a manifest defining custom Feature and claim grammars and records using them")]
    public void GivenCustomGrammars()
    {
        _policy = new BindingPolicy("custom", new Dictionary<string, string>
        {
            ["epic"] = "area[0-9]",
            ["feature"] = "cap:[a-z]+",
            ["claim"] = "need/[0-9]+",
        }, _policy.Markers, true, "3. Acceptance Criteria", _policy.Omissions);
        _features.Add(new BindingFeatureReader().Read(
            "---\nepic: area2\nid: cap:read\nspec_status: draft\n---\n\n## 3. Acceptance Criteria\n\n"
            + "| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n| need/7 | A need | Session | Active |\n",
            "src/custom/.spec/README.md", _policy));
        _claim = new ClaimReference("area2", "cap:read", "need/7");
    }

    [Given("two Features declaring the same local claim ID")]
    public void GivenRepeatedClaims()
    {
        _features.Add(new BindingFeature(new FeatureIdentity("0400", "F1"), "approved", new[] { new BindingClaim("B-001", "Active") },
            "src/one/.spec/README.md"));
        _features.Add(new BindingFeature(new FeatureIdentity("0400", "F2"), "approved", new[] { new BindingClaim("B-001", "Active") },
            "src/two/.spec/README.md"));
        _claim = new ClaimReference("0400", "F2", "B-001");
    }

    [Given("a consumer whose configured model has no epic tier")]
    public void GivenNoEpicTier()
    {
        _policy = new BindingPolicy(_policy.SchemaVersion, _policy.Identifiers, _policy.Markers, false, _policy.ClaimsSection, _policy.Omissions);
        _features.Add(new BindingFeature(new FeatureIdentity(null, "F1"), "draft", new[] { new BindingClaim("B-001", "Active") },
            "src/one/.spec/README.md"));
        _claim = new ClaimReference(null, "F1", "B-001");
    }

    [Given("a repository ADR and a Feature ADR sharing the same local identity")]
    public void GivenSharedAdrId()
    {
        _adrs.Add(new BindingAdr("ADR-0001", null, "accepted", ".spec/adr/0001.md"));
        _adrs.Add(new BindingAdr("ADR-0001", new FeatureIdentity("0400", "F1"), "accepted", "src/one/.spec/adr/0001.md"));
    }

    [Given("an ADR reference with no explicit Feature owner")]
    public void GivenRepositoryAdrReference() => _adr = new AdrReference(null, null, "ADR-0001");

    [Given("an ADR reference with an explicit Feature owner")]
    public void GivenFeatureAdrReference() => _adr = new AdrReference("0400", "F1", "ADR-0001");

    [Given("an enclosing type with a Feature authoring default")]
    public void GivenEnclosingDefault() => _authoringDefault = new FeatureIdentity("0400", "F9");

    [Given("the same specification inputs and declared references with no Reqnroll or test-framework assemblies")]
    public void GivenFrameworkIndependentInputs()
    {
        typeof(BindingResolver).Assembly.GetReferencedAssemblies().Select(static name => name.Name).Should()
            .NotContain(static name => name != null && (name.Contains("xunit", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Reqnroll", StringComparison.OrdinalIgnoreCase) || name.Contains("Playwright", StringComparison.OrdinalIgnoreCase)));
        GivenCurrentClaim("Active", "draft");
        _before = new BindingResolver(new BindingCatalog(_features, _adrs), _policy).Resolve(_claim!);
    }

    [Given("a claim reference with a Feature identity and a claim identity")]
    public void GivenSeparateComponents() => GivenCustomGrammars();

    [Given("an ADR reference selecting the repository")]
    public void GivenSelectingRepository() => _adr = new AdrReference(null, null, "ADR-0001");

    [Given("an ADR reference selecting a Feature")]
    public void GivenSelectingFeature() => _adr = new AdrReference("0400", "F1", "ADR-0001");

    [Given("the selected owner does not declare that ADR")]
    public void GivenNoSelectedAdr() => _adrs.Clear();

    [Given("another owner declares the same local ADR identity")]
    public void GivenOtherOwnerDeclaresAdr() => _adrs.Add(new BindingAdr("ADR-0001",
        _adr!.Feature is null ? new FeatureIdentity("0400", "F1") : null, "accepted", ".spec/adr/other.md"));

    [Given("a resolving reference to an {word} claim in a {word} Feature")]
    [Given("a resolving reference to a {word} claim in a {word} Feature")]
    public void GivenCurrentClaim(string claimState, string featureState)
    {
        _features.Add(new BindingFeature(new FeatureIdentity("0400", "F1"), featureState,
            new[] { new BindingClaim("B-001", claimState, 12) }, "src/one/.spec/README.md"));
        _claim = new ClaimReference("0400", "F1", "B-001");
        _rawState = claimState;
        _rawFeatureState = featureState;
    }

    [Given("a resolving reference to an accepted ADR")]
    public void GivenAcceptedAdr() => GivenAdrState("accepted");

    [Given("^a resolving reference to a (proposed|rejected|deprecated|superseded) ADR$")]
    public void GivenAdrState(string state)
    {
        _adrs.Add(new BindingAdr("ADR-0001", null, state, ".spec/adr/0001.md"));
        _adr = new AdrReference(null, null, "ADR-0001");
        _rawState = state;
    }

    [Given("a request to treat a {string} record as a checked relationship target")]
    public void GivenRequestedKind(string kind) => _requestedKind = kind;

    [Given("a consumer whose model declares an epic tier")]
    public void GivenEpicTier() => _tier = true;

    [Given("a consumer whose model declares no epic tier")]
    public void GivenAbsentEpicTier() => _tier = false;

    [Given("an otherwise complete reference to a claim with a valid epic")]
    public void GivenClaimWithValidEpic() => SetTierReference("claim", "0400");

    [Given("an otherwise complete reference to a claim with no epic")]
    public void GivenClaimWithoutEpic() => SetTierReference("claim", null);

    [Given("an otherwise complete reference to a claim with an epic")]
    public void GivenClaimWithForbiddenEpic() => SetTierReference("claim", "0400");

    [Given("an otherwise complete reference to a Feature ADR with a valid epic")]
    public void GivenAdrWithValidEpic() => SetTierReference("adr", "0400");

    [Given("an otherwise complete reference to a Feature ADR with no epic")]
    public void GivenAdrWithoutEpic() => SetTierReference("adr", null);

    [Given("an otherwise complete reference to a Feature ADR with an epic")]
    public void GivenAdrWithForbiddenEpic() => SetTierReference("adr", "0400");

    [Given("a persisted member reference missing a required owner component")]
    public void GivenIncompleteMember()
    {
        GivenCurrentClaim("Active", "draft");
        _claim = _claim! with { Feature = null };
    }

    [Given("its containing type supplies that component as an authoring default")]
    public void GivenDefaultOwner() => _authoringDefault = new FeatureIdentity("0400", "F1");

    [Given("a complete reference conforming to the consumer identity grammars")]
    public void GivenCompleteReference() => _claim = new ClaimReference("0400", "F1", "B-001");

    [Given("zero matching declarations in its selected owner")]
    public void GivenZeroMatches() => SetMatches(0);

    [Given("one matching declarations in its selected owner")]
    public void GivenOneMatch() => SetMatches(1);

    [Given("two matching declarations in its selected owner")]
    public void GivenTwoMatches() => SetMatches(2);

    [Given("a resolving reference to a claim")]
    public void GivenClaimToMove() => GivenCurrentClaim("Active", "draft");

    [Given("a resolving reference to a repository ADR")]
    public void GivenRepositoryAdrToMove() => GivenAdrState("accepted");

    [Given("a resolving reference to a Feature ADR")]
    public void GivenFeatureAdrToMove()
    {
        _adrs.Add(new BindingAdr("ADR-0001", new FeatureIdentity("0400", "F1"), "accepted", "src/one/.spec/adr/0001.md"));
        _adr = new AdrReference("0400", "F1", "ADR-0001");
    }

    [Given("a resolving reference to a superseded claim naming a current replacement")]
    public void GivenSupersededClaim()
    {
        _features.Add(new BindingFeature(new FeatureIdentity("0400", "F1"), "approved", new[]
        {
            new BindingClaim("B-001", "Superseded", 12, "B-002"), new BindingClaim("B-002", "Active", 13),
        }, "src/one/.spec/README.md"));
        _claim = new ClaimReference("0400", "F1", "B-001");
    }

    [Given("a resolving reference to a superseded ADR naming a current replacement")]
    public void GivenSupersededAdr()
    {
        _adrs.Add(new BindingAdr("ADR-0001", null, "superseded", ".spec/adr/0001.md", 1, "ADR-0002"));
        _adrs.Add(new BindingAdr("ADR-0002", null, "accepted", ".spec/adr/0002.md"));
        _adr = new AdrReference(null, null, "ADR-0001");
    }

    [Given("an Active claim in an approved Feature with a reasoned coverage exemption")]
    public void GivenExemptClaim()
    {
        _features.Add(new BindingFeature(new FeatureIdentity("0400", "F1"), "approved",
            new[] { new BindingClaim("B-001", "Active", 12, null, "Only external hardware can prove it.") }, "src/one/.spec/README.md"));
        _claim = new ClaimReference("0400", "F1", "B-001");
    }

    [Given("a pinned schema permitting omitted lifecycle information with an established {word} interpretation")]
    public void GivenOmissionPolicy(string interpretation)
    {
        var eligible = interpretation == "eligible";
        _policy = new BindingPolicy("synthetic-pin", _policy.Identifiers, _policy.Markers, true, _policy.ClaimsSection,
            new LifecycleOmissions(eligible, eligible, eligible));
    }

    [Given("an existing target omitting that information")]
    public void GivenOmittedLifecycle()
    {
        _features.Add(new BindingFeature(new FeatureIdentity("0400", "F1"), null,
            new[] { new BindingClaim("B-001", null, 12) }, "src/one/.spec/README.md"));
        _claim = new ClaimReference("0400", "F1", "B-001");
    }

    [Given("a resolving reference whose claim has an explicit unrecognized lifecycle state")]
    public void GivenUnknownClaim() => GivenCurrentClaim("future-state", "approved");

    [Given("a resolving reference whose owning Feature has an explicit unrecognized lifecycle state")]
    public void GivenUnknownFeature() => GivenCurrentClaim("Active", "future-state");

    [Given("a resolving reference whose ADR has an explicit unrecognized lifecycle state")]
    public void GivenUnknownAdr() => GivenAdrState("future-state");

    [Given("an eligible claim in a draft Feature")]
    public void GivenEligibleDraft() => GivenCurrentClaim("Active", "draft");

    [When("the declared analysis is requested")]
    [When("the ADR reference is resolved")]
    [When("the reference is represented for analysis")]
    [When("target lifecycle eligibility is evaluated")]
    [When("its declared owner identity is validated")]
    [When("the member reference is resolved")]
    [When("the reference is resolved")]
    [When("its lifecycle eligibility is evaluated")]
    [When("a resolving reference to that claim is evaluated")]
    [When("a resolving reference to that target is evaluated")]
    [When("its binding is evaluated")]
    public void WhenResolved()
    {
        var resolver = new BindingResolver(new BindingCatalog(_features, _adrs), _policy);
        _result = _adr is null ? resolver.Resolve(_claim!) : resolver.Resolve(_adr);
    }

    [When("the target kind is interpreted")]
    public void WhenKindInterpreted() => _kind = new BindingResolver(new BindingCatalog(_features, _adrs), _policy).ReadTargetKind(_requestedKind!);

    [When("the specification record moves without changing its declared identity or owner")]
    public void WhenMoved()
    {
        WhenResolved();
        _before = Result;
        for (var index = 0; index < _features.Count; index++)
        {
            var feature = _features[index];
            _features[index] = new BindingFeature(feature.Identity, feature.State, feature.Claims, "moved/.spec/README.md");
        }
        for (var index = 0; index < _adrs.Count; index++)
        {
            _adrs[index] = _adrs[index] with { RelativePath = "moved/decision.md" };
        }
        WhenResolved();
    }

    [Then("references are validated and resolved using those configured grammars")]
    public void ThenCustomGrammars()
    {
        Result.Status.Should().Be(BindingResolution.Resolved);
        Result.Target!.Id.Should().Be("need/7");
        var resolver = new BindingResolver(new BindingCatalog(_features, _adrs), _policy);
        resolver.Resolve(_claim! with { Claim = "B-001" }).Status.Should().Be(BindingResolution.Invalid);
    }

    [Then("a qualified claim reference resolves only in its declared owning Feature")]
    public void ThenOwner() => Result.Target!.Feature.Should().Be("F2");

    [Then("a Feature and claim reference resolves without inventing an epic")]
    public void ThenNoEpic()
    {
        Result.Status.Should().Be(BindingResolution.Resolved);
        Result.Target!.Epic.Should().BeNull();
    }

    [Then("the selected owner is the repository")]
    public void ThenRepository() => Result.Target!.Feature.Should().BeNull();

    [Then("the selected owner is the named Feature")]
    public void ThenFeatureOwner()
    {
        Result.Target!.Feature.Should().Be("F1");
        Result.Target.Feature.Should().NotBe(_authoringDefault!.Feature);
    }

    [Then("reference resolution yields the same identities and findings")]
    public void ThenSameResult()
    {
        Result.Status.Should().Be(BindingResolution.Resolved);
        Result.Should().BeEquivalentTo(_before);
    }

    [Then("the epic, Feature and claim occupy separate identity components")]
    public void ThenSeparateComponents()
    {
        _claim!.Epic.Should().Be("area2");
        _claim.Feature.Should().Be("cap:read");
        _claim.Claim.Should().Be("need/7");
        Result.Status.Should().Be(BindingResolution.Resolved);
    }

    [Then("each supplied component uses its configured local grammar")]
    public void ThenLocalGrammars() => ThenCustomGrammars();

    [Then("the reference is reported as unresolved")]
    public void ThenUnresolved() => Result.Status.Should().Be(BindingResolution.Unresolved);

    [Then("the target is eligible for current evidence")]
    [Then("the claim remains eligible under the current-target lifecycle policy")]
    public void ThenEligible()
    {
        Result.Status.Should().Be(BindingResolution.Resolved);
        Result.IsEligible.Should().BeTrue();
    }

    [Then("the target is ineligible for current evidence")]
    [Then("it remains ineligible for current evidence")]
    public void ThenIneligible()
    {
        Result.Status.Should().Be(BindingResolution.Resolved);
        Result.IsEligible.Should().BeFalse();
    }

    [Then("its resolved identity and lifecycle state remain available")]
    [Then("its resolved identity and unrecognized state remain available")]
    public void ThenPreservedLifecycle()
    {
        Result.Target!.Id.Should().Be(_adr is null ? "B-001" : "ADR-0001");
        Result.Target.State.Should().Be(_rawState);
        Result.Target.FeatureState.Should().Be(_rawFeatureState);
    }

    [Then("no additional checked target kind is created")]
    public void ThenNoNewKind() => _kind.Should().BeNull();

    [Then("checked targets remain claims or ADRs")]
    public void ThenKindsRemain()
    {
        var resolver = new BindingResolver(new BindingCatalog(_features, _adrs), _policy);
        resolver.ReadTargetKind("claim").Should().Be(BindingTargetKind.Claim);
        resolver.ReadTargetKind("adr").Should().Be(BindingTargetKind.Adr);
    }

    [Then("its owner identity is complete")]
    public void ThenComplete() => Result.Status.Should().Be(BindingResolution.Resolved);

    [Then("its owner identity is incomplete")]
    [Then("its explicit target identity remains incomplete")]
    public void ThenIncomplete() => Result.Status.Should().Be(BindingResolution.Incomplete);

    [Then("its owner identity is invalid")]
    public void ThenInvalid() => Result.Status.Should().Be(BindingResolution.Invalid);

    [Then("no checked binding is obtained by borrowing the default")]
    public void ThenNoBorrowedBinding()
    {
        Result.Target.Should().BeNull();
        _claim!.Feature.Should().BeNull();
        _authoringDefault!.Feature.Should().Be("F1");
    }

    [Then("its resolution is {word}")]
    public void ThenResolution(string verdict) => Result.Status.ToString().ToLowerInvariant().Should().Be(verdict);

    [Then("the reference resolves to the same target identity")]
    public void ThenSameIdentity()
    {
        Result.Status.Should().Be(BindingResolution.Resolved);
        Result.Target!.Id.Should().Be(_before!.Target!.Id);
        Result.Target.Kind.Should().Be(_before.Target.Kind);
        Result.Target.Feature.Should().Be(_before.Target.Feature);
        Result.Target.Epic.Should().Be(_before.Target.Epic);
        Result.Target.RelativePath.Should().NotBe(_before.Target.RelativePath);
    }

    [Then("the binding retains the original target identity")]
    public void ThenOriginalIdentity() => Result.Target!.Id.Should().Be(_adr is null ? "B-001" : "ADR-0001");

    [Then("the Feature remains draft")]
    [Then("the eligibility result does not represent agreement approval")]
    public void ThenStillDraft()
    {
        Result.IsEligible.Should().BeTrue();
        Result.Target!.FeatureState.Should().Be("draft");
        _features.Single().State.Should().Be("draft");
    }

    [Given("^supplied Feature text and Feature and ADR declarations with (.*) provenance$")]
    public void GivenProvenanceCategory(string category)
    {
        _provenancePath = category switch
        {
            "a POSIX-rooted path" => '/' + "outside.md",
            "a Windows drive-qualified path" => "Q" + ':' + '\\' + "outside.md",
            "a UNC-rooted path" => new string('\\', 2) + "server" + '\\' + "outside.md",
            "a backslash-rooted path" => '\\' + "outside.md",
            "traversal above the root using forward slashes" => string.Join('/', "safe", "..", "..", "outside.md"),
            "traversal above the root using backslashes" => string.Join('\\', "safe", "..", "..", "outside.md"),
            "traversal above the root using mixed separators" => "safe" + '\\' + ".." + '/' + ".." + '\\' + "outside.md",
            "a nested repository-relative path" => "src/one/.spec/README.md",
            "a repository-relative path with backslash separators" => string.Join('\\', "src", "one", ".spec", "README.md"),
            "an internal parent segment that stays inside the root" => "src/old/../one/.spec/README.md",
            _ => throw new InvalidOperationException("Unknown provenance category."),
        };
    }

    [When("these binding inputs are prepared")]
    [When("these binding inputs are prepared and their declared targets resolve")]
    public void WhenProvenanceInputsPrepared()
    {
        const string text = "---\nepic: 0400\nid: F1\nspec_status: draft\n---\n\n## 3. Acceptance Criteria\n\n"
            + "| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n| B-001 | A need | Source | Active |\n";
        var path = _provenancePath ?? throw new InvalidOperationException("No provenance was supplied.");
        var original = new BindingAdr("ADR-0001", null, "accepted", ".spec/adr/0001.md");
        Action prepareText = () => _provenanceTextFeature = new BindingFeatureReader().Read(text, path, _policy);
        Action prepareFeature = () =>
        {
            var feature = new BindingFeature(new FeatureIdentity("0400", "F1"), "draft",
                new[] { new BindingClaim("B-001", "Active") }, path);
            var catalog = new BindingCatalog(new[] { feature }, Array.Empty<BindingAdr>());
            _provenanceResults.Add(new BindingResolver(catalog, _policy).Resolve(new ClaimReference("0400", "F1", "B-001")));
        };
        Action prepareAdr = () =>
        {
            var changed = original with { RelativePath = path };
            var catalog = new BindingCatalog(Array.Empty<BindingFeature>(), new[] { changed });
            _provenanceResults.Add(new BindingResolver(catalog, _policy).Resolve(new AdrReference(null, null, "ADR-0001")));
        };
        foreach (var prepare in new[] { prepareText, prepareFeature, prepareAdr })
        {
            try
            {
                prepare.Invoke();
            }
            catch (ArgumentException error)
            {
                _provenanceErrors.Add(error.Message);
            }
        }
        if (_provenanceTextFeature is not null && _provenanceErrors.Count == 0)
        {
            var catalog = new BindingCatalog(new[] { _provenanceTextFeature }, Array.Empty<BindingAdr>());
            _provenanceResults.Add(new BindingResolver(catalog, _policy).Resolve(new ClaimReference("0400", "F1", "B-001")));
        }
    }

    [Then("each input is rejected with a fixed error that omits the rejected value")]
    public void ThenEachInputRejected()
    {
        _provenanceErrors.Should().HaveCount(3);
        _provenanceErrors.Should().OnlyContain(static error => error == ProvenanceError);
        _provenanceErrors.Any(error => error.Contains(_provenancePath!, StringComparison.Ordinal)).Should().BeFalse();
    }

    [Then("the unsafe provenance cannot appear in a binding result")]
    public void ThenNoUnsafeResults() => _provenanceResults.Should().BeEmpty();

    [Then("each valid repository-relative provenance is retained in the results")]
    public void ThenValidProvenanceRetained()
    {
        _provenanceErrors.Should().BeEmpty();
        _provenanceResults.Should().HaveCount(3);
        _provenanceResults.Should().OnlyContain(static result => result.Status == BindingResolution.Resolved);
        _provenanceResults.Select(static result => result.Target!.RelativePath).Should().Equal(Enumerable.Repeat(_provenancePath, 3));
    }

    private BindingResult Result => _result ?? throw new InvalidOperationException("No reference was resolved.");

    private void SetTierReference(string kind, string? epic)
    {
        _policy = new BindingPolicy(_policy.SchemaVersion, _policy.Identifiers, _policy.Markers, _tier, _policy.ClaimsSection, _policy.Omissions);
        if (kind == "claim")
        {
            _features.Add(new BindingFeature(new FeatureIdentity(_tier ? "0400" : null, "F1"), "draft",
                new[] { new BindingClaim("B-001", "Active") }, "src/one/.spec/README.md"));
            _claim = new ClaimReference(epic, "F1", "B-001");
        }
        else
        {
            _adrs.Add(new BindingAdr("ADR-0001", new FeatureIdentity(_tier ? "0400" : null, "F1"), "accepted", "src/one/.spec/adr/0001.md"));
            _adr = new AdrReference(epic, "F1", "ADR-0001");
        }
    }

    private void SetMatches(int count) => _features.Add(new BindingFeature(new FeatureIdentity("0400", "F1"), "approved",
        Enumerable.Range(0, count).Select(static index => new BindingClaim("B-001", "Active", index + 12)).ToArray(), "src/one/.spec/README.md"));

    private const string ProvenanceError = "Binding provenance must be repository-relative and remain within the root.";

    private readonly List<BindingResult> _provenanceResults = new();
    private readonly List<string> _provenanceErrors = new();
    private readonly List<BindingFeature> _features = new();
    private readonly List<BindingAdr> _adrs = new();
    private BindingPolicy _policy = new("0.5.0", new Dictionary<string, string>
    {
        ["epic"] = "^[0-9]{4}$",
        ["feature"] = "^F[0-9]+$",
        ["claim"] = "^B-[0-9]{3}$",
    }, new Dictionary<string, string> { ["approved"] = "approved", ["withdrawn"] = "Withdrawn", ["superseded"] = "Superseded" },
        true, "3. Acceptance Criteria", new LifecycleOmissions(false, false, false));
    private ClaimReference? _claim;
    private AdrReference? _adr;
    private BindingResult? _result;
    private BindingResult? _before;
    private FeatureIdentity? _authoringDefault;
    private string? _requestedKind;
    private BindingTargetKind? _kind;
    private string? _rawState;
    private string? _rawFeatureState;
    private bool _tier;
    private string? _provenancePath;
    private BindingFeature? _provenanceTextFeature;
}
