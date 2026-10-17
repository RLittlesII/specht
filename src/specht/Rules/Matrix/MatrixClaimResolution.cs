using Specht.Model;

namespace Specht.Rules.Matrix;

/// <summary>A declared claim's status, matrix rows and matching companion tags from the parsed snapshot.</summary>
/// <param name="Claim">The claim's identity and declaration line.</param>
/// <param name="Status">The fourth claim-table cell.</param>
/// <param name="Rows">The matrix rows naming the claim.</param>
/// <param name="Tags">The companion tags matching under the configured tag-case setting.</param>
public sealed record MatrixClaimResolution(
    SpecClaim Claim, string Status, IReadOnlyList<MatrixRowResolution> Rows, IReadOnlyList<FeatureTag> Tags);
