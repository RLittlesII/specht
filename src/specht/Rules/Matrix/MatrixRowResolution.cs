namespace Specht.Rules.Matrix;

/// <summary>A matrix row read by column position, with each named test's resolution.</summary>
/// <param name="Line">The row's one-based line in the specification.</param>
/// <param name="Claim">The first cell, the claim.</param>
/// <param name="Scenario">The second cell, the scenario description.</param>
/// <param name="Test">The third cell's visible text.</param>
/// <param name="Status">The fourth cell, the coverage status, without interpretation.</param>
/// <param name="Tests">The inline-code test names, in cell order, and their resolution evidence.</param>
public sealed record MatrixRowResolution(
    int Line, string Claim, string Scenario, string Test, string Status, IReadOnlyList<MatrixTestResolution> Tests);
