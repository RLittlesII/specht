namespace Specht.Rules.Matrix;

/// <summary>The outcome of resolving a matrix test name as text.</summary>
public enum MatrixTestStatus
{
    /// <summary>No test files were declared in the manifest.</summary>
    Unconfigured,

    /// <summary>No declared file holds the name in its required form.</summary>
    Unresolved,

    /// <summary>A declared file holds the name in its required form.</summary>
    Resolved,
}
