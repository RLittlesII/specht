namespace Specht.Tests.Manifest;

/// <summary>
/// The members of a schema 0.4.0 manifest whose effective claim and scenario records share a head, which
/// <c>0001-F5</c> B-094 rejects: the same text ahead of <c>{id}</c> in both <c>plain</c>s or both <c>form</c>s, the same
/// text but for case while claim tags are compared ignoring it, and a scenario <c>plain</c> equal to the claim record's
/// default one.
/// </summary>
public sealed class SharedClaimAndScenarioHeads : TheoryData<string>
{
    /// <summary>Initializes the rows.</summary>
    public SharedClaimAndScenarioHeads()
    {
        Add("""{ "ids": { "claim": { "plain": "X-{id}" }, "scenario": { "plain": "X-{id}" } } }""");
        Add("""{ "ids": { "claim": { "form": "{prefix}-{id}" }, "scenario": { "form": "{prefix}-{id}" } } }""");
        Add("""{ "claim": { "tagIgnoreCase": true }, "ids": { "claim": { "plain": "X-{id}" }, "scenario": { "plain": "x-{id}" } } }""");
        Add("""{ "ids": { "scenario": { "plain": "B-{id}" } } }""");
    }
}
