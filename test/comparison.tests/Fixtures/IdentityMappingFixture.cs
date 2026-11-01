using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Tests.Evidence;

namespace Specht.Comparison.Tests.Collection;

[AutoFixture(typeof(IdentityMapping))]
internal sealed partial class IdentityMappingFixture
{
    public IdentityMappingFixture() => WithFullName(ScriptedRevisions.EngineCase).WithProductCommit(new string('1', 40))
        .WithSourceFile("CheckBenchmarks.cs").WithSourceDigest(new string('d', 64)).WithCase(new WorkloadCaseFixture());
}
