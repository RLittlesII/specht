namespace Specht.Rules.Matrix;

/// <summary>The outcome of resolving a Scenario cell title against the companion scenarios.</summary>
/// <implements epic="0001" feature="F12" claim="B-008" />
public enum MatrixScenarioStatus
{
    /// <summary>The manifest does not turn title matching on; the cell is prose.</summary>
    Unconfigured,

    /// <summary>A scenario with the title carries the row's claim tag.</summary>
    Resolved,

    /// <summary>No <c>Scenario:</c> or <c>Scenario Outline:</c> in the companion has the title.</summary>
    NoSuchTitle,

    /// <summary>Scenarios have the title, and none carries the row's claim tag.</summary>
    Untagged,
}
