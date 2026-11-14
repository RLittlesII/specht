namespace Specht.Tests.Identifiers;

/// <summary>
/// A claim file of the Feature <c>features/one</c> in which one element declares <c>B-aa</c>, the first <c>claim</c>
/// candidate a request for that Feature draws under a schema <c>0.4.0</c> manifest whose <c>claim.source</c> is
/// <c>gherkin</c> (<c>0001-F19</c> B-078; <c>0001-F16</c> B-001, B-004; item <c>0440</c>), one row per Examples row of B-078's
/// scenario: a scenario, a rule and the Feature.
/// </summary>
public sealed class NativeClaimsHoldingTheFirstCandidate : TheoryData<string>
{
    /// <summary>Initializes the scenario, rule and Feature rows.</summary>
    public NativeClaimsHoldingTheFirstCandidate()
    {
        Add("Feature: it\n\n  @B-aa\n  Scenario: It is held\n    Given a thing\n");
        Add("Feature: it\n\n  @B-aa\n  Rule: It is held\n\n    Scenario: It is held\n      Given a thing\n");
        Add("@B-aa\nFeature: it\n\n  Scenario: It is held\n    Given a thing\n");
    }
}
