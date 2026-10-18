using AwesomeAssertions;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

[Trait("Tier", "Unit")]
public sealed class BindingResolverUnitTests
{
    [Fact]
    public void CustomLocalGrammars_WhenResolved_ShouldValidateWholeComponentsBeforeLookingUpTargets()
    {
        // Given
        BindingPolicy policy = new BindingPolicyFixture().WithIdentifiers(new Dictionary<string, string>
        {
            ["epic"] = "area[0-9]",
            ["feature"] = "cap:[a-z]+",
            ["claim"] = "need/[0-9]+",
        });
        BindingFeature feature = new BindingFeatureFixture().WithIdentity(new FeatureIdentity("area2", "cap:read"))
            .WithClaims(new BindingClaim[] { new BindingClaimFixture().WithId("need/7") });
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new[] { feature });
        var resolver = new BindingResolver(catalog, policy);

        // When
        var valid = resolver.Resolve(new ClaimReferenceFixture().WithEpic("area2").WithFeature("cap:read").WithClaim("need/7"));
        var invalid = resolver.Resolve(new ClaimReferenceFixture().WithEpic("area2").WithFeature("cap:read").WithClaim("prefixneed/7suffix"));
        var missing = resolver.Resolve(new ClaimReferenceFixture().WithEpic("area2").WithFeature("cap:read").WithClaim("need/8"));

