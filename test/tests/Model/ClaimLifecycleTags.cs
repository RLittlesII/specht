namespace Specht.Tests.Model;

/// <summary>
/// The members of a schema 0.4.0 manifest that selects Gherkin claims, a claim file declaring <c>B-001</c>, and the
/// claim's status (<c>0001-F16</c> B-012; C-4): from the lifecycle tags the manifest lists on the declaring element, a
/// superseded tag winning over a withdrawn one; a tag on an element that does not declare the claim, and a tag the shipped
/// <c>0.4.0</c> defaults leave unlisted when the manifest names no list, give Active.
/// </summary>
public sealed class ClaimLifecycleTags : TheoryData<string, string, string>
{
    /// <summary>Initializes one row for each status and each place a lifecycle tag can sit.</summary>
    public ClaimLifecycleTags()
    {
        const string Lists = """{ "claim": { "withdrawnTags": ["@withdrawn"], "supersededTags": ["@superseded"] } }""";
        Add(Lists, "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n", "Active");
        Add(Lists, "Feature: Arithmetic\n\n  @B-001 @withdrawn\n  Scenario: Adds\n    Given two numbers\n", "Withdrawn");
        Add(Lists, "Feature: Arithmetic\n\n  @B-001 @superseded\n  Scenario: Adds\n    Given two numbers\n", "Superseded");
        Add(Lists, "Feature: Arithmetic\n\n  @B-001 @withdrawn @superseded\n  Scenario: Adds\n    Given two numbers\n", "Superseded");
        Add(Lists, "@B-001\nFeature: Arithmetic\n\n  @withdrawn\n  Scenario: Adds\n    Given two numbers\n", "Active");
        Add("{}", "Feature: Arithmetic\n\n  @B-001 @withdrawn @superseded\n  Scenario: Adds\n    Given two numbers\n", "Active");
    }
}
