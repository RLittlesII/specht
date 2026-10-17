namespace Specht.Rules.Matrix;

/// <summary>A named test's text resolution and its evidence, without a rule verdict.</summary>
/// <param name="Name">The name the matrix cell carries, including dotted qualifiers.</param>
/// <param name="Status">Whether the test resolves or test-file selection is unconfigured.</param>
/// <param name="File">The root-relative matching file, only for a resolved test.</param>
/// <param name="Line">The matching declaration's one-based line, only for a resolved test.</param>
public sealed record MatrixTestResolution(string Name, MatrixTestStatus Status, string? File = null, int? Line = null);