        // Then
        valid.Status.Should().Be(BindingResolution.Resolved);
        valid.Target!.Feature.Should().Be("cap:read");
        valid.Target.Id.Should().Be("need/7");
        invalid.Status.Should().Be(BindingResolution.Invalid);
        invalid.IsEligible.Should().BeNull();
        missing.Status.Should().Be(BindingResolution.Unresolved);
    }

    [Theory]
    [ClassData(typeof(ReferenceValidationCases))]
    public void MissingOrMalformedComponents_WhenResolved_ShouldFailValidationBeforeCatalogLookup(
        string kind, string? epic, string? feature, string? id, BindingResolution expected)
    {
        // Given
        var resolver = new BindingResolver(new BindingCatalogFixture(), new BindingPolicyFixture());

        // When
        var result = kind == "claim"
            ? resolver.Resolve(new ClaimReferenceFixture().WithEpic(epic).WithFeature(feature).WithClaim(id))
            : resolver.Resolve(new AdrReferenceFixture().WithEpic(epic).WithFeature(feature).WithAdr(id));

        // Then
        result.Status.Should().Be(expected);
        result.Target.Should().BeNull();
        result.IsEligible.Should().BeNull();
        result.Matches.Should().BeEmpty();
    }

    [Fact]
    public void RepeatedClaimIds_WhenResolved_ShouldSelectOnlyTheExplicitOwner()
    {
        // Given
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new BindingFeature[]
        {
            new BindingFeatureFixture(),
            new BindingFeatureFixture().WithIdentity(new FeatureIdentity("0400", "F2")).WithRelativePath("src/two/.spec/README.md"),
        });
        var resolver = new BindingResolver(catalog, new BindingPolicyFixture());

        // When
        var result = resolver.Resolve(new ClaimReferenceFixture().WithFeature("F2"));

        // Then
        result.Status.Should().Be(BindingResolution.Resolved);
        result.Target!.Feature.Should().Be("F2");
        result.Target.RelativePath.Should().Be("src/two/.spec/README.md");
        result.Matches.Should().HaveCount(1);
    }

    [Theory]
    [ClassData(typeof(EpicComponentCases))]
    public void ConsumerEpicTier_WhenValidatingReferences_ShouldRequireOrRejectTheEpicComponent(
        bool hasTier, string kind, string? epic, BindingResolution expected)
    {
        // Given
        BindingPolicy policy = new BindingPolicyFixture().WithDeclaresEpicGrammar(hasTier);
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new BindingFeature[]
        {
            new BindingFeatureFixture().WithIdentity(new FeatureIdentity(hasTier ? "0400" : null, "F1")),
        }).WithAdrs(new BindingAdr[]
        {
            new BindingAdrFixture().WithOwner(new FeatureIdentity(hasTier ? "0400" : null, "F1")),
        });
        var resolver = new BindingResolver(catalog, policy);

        // When
        var result = kind == "claim"
            ? resolver.Resolve(new ClaimReferenceFixture().WithEpic(epic))
            : resolver.Resolve(new AdrReferenceFixture().WithEpic(epic).WithFeature("F1"));

        // Then
        result.Status.Should().Be(expected);
        if (expected == BindingResolution.Resolved)
        {
            result.Target!.Epic.Should().Be(hasTier ? "0400" : null);
        }
        else
        {
            result.Target.Should().BeNull();
            result.IsEligible.Should().BeNull();
        }
    }

    [Theory]
    [ClassData(typeof(AdrOwnerCases))]
    public void AdrOwnerComponents_WhenResolved_ShouldSelectOnlyTheDeclaredOwner(string? epic, string? feature, string path)
    {
        // Given
        BindingCatalog catalog = new BindingCatalogFixture().WithAdrs(new BindingAdr[]
        {
            new BindingAdrFixture(),
            new BindingAdrFixture().WithOwner(new FeatureIdentity("0400", "F1")).WithRelativePath("src/one/.spec/adr/0001-choice.md"),
        });
        var resolver = new BindingResolver(catalog, new BindingPolicyFixture());

        // When
        var result = resolver.Resolve(new AdrReferenceFixture().WithEpic(epic).WithFeature(feature));

        // Then
        result.Status.Should().Be(BindingResolution.Resolved);
        result.Target!.Feature.Should().Be(feature);
        result.Target.RelativePath.Should().Be(path);
    }

    [Theory]
    [ClassData(typeof(AdrFallbackCases))]
    public void AdrMissingFromSelectedOwner_WhenResolved_ShouldRemainUnresolved(string? selectedFeature, string? otherFeature)
    {
        // Given
        BindingCatalog catalog = new BindingCatalogFixture().WithAdrs(new BindingAdr[]
        {
            new BindingAdrFixture().WithOwner(otherFeature is null ? null : new FeatureIdentity("0400", otherFeature)),
        });
        var resolver = new BindingResolver(catalog, new BindingPolicyFixture());

        // When
        var result = resolver.Resolve(new AdrReferenceFixture().WithFeature(selectedFeature).WithEpic(selectedFeature is null ? null : "0400"));

        // Then
        result.Status.Should().Be(BindingResolution.Unresolved);
        result.Matches.Should().BeEmpty();
        result.IsEligible.Should().BeNull();
    }

    [Fact]
    public void MissingMemberOwner_WhenResolved_ShouldRemainIncompleteWithoutBorrowingContext()
    {
        // Given
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new BindingFeature[] { new BindingFeatureFixture() });
        var resolver = new BindingResolver(catalog, new BindingPolicyFixture());
        ClaimReference member = new ClaimReferenceFixture().WithFeature(null);

        // When
        var result = resolver.Resolve(member);

        // Then
        result.Status.Should().Be(BindingResolution.Incomplete);
        result.Target.Should().BeNull();
        member.Feature.Should().BeNull();
        result.IsEligible.Should().BeNull();
    }

    [Theory]
    [ClassData(typeof(TargetCountCases))]
    public void TargetCardinality_WhenResolved_ShouldDistinguishMissingUniqueAndAmbiguous(int count, BindingResolution expected)
    {
        // Given
        var claims = Enumerable.Range(0, count).Select(static index => (BindingClaim)new BindingClaimFixture().WithLine(index + 12)).ToArray();
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new BindingFeature[] { new BindingFeatureFixture().WithClaims(claims) });
        var resolver = new BindingResolver(catalog, new BindingPolicyFixture());

        // When
        var result = resolver.Resolve(new ClaimReferenceFixture());

        // Then
        result.Status.Should().Be(expected);
        result.Matches.Should().HaveCount(count);
        result.IsEligible.Should().Be(expected == BindingResolution.Resolved ? true : null);
    }

    [Theory]
    [ClassData(typeof(NoEpicOwnerCases))]
    public void IncidentalEpicMetadata_WhenEpicTierIsAbsent_ShouldNormalizeOwnersAndRetainAmbiguity(bool duplicate)
    {
        // Given
        BindingPolicy policy = new BindingPolicyFixture().WithDeclaresEpicGrammar(false);
        BindingFeature first = new BindingFeatureFixture();
        BindingFeature second = new BindingFeatureFixture().WithIdentity(new FeatureIdentity("0500", "F1"))
            .WithClaims(Array.Empty<BindingClaim>()).WithRelativePath("src/two/.spec/README.md");
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(duplicate ? new[] { first, second } : new[] { first })
            .WithAdrs(new BindingAdr[] { new BindingAdrFixture().WithOwner(new FeatureIdentity("0500", "F1")) });
        var resolver = new BindingResolver(catalog, policy);

        // When
        var claim = resolver.Resolve(new ClaimReferenceFixture().WithEpic(null));
        var adr = resolver.Resolve(new AdrReferenceFixture().WithEpic(null).WithFeature("F1"));

        // Then
        claim.Status.Should().Be(duplicate ? BindingResolution.Ambiguous : BindingResolution.Resolved);
        claim.IsEligible.Should().Be(duplicate ? null : true);
        if (!duplicate)
        {
            claim.Target!.Epic.Should().BeNull();
        }
        adr.Status.Should().Be(duplicate ? BindingResolution.Ambiguous : BindingResolution.Resolved);
        if (!duplicate)
        {
            adr.Target!.Epic.Should().BeNull();
        }
    }

    [Fact]
    public void DuplicateTargetsInDifferentOrders_WhenResolved_ShouldKeepStableProvenanceOrder()
    {
        // Given
        BindingAdr first = new BindingAdrFixture().WithRelativePath("z/decision.md").WithLine(3);
        BindingAdr second = new BindingAdrFixture().WithRelativePath("a/decision.md").WithLine(9);
        BindingPolicy policy = new BindingPolicyFixture();
        var forward = new BindingResolver(new BindingCatalogFixture().WithAdrs(new[] { first, second }), policy);
        var reverse = new BindingResolver(new BindingCatalogFixture().WithAdrs(new[] { second, first }), policy);

        // When
        var a = forward.Resolve(new AdrReferenceFixture());
        var b = reverse.Resolve(new AdrReferenceFixture());

        // Then
        a.Status.Should().Be(BindingResolution.Ambiguous);
        a.Matches.Select(static target => target.RelativePath).Should().Equal("a/decision.md", "z/decision.md");
        b.Should().BeEquivalentTo(a, static options => options.WithStrictOrdering());
    }

    [Theory]
    [ClassData(typeof(TargetMoveCases))]
    public void RecordMovedWithoutIdentityChanges_WhenResolved_ShouldPreserveTheTarget(string kind)
    {
        // Given
        var owner = kind == "Feature ADR" ? new FeatureIdentity("0400", "F1") : null;
        BindingCatalog before = new BindingCatalogFixture().WithFeatures(new BindingFeature[] { new BindingFeatureFixture() })
            .WithAdrs(new BindingAdr[] { new BindingAdrFixture().WithOwner(owner) });
        BindingCatalog after = new BindingCatalogFixture().WithFeatures(new BindingFeature[]
        {
            new BindingFeatureFixture().WithRelativePath("moved/.spec/README.md"),
        }).WithAdrs(new BindingAdr[] { new BindingAdrFixture().WithOwner(owner).WithRelativePath("moved/decision.md") });
        BindingPolicy policy = new BindingPolicyFixture();

        // When
        var oldResolver = new BindingResolver(before, policy);
        var newResolver = new BindingResolver(after, policy);
        var oldResult = kind == "claim" ? oldResolver.Resolve(new ClaimReferenceFixture())
            : oldResolver.Resolve(new AdrReferenceFixture().WithEpic(owner?.Epic).WithFeature(owner?.Feature));
        var newResult = kind == "claim" ? newResolver.Resolve(new ClaimReferenceFixture())
            : newResolver.Resolve(new AdrReferenceFixture().WithEpic(owner?.Epic).WithFeature(owner?.Feature));

        // Then
        newResult.Status.Should().Be(BindingResolution.Resolved);
        newResult.Target!.Kind.Should().Be(oldResult.Target!.Kind);
        newResult.Target.Epic.Should().Be(oldResult.Target.Epic);
        newResult.Target.Feature.Should().Be(oldResult.Target.Feature);
        newResult.Target.Id.Should().Be(oldResult.Target.Id);
        newResult.Target.RelativePath.Should().NotBe(oldResult.Target.RelativePath);
    }

    [Theory]
    [ClassData(typeof(TargetKindCases))]
    public void RequestedTargetKind_WhenInterpreted_ShouldAdmitOnlyClaimsAndAdrs(string kind, BindingTargetKind? expected)
    {
        // Given
        var resolver = new BindingResolver(new BindingCatalogFixture(), new BindingPolicyFixture());

        // When
        var result = resolver.ReadTargetKind(kind);

        // Then
        result.Should().Be(expected);
    }

    [Fact]
    public void AdrGrammarSuppliedByPolicy_WhenValidated_ShouldValidateTheWholeLocalIdentity()
    {
        // Given
        BindingPolicy policy = new BindingPolicyFixture().WithIdentifiers(new Dictionary<string, string>
        {
            ["epic"] = "^[0-9]{4}$",
            ["feature"] = "^F[0-9]+$",
            ["claim"] = "^B-[0-9]{3}$",
            ["adr"] = "DEC-[0-9]+",
        });
        BindingCatalog catalog = new BindingCatalogFixture().WithAdrs(new BindingAdr[] { new BindingAdrFixture().WithId("DEC-7") });
        var resolver = new BindingResolver(catalog, policy);

        // When
        var accepted = resolver.Resolve(new AdrReferenceFixture().WithAdr("DEC-7"));
        var invalid = resolver.Resolve(new AdrReferenceFixture().WithAdr("xDEC-7"));

        // Then
        accepted.Status.Should().Be(BindingResolution.Resolved);
        invalid.Status.Should().Be(BindingResolution.Invalid);
    }
}
