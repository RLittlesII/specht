using Specht.Model;

namespace Specht.Rules.Matrix;

/// <summary>A declared claim's status, matrix rows and matching companion tags from the parsed snapshot.</summary>
/// <param name="Claim">The claim's identity and declaration line.</param>
/// <param name="Status">The fourth claim-table cell.</param>
/// <param name="Rows">The matrix rows naming the claim.</param>
/// <param name="Tags">The companion tags matching under the configured tag-case setting.</param>
/// <implements epic="0001" feature="F12" claim="B-001" />
public sealed record MatrixClaimResolution(
    SpechtClaim Claim, string Status, IReadOnlyList<MatrixRowResolution> Rows, IReadOnlyList<FeatureTag> Tags)
{
    /// <summary>
    /// Whether <c>SPEC4008</c>'s condition holds at <paramref name="row"/>: the claim is live, no companion tag carries it,
    /// and the row's Status is neither the missing nor the waived marker.
    /// </summary>
    /// <param name="row">A matrix row naming this claim.</param>
    /// <param name="markers">The selected manifest's markers.</param>
    /// <returns>True when the row is the missing-scenario rule's.</returns>
    /// <implements epic="0001" feature="F12" claim="B-011" />
    /// <implements epic="0001" feature="F12" claim="B-029" />
    public bool MissesScenarioAt(MatrixRowResolution row, IReadOnlyDictionary<string, string> markers)
    {
        var status = row.Status.Trim();
        return Tags.Count == 0
            && !string.Equals(Status, markers["withdrawn"], StringComparison.Ordinal)
            && !string.Equals(Status, markers["superseded"], StringComparison.Ordinal)
            && !string.Equals(status, markers["missing"], StringComparison.Ordinal)
            && !string.Equals(status, markers["waived"], StringComparison.Ordinal);
    }
}
