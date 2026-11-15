namespace Specht.Tests.Model;

/// <summary>
/// A claim file text the Gherkin claim reader refuses, and the line its refusal should carry (<c>0001-F16</c> B-014,
/// B-015; § 7, item <c>0395</c>): a <c># language: de</c> header on line 1 and a <c># language: fr</c> header on line 2,
/// each at its own line; and at line <c>0</c> each cause B-015 names, no <c>Feature:</c> line, a second one, a ragged
/// table, a malformed tag token, an unterminated doc string, tags followed by no element and an <c>Examples:</c> with no
/// outline above it, and an empty file.
/// </summary>
public sealed class RefusedClaimTexts : TheoryData<string, int>
{
    /// <summary>Initializes one row per refused text.</summary>
    public RefusedClaimTexts()
    {
        Add("# language: de\n" + Adds, 1);
        Add("# A comment\n# language: fr\n" + Adds, 2);
        Add("  @B-001\n  Scenario: Adds\n    Given two numbers\n", 0);
        Add(Adds + "\nFeature: Again\n", 0);
        Add("Feature: Arithmetic\n\n  @B-001\n  Scenario Outline: Adds <a>\n    Given <a>\n\n    Examples:\n      | a |\n      | 1 | 2 |\n", 0);
        Add("Feature: Arithmetic\n\n  @B-001 smoke\n  Scenario: Adds\n    Given two numbers\n", 0);
        Add(Adds + "      \"\"\"\n      text\n", 0);
        Add(Adds + "\n  @B-002\n", 0);
        Add("Feature: Arithmetic\n\n    Examples:\n      | a |\n      | 1 |\n", 0);
        Add(string.Empty, 0);
    }

    private const string Adds = "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n";
}
