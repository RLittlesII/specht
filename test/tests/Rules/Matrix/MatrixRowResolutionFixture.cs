using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Rules.Matrix;

namespace Specht.Tests.Rules.Matrix;

/// <summary>
/// Builds a <see cref="MatrixRowResolution"/>: row one of a matrix naming claim <c>B-001</c>, every cell empty and no test or
/// scenario, until a test overrides what it asserts on.
/// </summary>
[AutoFixture(typeof(MatrixRowResolution))]
internal sealed partial class MatrixRowResolutionFixture
{
    public MatrixRowResolutionFixture() =>
        WithLine(1).WithClaim("B-001").WithScenario(string.Empty).WithTest(string.Empty).WithStatus(string.Empty)
            .WithList(Array.Empty<MatrixTestResolution>()).WithList(Array.Empty<MatrixScenarioResolution>());
}
