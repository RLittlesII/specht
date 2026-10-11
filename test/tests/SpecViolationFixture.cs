using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Model;

namespace Specht.Tests;

/// <summary>
/// Builds a <see cref="SpecViolation"/>: a valid <c>SPEC031</c> error on line 1 of <c>a/spec.md</c> until a test overrides
/// what it asserts on. The generated defaults are <see langword="null"/>, and a violation with no rule, file or message is
/// not one the engine reports.
/// </summary>
[AutoFixture(typeof(SpecViolation))]
internal sealed partial class SpecViolationFixture
{
    public SpecViolationFixture() =>
        WithRuleId("SPEC031").WithSeverity(SpecSeverity.Error).WithFile("a/spec.md").WithLine(1).WithMessage("it is wrong");
}
