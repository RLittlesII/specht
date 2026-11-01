using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Tests.Evidence;

namespace Specht.Comparison.Tests.Reporting;

[AutoFixture(typeof(IdentityMapping))]
internal sealed partial class IdentityMappingFixture
{
    public IdentityMappingFixture() => WithFullName("Synthetic.Check(1)").WithProductCommit(new string('a', 40)).WithSourceFile("Check.cs")
        .WithSourceDigest(new string('d', 64)).WithCase(new WorkloadCaseFixture());
}
