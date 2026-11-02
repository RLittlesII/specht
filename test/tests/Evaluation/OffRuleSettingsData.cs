using Specht.Model;

namespace Specht.Tests.Evaluation;

/// <summary>Manifest overrides for a rule whose own level is off, and the rule's id (0001-F5 B-061, B-062).</summary>
public sealed class OffRuleSettingsData : TheoryData<string, string, string, bool, string[], SpechtSeverity>
{
    /// <summary>Initializes the omission, override, unrelated-id and vocabulary boundaries.</summary>
    public OffRuleSettingsData()
    {
        Add("omitted rules keep the own off level", "{}", "FAKE001", false, [], SpechtSeverity.Error);
        Add("an empty rules map keeps the own off level", """{ "rules": {} }""", "FAKE001", false, [], SpechtSeverity.Error);
        Add("an explicit off keeps the rule disabled", """{ "rules": { "FAKE001": "off" } }""", "FAKE001", false, [], SpechtSeverity.Error);
        Add("warning raises the rule", """{ "rules": { "FAKE001": "warning" } }""", "FAKE001", true, ["FAKE001"], SpechtSeverity.Warning);
        Add("error raises the rule", """{ "rules": { "FAKE001": "error" } }""", "FAKE001", true, ["FAKE001"], SpechtSeverity.Error);
        Add("raising an unrelated id does not enable the off rule", """{ "rules": { "FAKE002": "warning" } }""", "FAKE001", false, [], SpechtSeverity.Error);
        Add("a rule outside the vocabulary stays unrun", "{}", "OUTSIDE", false, [], SpechtSeverity.Error);
    }
}
