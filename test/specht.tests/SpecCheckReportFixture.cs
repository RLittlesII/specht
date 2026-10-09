using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace specht.tests;

/// <summary>
/// Builds a <see cref="SpecCheckReport"/>: pinned to schema version 1, with zero counts and no violation until a test adds
/// some. The generated default version is 0, which no shipped version carries.
/// </summary>
[AutoFixture(typeof(SpecCheckReport))]
internal sealed partial class SpecCheckReportFixture
{
    public SpecCheckReportFixture() => WithSchemaVersion(1);

    /// <summary>Sets the violations, in the runner's order.</summary>
    /// <remarks>The generator names the list's setter <c>WithList</c>, after its type; this names it after the report's.</remarks>
    /// <param name="violations">The violations.</param>
    /// <returns>The fixture.</returns>
    public SpecCheckReportFixture WithViolations(params IReadOnlyList<SpecViolation> violations) => WithList(violations);
}
