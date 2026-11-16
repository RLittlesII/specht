using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Model;

namespace Specht.Comparison.Tests.Fixtures;

[AutoFixture(typeof(IdentityMapping))]
internal sealed partial class IdentityMappingFixture
{
    public IdentityMappingFixture() => WithFullName(ScriptedRevisions.EngineCase).WithProductCommit(new string('1', 40))
        .WithSourceFile("CheckBenchmarks.cs").WithSourceDigest(new string('d', 64)).WithCase(new WorkloadCaseFixture());
}
