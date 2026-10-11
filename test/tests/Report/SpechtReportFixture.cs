using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Model;
using Specht.Report;
using Specht.Versioning;

namespace Specht.Tests.Report;

/// <summary>
/// Builds a <see cref="SpechtReport"/>: pinned to schema version 0.1.0, with the default manifest's two layouts at zero, zero
/// counts and no violation until a test adds some. The generated default version is 0.0.0, which no shipped version carries.
/// </summary>
[AutoFixture(typeof(SpechtReport))]
internal sealed partial class SpechtReportFixture
{
    public SpechtReportFixture() =>
        WithSchemaVersion(new SemanticVersion(0, 1, 0)).WithLayouts(new SpecReportLayout("epics", 0), new SpecReportLayout("features", 0));

    /// <summary>Sets each layout and its specification count, in the manifest's order.</summary>
    /// <remarks>The generator names the list's setter <c>WithList</c>, after its type; this names it after the report's.</remarks>
    /// <param name="layouts">The layouts.</param>
    /// <returns>The fixture.</returns>
    public SpechtReportFixture WithLayouts(params IReadOnlyList<SpecReportLayout> layouts) => WithList(layouts);

    /// <summary>Sets the violations, in the runner's order.</summary>
    /// <remarks>The generator names the list's setter <c>WithList</c>, after its type; this names it after the report's.</remarks>
    /// <param name="violations">The violations.</param>
    /// <returns>The fixture.</returns>
    public SpechtReportFixture WithViolations(params IReadOnlyList<SpecViolation> violations) => WithList(violations);
}
