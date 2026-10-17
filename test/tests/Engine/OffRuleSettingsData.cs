using Specht.Model;

namespace Specht.Tests.Engine;

/// <summary>Manifest overrides and reported ids for a rule whose own level is off (0001-F5 B-061, B-062).</summary>
public sealed class OffRuleSettingsData : TheoryData<string, string, string[], bool, string[], SpecSeverity>
{
    /// <summary>Initializes the omission, override, grouped-id and vocabulary boundaries.</summary>
    public OffRuleSettingsData()
    {
        Add("omitted rules keep the own off level", "{}", ["FAKE001"], false, [], SpecSeverity.Error);
        Add("an empty rules map keeps the own off level", """{ "rules": {} }""", ["FAKE001"], false, [], SpecSeverity.Error);
        Add("an explicit off keeps the rule disabled", """{ "rules": { "FAKE001": "off" } }""", ["FAKE001"], false, [], SpecSeverity.Error);
        Add("warning raises the rule", """{ "rules": { "FAKE001": "warning" } }""", ["FAKE001"], true, ["FAKE001"], SpecSeverity.Warning);
        Add("error raises the rule", """{ "rules": { "FAKE001": "error" } }""", ["FAKE001"], true, ["FAKE001"], SpecSeverity.Error);
        Add(
            "raising one grouped id keeps an omitted sibling off",
            """{ "rules": { "FAKE002": "warning" } }""",
            ["FAKE001", "FAKE002"],
            true,
            ["FAKE002"],
            SpecSeverity.Warning);
        Add(
            "raising one grouped id keeps an explicitly disabled sibling off",
            """{ "rules": { "FAKE001": "off", "FAKE002": "error" } }""",
            ["FAKE001", "FAKE002"],
            true,
            ["FAKE002"],
            SpecSeverity.Error);
        Add(
            "raising an unrelated id does not enable the off rule",
            """{ "rules": { "FAKE002": "warning" } }""",
            ["FAKE001"],
            false,
            [],
            SpecSeverity.Error);
        Add("a rule outside the vocabulary stays unrun", "{}", ["OUTSIDE"], false, [], SpecSeverity.Error);
    }
}
