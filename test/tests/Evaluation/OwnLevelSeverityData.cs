using Specht.Manifest;
using Specht.Model;

namespace Specht.Tests.Evaluation;

/// <summary>
/// A manifest, the own level of the rule under <c>FAKE001</c>, and the severity its finding is reported at: the manifest's
/// level for the id, else the rule's own (<c>0001-F5</c> B-010, B-011, B-044).
/// </summary>
public sealed class OwnLevelSeverityData : TheoryData<string, string, SpechtRuleLevel, SpechtSeverity>
{
    /// <summary>Initializes the named, unnamed-beside-another and omitted-settings cases.</summary>
    public OwnLevelSeverityData()
    {
        Add(
            "an id set to error reports the finding of a rule whose own level is error at error",
            """{ "rules": { "FAKE001": "error" } }""",
            SpechtRuleLevel.Error,
            SpechtSeverity.Error);
        Add(
            "an id the rule settings do not name keeps the rule's own error beside one they lower",
            """{ "rules": { "FAKE002": "warning" } }""",
            SpechtRuleLevel.Error,
            SpechtSeverity.Error);
        Add(
            "an id the rule settings do not name keeps the rule's own warning beside one they raise",
            """{ "rules": { "FAKE002": "error" } }""",
            SpechtRuleLevel.Warning,
            SpechtSeverity.Warning);
        Add("a manifest leaving its rule settings out keeps the rule's own warning", "{}", SpechtRuleLevel.Warning, SpechtSeverity.Warning);
        Add("a manifest leaving its rule settings out keeps the rule's own error", "{}", SpechtRuleLevel.Error, SpechtSeverity.Error);
    }
}
