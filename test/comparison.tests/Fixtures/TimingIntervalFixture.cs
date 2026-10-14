using System.Collections.Immutable;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Specht.Comparison.Tests.Evidence;

[AutoFixture(typeof(TimingInterval))]
internal sealed partial class TimingIntervalFixture
{
    public TimingIntervalFixture() => WithLower(98).WithUpper(102).WithConfidenceLevel(0.999m);
}
