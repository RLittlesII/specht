namespace Specht.Analysis.Tests.Gherkin;

public sealed class GherkinFindingCases : TheoryData<string, string, int, string>
{
    public GherkinFindingCases()
    {
        Add("Feature: Arithmetic\n\n  @REQ-FIX-001\n  Scenario: First\n    Given x\n\n  @REQ-FIX-001\n  Scenario: Second\n    Given y\n",
            "SPCT0010", 4, "Claim REQ-FIX-001 is declared more than once in Feature arithmetic.");
        Add("Feature: Arithmetic\n\n  @REQ-FIX-001\n  Scenario: First\n    Given x\n\n  @REQ-FIX-001\n  Scenario: Second\n    Given y\n",
            "SPCT0010", 8, "Claim REQ-FIX-001 is declared more than once in Feature arithmetic.");
        Add("Feature: Arithmetic\n\n  @REQ-FIX-001\n  Scenario Outline: Add\n    Given <a>\n\n    @REQ-FIX-006\n    Examples:\n      | a |\n      | 1 |\n",
            "SPCT0011", 7, "Tag @REQ-FIX-006 declares a claim where no claim can be declared.");
        Add("Feature: Arithmetic\n\n  Scenario: Unclaimed\n    Given x\n",
            "SPCT0012", 3, "Scenario \"Unclaimed\" declares no claim.");
    }
}
