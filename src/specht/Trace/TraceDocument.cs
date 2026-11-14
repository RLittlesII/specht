using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Specht.Model;
using Specht.Rules.Matrix;

namespace Specht.Trace;

/// <summary>
/// The trace document (<c>0001-F13</c>): the shape <c>docs/schema/trace.schema.json</c> publishes, projected from one
/// <see cref="SpechtModel"/> through the check's matrix resolver. No rule runs, so it judges nothing, and it is a function
/// of the model alone, so the same tree gives the same text (B-007).
/// </summary>
/// <param name="Specifications">Every discovered Feature specification with a claims table, in discovery order.</param>
/// <implements epic="0001" feature="F13" claim="B-002" />
/// <implements epic="0001" feature="F13" claim="B-007" />
public sealed record TraceDocument(IReadOnlyList<FeatureTrace> Specifications)
{
    /// <summary>
    /// Makes the trace of <paramref name="model"/>: each Feature in discovery order, a native-claim Feature left out, and
    /// each of its claims in claims-table order with the first traceability row naming it, the scenarios carrying its tag
    /// and the tests the row names, every test of a waived, withdrawn or superseded row <c>skipped</c>.
    /// </summary>
    /// <param name="model">The model the check's own read gives.</param>
    /// <param name="resolver">The check's matrix resolver.</param>
    /// <returns>The document.</returns>
    /// <implements epic="0001" feature="F13" claim="B-003" />
    /// <implements epic="0001" feature="F13" claim="B-007" />
    /// <implements epic="0001" feature="F13" claim="B-019" />
    public static TraceDocument From(SpechtModel model, MatrixResolver resolver)
    {
        var identified = model.FeaturesWithIdentity.ToHashSet();
        var structure = model.Schemas.Structure;
        return new(model.Features
            .Where(static feature => feature.ClaimFileSet is null)
            .Select(feature => new FeatureTrace(
                feature.RelativePath,
                identified.Contains(feature) ? feature.Identity : null,
                resolver.ResolveClaims(feature, structure, model.Claims[feature], model.TestFiles)
                    .Select(claim => Claim(claim, structure.Markers))
                    .ToList()))
            .ToList());
    }

    /// <summary>Serializes the document as the JSON the published trace schema describes.</summary>
    /// <returns>The JSON text.</returns>
    /// <implements epic="0001" feature="F13" claim="B-002" />
    /// <implements epic="0001" feature="F13" claim="B-007" />
    public string ToJson() => JsonSerializer.Serialize(this, Options);

    /// <summary>
    /// Gives one line per claim, specification by specification and claim by claim in this document's order, each five
    /// tab-separated fields: path, claim id, the row's Status or <c>none</c>, the number of scenarios, and the resolved
    /// tests out of those not skipped. A tab, line feed or carriage return inside a field is written as one space.
    /// </summary>
    /// <returns>The lines, with no line terminator.</returns>
    /// <implements epic="0001" feature="F13" claim="B-001" />
    /// <implements epic="0001" feature="F13" claim="B-007" />
    public IReadOnlyList<string> Lines() =>
        Specifications
            .SelectMany(static specification => specification.Claims.Select(claim => string.Join(
                '\t',
                Field(specification.Path),
                Field(claim.Id),
                claim.Row is { } row ? Field(row.Status) : NoRow,
                claim.Scenarios.Count.ToString(CultureInfo.InvariantCulture),
                Tests(claim.Tests))))
            .ToList();

    /// <implements epic="0001" feature="F13" claim="B-003" />
    /// <implements epic="0001" feature="F13" claim="B-019" />
    private static ClaimTrace Claim(MatrixClaimResolution claim, IReadOnlyDictionary<string, string> markers)
    {
        var row = claim.Rows.FirstOrDefault();
        var skipped = row?.Skips(markers) == true;
        return new ClaimTrace(
            claim.Claim.Id,
            claim.Claim.Line,
            claim.Status,
            row is null ? null : new RowTrace(row.Line, row.Claim.Trim(), row.Scenario.Trim(), row.Test.Trim(), row.Status.Trim()),
            claim.Scenarios.Select(static scenario => new ScenarioTrace(scenario.Title, scenario.File!, scenario.Line!.Value)).ToList(),
            (row?.Tests ?? []).Select(test => Test(test, skipped)).ToList());
    }

    /// <implements epic="0001" feature="F13" claim="B-003" />
    /// <implements epic="0001" feature="F13" claim="B-019" />
    private static TestTrace Test(MatrixTestResolution test, bool skipped)
    {
        var resolution = skipped ? TraceResolution.Skipped : Resolution(test.Status);
        return resolution == TraceResolution.Resolved
            ? new TestTrace(test.Name, resolution, test.File, test.Line)
            : new TestTrace(test.Name, resolution, null, null);
    }

    /// <implements epic="0001" feature="F13" claim="B-003" />
    private static TraceResolution Resolution(MatrixTestStatus status) => status switch
    {
        MatrixTestStatus.Resolved => TraceResolution.Resolved,
        MatrixTestStatus.Unresolved => TraceResolution.Unresolved,
        _ => TraceResolution.Unconfigured,
    };

    /// <implements epic="0001" feature="F13" claim="B-001" />
    private static string Tests(IReadOnlyList<TestTrace> tests)
    {
        var resolved = tests.Count(static test => test.Resolution == TraceResolution.Resolved);
        var named = tests.Count(static test => test.Resolution != TraceResolution.Skipped);
        return string.Create(CultureInfo.InvariantCulture, $"{resolved}/{named}");
    }

    /// <implements epic="0001" feature="F13" claim="B-001" />
    private static string Field(string text) => text.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');

    private const string NoRow = "none";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        NewLine = "\n",
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}
