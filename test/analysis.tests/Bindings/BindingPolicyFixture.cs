using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

internal sealed class BindingPolicyFixture : AutoFixtureBase<BindingPolicyFixture>
{
    public BindingPolicyFixture WithSchemaVersion(string version) => With(ref _schemaVersion, version);
    public BindingPolicyFixture WithIdentifiers(IReadOnlyDictionary<string, string> identifiers) => With(ref _identifiers, identifiers);
    public BindingPolicyFixture WithMarkers(IReadOnlyDictionary<string, string> markers) => With(ref _markers, markers);
    public BindingPolicyFixture WithDeclaresEpicGrammar(bool declaresEpicGrammar) => With(ref _declaresEpicGrammar, declaresEpicGrammar);
    public BindingPolicyFixture WithClaimsSection(string claimsSection) => With(ref _claimsSection, claimsSection);
    public BindingPolicyFixture WithOmissions(LifecycleOmissions omissions) => With(ref _omissions, omissions);
    public static implicit operator BindingPolicy(BindingPolicyFixture fixture) => new(fixture._schemaVersion,
        fixture._identifiers, fixture._markers, fixture._declaresEpicGrammar, fixture._claimsSection, fixture._omissions);

    private string _schemaVersion = "0.5.0";
    private IReadOnlyDictionary<string, string> _identifiers = new Dictionary<string, string>
    {
        ["epic"] = "^[0-9]{4}$",
        ["feature"] = "^F[0-9]+$",
        ["claim"] = "^B-[0-9]{3}$",
    };
    private IReadOnlyDictionary<string, string> _markers = new Dictionary<string, string>
    {
        ["approved"] = "approved",
        ["withdrawn"] = "Withdrawn",
        ["superseded"] = "Superseded",
    };
    private bool _declaresEpicGrammar = true;
    private string _claimsSection = "3. Acceptance Criteria";
    private LifecycleOmissions _omissions = new(false, false, false);
}
