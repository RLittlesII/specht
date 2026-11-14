namespace Specht.Rules.Matrix;

/// <summary>A matrix row read by column position, with each named test's resolution.</summary>
/// <param name="Line">The row's one-based line in the specification.</param>
/// <param name="Claim">The first cell, the claim.</param>
/// <param name="Scenario">The second cell, the scenario description.</param>
/// <param name="Test">The third cell's visible text.</param>
/// <param name="Status">The fourth cell, the coverage status, without interpretation.</param>
/// <param name="Tests">The inline-code test names, in cell order, and their resolution evidence.</param>
/// <param name="Scenarios">The Scenario cell's titles, in cell order, and the scenarios they name.</param>
/// <implements epic="0001" feature="F12" claim="B-001" />
public sealed record MatrixRowResolution(
    int Line, string Claim, string Scenario, string Test, string Status, IReadOnlyList<MatrixTestResolution> Tests,
    IReadOnlyList<MatrixScenarioResolution> Scenarios)
{
    /// <summary>
    /// Whether the row's tests are skipped: its trimmed Status equals the waived, withdrawn or superseded marker, ordinally.
    /// A marker <paramref name="markers"/> lacks matches nothing.
    /// </summary>
    /// <param name="markers">The selected manifest's markers.</param>
    /// <returns>True when the row's tests are neither resolved nor judged.</returns>
    /// <implements epic="0001" feature="F12" claim="B-002" />
    /// <implements epic="0001" feature="F13" claim="B-019" />
    public bool Skips(IReadOnlyDictionary<string, string> markers)
    {
        var status = Status.Trim();
        return SkippingMarkers.Any(marker => markers.TryGetValue(marker, out var text) && string.Equals(status, text, StringComparison.Ordinal));
    }

    private static readonly string[] SkippingMarkers = ["waived", "withdrawn", "superseded"];
}
