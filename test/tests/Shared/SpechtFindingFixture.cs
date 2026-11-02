using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Rules;

namespace Specht.Tests.Shared;

/// <summary>
/// Builds a <see cref="SpechtFinding"/>: a finding on line 1 of <c>a/spec.md</c> with no identifier, the fields of the
/// <see cref="SpechtViolationFixture"/> default, until a test overrides what it asserts on. The generated defaults are
/// <see langword="null"/>, and a finding with no file or message is not one a rule gives.
/// </summary>
[AutoFixture(typeof(SpechtFinding))]
internal sealed partial class SpechtFindingFixture
{
    public SpechtFindingFixture() => WithFile("a/spec.md").WithLine(1).WithMessage("it is wrong");
}
