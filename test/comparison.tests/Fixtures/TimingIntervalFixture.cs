using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Model;

namespace Specht.Comparison.Tests.Fixtures;

[AutoFixture(typeof(TimingInterval))]
internal sealed partial class TimingIntervalFixture
{
    public TimingIntervalFixture() => WithLower(98).WithUpper(102).WithConfidenceLevel(0.999m);
}
