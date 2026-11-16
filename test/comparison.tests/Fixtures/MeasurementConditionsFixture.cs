using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Model;

namespace Specht.Comparison.Tests.Fixtures;

[AutoFixture(typeof(MeasurementConditions))]
internal sealed partial class MeasurementConditionsFixture
{
    public MeasurementConditionsFixture() => WithOperatingSystem("Linux").WithRuntime("10.0.0").WithArchitecture("x64")
        .WithToolchain("out-of-process").WithGarbageCollection("workstation").WithJob("Short");
}
