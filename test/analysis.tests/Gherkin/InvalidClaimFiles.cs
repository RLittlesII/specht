namespace Specht.Analysis.Tests.Gherkin;

public sealed class InvalidClaimFiles : TheoryData<string>
{
    public InvalidClaimFiles()
    {
        Add(string.Empty);
        Add("Feature: A\nFeature: B\n");
        Add("Some text\nFeature: A\n");
        Add("Feature: A\n\n  @REQ-FIX-001\n  Background:\n    Given x\n");
        Add("Feature: A\n\n  @REQ-FIX-001\n  Given x\n");
        Add("Feature: A\n\n  @REQ-FIX-001\n  | a |\n");
        Add("Feature: A\n\n  @REQ-FIX-001\n  A description line\n");
        Add("Feature: A\n\n  @REQ-FIX-001\n");
        Add("Feature: A\n\n  @ @REQ-FIX-001\n  Scenario: S\n    Given x\n");
        Add("Feature: A\n\n  Examples:\n    | a |\n");
        Add("Feature: A\n\n  Scenario Outline: S\n    Given <a>\n\n    Examples:\n      | a | b |\n      | 1 |\n");
        Add("Feature: A\n\n  Scenario: S\n    Given x\n      \"\"\"\n      still open\n");
        Add("Feature: A\n\n  Scenario: S\n    Given x\n\n  Rule: R\n\n    Examples:\n      | a |\n");
    }
}
