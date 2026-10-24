using Specht.Analysis.Bindings.Gherkin;

namespace Specht.Analysis.Tests.Gherkin;

internal sealed class GherkinClaimPolicyFixture
{
    public GherkinClaimPolicyFixture WithFeatureScope(GherkinTagForm? value)
    {
        _featureScope = value;
        return this;
    }

    public GherkinClaimPolicyFixture WithEpicScope(GherkinTagForm? value)
    {
        _epicScope = value;
        return this;
    }

    public GherkinClaimPolicyFixture WithPlannedTags(IReadOnlyList<string> value)
    {
        _plannedTags = value;
        return this;
    }

    public static implicit operator GherkinClaimPolicy(GherkinClaimPolicyFixture fixture) => new(fixture._claim, fixture._featureScope,
        fixture._epicScope, fixture._plannedTags, ["@withdrawn"], ["@superseded"],
        [new GherkinClaimFileSet("specifications/*.feature", "arithmetic", null, null)]);

    private GherkinTagForm _claim = new("@{claim}", "claim", false, "^REQ-FIX-[0-9]{3}$");
    private GherkinTagForm? _featureScope = new("@spec.feature:{feature}", "feature", false, null);
    private GherkinTagForm? _epicScope = new("@spec.epic:{epic}", "epic", false, null);
    private IReadOnlyList<string> _plannedTags = ["@ignore"];
}
