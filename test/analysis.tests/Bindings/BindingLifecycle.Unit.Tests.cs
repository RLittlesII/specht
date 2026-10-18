using AwesomeAssertions;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

[Trait("Tier", "Unit")]
public sealed class BindingLifecycleUnitTests
{
    [Theory]
    [ClassData(typeof(ClaimLifecycleCases))]
    public void ClaimAndFeatureLifecycle_WhenEvaluated_ShouldSeparateResolutionFromEligibility(string claimState, string featureState, bool eligible)
    {
        // Given
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new BindingFeature[]
        {
            new BindingFeatureFixture().WithState(featureState)
                .WithClaims(new BindingClaim[] { new BindingClaimFixture().WithState(claimState) }),
        });
        var resolver = new BindingResolver(catalog, new BindingPolicyFixture());

        // When
        var result = resolver.Resolve(new ClaimReferenceFixture());

        // Then
        result.Status.Should().Be(BindingResolution.Resolved);
        result.IsEligible.Should().Be(eligible);
        result.Target!.State.Should().Be(claimState);
        result.Target.FeatureState.Should().Be(featureState);
    }

    [Theory]
    [ClassData(typeof(AdrLifecycleCases))]
    public void AdrLifecycle_WhenEvaluated_ShouldAdmitOnlyAcceptedDecisions(string state, bool eligible)
    {
        // Given
        BindingCatalog catalog = new BindingCatalogFixture().WithAdrs(new BindingAdr[] { new BindingAdrFixture().WithState(state) });
        var resolver = new BindingResolver(catalog, new BindingPolicyFixture());

        // When
        var result = resolver.Resolve(new AdrReferenceFixture());

        // Then
        result.Status.Should().Be(BindingResolution.Resolved);
        result.IsEligible.Should().Be(eligible);
        result.Target!.State.Should().Be(state);
    }

    [Theory]
    [ClassData(typeof(OmittedLifecycleCases))]
    public void PermittedMissingLifecycle_WhenEvaluated_ShouldUseOnlyItsPinnedOmissionPolicy(
        string kind, bool featureEligible, bool claimEligible, bool adrEligible, bool eligible)
    {
        // Given
        BindingPolicy policy = new BindingPolicyFixture().WithOmissions(new LifecycleOmissions(featureEligible, claimEligible, adrEligible));
        BindingFeature feature = new BindingFeatureFixture().WithState(kind == "Feature" ? null : "approved")
            .WithClaims(new BindingClaim[] { new BindingClaimFixture().WithState(kind == "claim" ? null : "Active") });
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new[] { feature })
            .WithAdrs(new BindingAdr[] { new BindingAdrFixture().WithState(null) });
        var resolver = new BindingResolver(catalog, policy);

        // When
        var result = kind == "ADR" ? resolver.Resolve(new AdrReferenceFixture()) : resolver.Resolve(new ClaimReferenceFixture());

        // Then
        result.Status.Should().Be(BindingResolution.Resolved);
        result.IsEligible.Should().Be(eligible);
        (kind == "Feature" ? result.Target!.FeatureState : result.Target!.State).Should().BeNull();
    }

    [Theory]
    [ClassData(typeof(UnknownLifecycleCases))]
    public void UnknownExplicitLifecycle_WhenEvaluated_ShouldRemainResolvedButIneligible(string kind)
    {
        // Given
        BindingPolicy policy = new BindingPolicyFixture().WithOmissions(new LifecycleOmissions(true, true, true));
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new BindingFeature[]
        {
            new BindingFeatureFixture().WithState(kind == "Feature" ? "future-state" : "approved")
                .WithClaims(new BindingClaim[] { new BindingClaimFixture().WithState(kind == "claim" ? "future-state" : "Active") }),
        }).WithAdrs(new BindingAdr[] { new BindingAdrFixture().WithState("future-state") });
        var resolver = new BindingResolver(catalog, policy);

        // When
        var result = kind == "ADR" ? resolver.Resolve(new AdrReferenceFixture()) : resolver.Resolve(new ClaimReferenceFixture());

        // Then
        result.Status.Should().Be(BindingResolution.Resolved);
        result.IsEligible.Should().BeFalse();
        (kind == "Feature" ? result.Target!.FeatureState : result.Target!.State).Should().Be("future-state");
    }

    [Theory]
    [ClassData(typeof(SupersededTargetCases))]
    public void SupersededTargetWithReplacement_WhenResolved_ShouldRetainTheOriginalTarget(string kind)
    {
        // Given
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new BindingFeature[]
        {
            new BindingFeatureFixture().WithClaims(new BindingClaim[]
            {
                new BindingClaimFixture().WithState("Superseded").WithReplacementId("B-002"),
                new BindingClaimFixture().WithId("B-002"),
            }),
        }).WithAdrs(new BindingAdr[]
        {
            new BindingAdrFixture().WithState("superseded").WithReplacementId("ADR-0002"),
            new BindingAdrFixture().WithId("ADR-0002"),
        });
        var resolver = new BindingResolver(catalog, new BindingPolicyFixture());

        // When
        var result = kind == "claim" ? resolver.Resolve(new ClaimReferenceFixture()) : resolver.Resolve(new AdrReferenceFixture());

        // Then
        result.Status.Should().Be(BindingResolution.Resolved);
        result.Target!.Id.Should().Be(kind == "claim" ? "B-001" : "ADR-0001");
        result.IsEligible.Should().BeFalse();
    }

    [Fact]
    public void EligibleClaimWithCoverageExemption_WhenEvaluated_ShouldRemainCurrent()
    {
        // Given
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new BindingFeature[]
        {
            new BindingFeatureFixture().WithClaims(new BindingClaim[]
            {
                new BindingClaimFixture().WithCoverageExemption("Only external hardware can prove this obligation."),
            }),
        });
        var resolver = new BindingResolver(catalog, new BindingPolicyFixture());

        // When
        var result = resolver.Resolve(new ClaimReferenceFixture());

        // Then
        result.IsEligible.Should().BeTrue();
        result.Target!.State.Should().Be("Active");
    }

    [Fact]
    public void EligibleDraftClaim_WhenEvaluated_ShouldPreserveAgreementMaturity()
    {
        // Given
        BindingFeature feature = new BindingFeatureFixture().WithState("draft");
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new[] { feature });
        var resolver = new BindingResolver(catalog, new BindingPolicyFixture());

        // When
        var result = resolver.Resolve(new ClaimReferenceFixture());

        // Then
        result.IsEligible.Should().BeTrue();
        result.Target!.FeatureState.Should().Be("draft");
        feature.State.Should().Be("draft");
    }

    [Theory]
    [ClassData(typeof(FeatureMarkerCases))]
    public void ConfiguredApprovedMarker_WhenEvaluated_ShouldReplaceDefaultSpelling(string state, bool eligible)
    {
        // Given
        BindingPolicy policy = new BindingPolicyFixture().WithMarkers(new Dictionary<string, string> { ["approved"] = "ratified" });
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new BindingFeature[] { new BindingFeatureFixture().WithState(state) });
        var resolver = new BindingResolver(catalog, policy);

        // When
        var result = resolver.Resolve(new ClaimReferenceFixture());

        // Then
        result.IsEligible.Should().Be(eligible);
        result.Target!.FeatureState.Should().Be(state);
    }

    [Theory]
    [ClassData(typeof(RetiredMarkerCases))]
    public void ConfiguredRetirementMarkers_WhenEvaluated_ShouldRetainHistoricalState(string state)
    {
        // Given
        BindingPolicy policy = new BindingPolicyFixture().WithMarkers(new Dictionary<string, string>
        {
            ["approved"] = "approved",
            ["withdrawn"] = "retired",
            ["superseded"] = "replaced",
        });
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new BindingFeature[]
        {
            new BindingFeatureFixture().WithClaims(new BindingClaim[] { new BindingClaimFixture().WithState(state) }),
        });
        var resolver = new BindingResolver(catalog, policy);

        // When
        var result = resolver.Resolve(new ClaimReferenceFixture());

        // Then
        result.Status.Should().Be(BindingResolution.Resolved);
        result.IsEligible.Should().BeFalse();
        result.Target!.State.Should().Be(state);
    }
}
