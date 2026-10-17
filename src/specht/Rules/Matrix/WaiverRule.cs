using System.Globalization;
using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules.Matrix;

/// <summary>SPEC034 checks a waived matrix row's dated approver against the positional sign-off columns.</summary>
/// <param name="resolver">The shared, pure matrix resolver.</param>
public sealed class WaiverRule(MatrixResolver resolver) : ISpecRule
{
    /// <inheritdoc />
    public string Id => "SPEC034";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => [Id];

    /// <inheritdoc />
    public SpecRuleLevel DefaultLevel => SpecRuleLevel.Off;

    /// <inheritdoc />
    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        var structure = model.Schemas.Structure;
        var markers = structure.Markers;
        foreach (var feature in model.Features)
        {
            var signOff = feature.Document.Section(structure.Roles["signOff"]);
            var approvers = (signOff?.Rows ?? [])
                .Where(row => !string.Equals(row.ElementAtOrDefault(1)?.Trim(), markers["draft"], StringComparison.Ordinal)
                    && !string.Equals(row.ElementAtOrDefault(1)?.Trim(), markers["blocked"], StringComparison.Ordinal))
                .Select(static row => row.ElementAtOrDefault(2)?.Trim() ?? string.Empty)
                .Where(static name => name.Length > 0)
                .ToHashSet(StringComparer.Ordinal);
            foreach (var row in resolver.Resolve(feature.Document, structure, []))
            {
                if (!string.Equals(row.Status.Trim(), markers["waived"], StringComparison.Ordinal))
                {
                    continue;
                }

                var separator = row.Test.LastIndexOf(',');
                var approver = separator < 0 ? string.Empty : row.Test[..separator].Trim();
                var date = separator < 0 ? string.Empty : row.Test[(separator + 1)..].Trim();
                if (approvers.Contains(approver)
                    && DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    continue;
                }

                yield return new SpecViolation(
                    Id, SpecSeverity.Error, feature.RelativePath, row.Line, row.Claim.Trim(),
                    "waiver must name a signed-off approver and date as '<approver>, <YYYY-MM-DD>'");
            }
        }
    }
}
