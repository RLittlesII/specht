namespace Specht.Tests.Rules.NativeClaims;

/// <summary>
/// Manifest members under which no Feature-level scope form reads a claim file's <c>@spec.feature:F2</c> tag
/// (<c>0001-F16</c> B-019, OQ-20; § 7, item <c>0395</c>): no <c>claim.scopes.featureTag</c>, and a
/// <c>featureTag</c> with no <c>{feature}</c> and with two, which OQ-20 reads as no form.
/// </summary>
public sealed class FeatureTagFormsReadingNoTag : TheoryData<string>
{
    /// <summary>Initializes one row per form.</summary>
    public FeatureTagFormsReadingNoTag()
    {
        Add("{}");
        Add("""{ "claim": { "scopes": { "featureTag": "@spec.feature" } } }""");
        Add("""{ "claim": { "scopes": { "featureTag": "@{feature}:{feature}" } } }""");
    }
}
