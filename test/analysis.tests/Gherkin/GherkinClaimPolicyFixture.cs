using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Analysis.Bindings.Gherkin;

namespace Specht.Analysis.Tests.Gherkin;

/// <summary>
/// Builds a <see cref="GherkinClaimPolicy"/>: the <c>REQ-FIX-nnn</c> claim form, both scope forms, the <c>@ignore</c> planned
/// tag and one file set, until a test overrides what it asserts on.
/// </summary>
/// <remarks>
/// Written on <see cref="AutoFixtureBase{TFixture}"/> rather than declared with <c>[AutoFixture]</c>: the policy has three
/// <see cref="IReadOnlyList{T}"/> of string properties and the generator names each setter <c>WithList</c>, which fails with
/// CS0111 (duplicate member).
/// </remarks>
internal sealed class GherkinClaimPolicyFixture : AutoFixtureBase<GherkinClaimPolicyFixture>
{
    public GherkinClaimPolicyFixture WithClaim(GherkinTagForm value) => With(ref _claim, value);

    public GherkinClaimPolicyFixture WithFeatureScope(GherkinTagForm? value) => With(ref _featureScope, value);

    public GherkinClaimPolicyFixture WithEpicScope(GherkinTagForm? value) => With(ref _epicScope, value);

    public GherkinClaimPolicyFixture WithPlannedTags(IReadOnlyList<string> value) => With(ref _plannedTags, value);

    public static implicit operator GherkinClaimPolicy(GherkinClaimPolicyFixture fixture) => fixture.Build();

    private GherkinClaimPolicy Build() => new(_claim, _featureScope, _epicScope, _plannedTags, _withdrawnTags, _supersededTags,
        _fileSets);

    private GherkinTagForm _claim = new("@{claim}", "claim", false, "^REQ-FIX-[0-9]{3}$");

    private GherkinTagForm? _featureScope = new("@spec.feature:{feature}", "feature", false, null);

    private GherkinTagForm? _epicScope = new("@spec.epic:{epic}", "epic", false, null);

    private IReadOnlyList<string> _plannedTags = ["@ignore"];

    private IReadOnlyList<string> _withdrawnTags = ["@withdrawn"];

    private IReadOnlyList<string> _supersededTags = ["@superseded"];

    private IReadOnlyList<GherkinClaimFileSet> _fileSets = [new("specifications/*.feature", "arithmetic", null, null)];
}
