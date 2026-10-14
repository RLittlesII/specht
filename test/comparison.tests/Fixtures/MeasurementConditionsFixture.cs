using System.Collections.Immutable;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Specht.Comparison.Tests.Evidence;

[AutoFixture(typeof(MeasurementConditions))]
internal sealed partial class MeasurementConditionsFixture
{
    public MeasurementConditionsFixture() => WithOperatingSystem("Linux").WithRuntime("10.0.0").WithArchitecture("x64")
        .WithToolchain("out-of-process").WithGarbageCollection("workstation").WithJob("Short");
}
