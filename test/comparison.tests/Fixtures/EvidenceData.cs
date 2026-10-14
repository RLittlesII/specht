using System.Collections.Immutable;

namespace Specht.Comparison.Tests.Evidence;

internal static class EvidenceData
{
    public static (MeasurementInventory, SessionManifest) Change(string condition, MeasurementInventory inventory, SessionManifest session)
    {
        var evidence = inventory.Cases[0];
        var conditions = session.Current.Conditions;
        conditions = condition switch
        {
            "operating system" => conditions with { OperatingSystem = "other" },
            "runtime" => conditions with { Runtime = "other" },
            "architecture" => conditions with { Architecture = "other" },
            "toolchain" => conditions with { Toolchain = "other" },
            "garbage collection" => conditions with { GarbageCollection = "other" },
            "job" => conditions with { Job = "other" },
            _ => conditions,
        };
        evidence = condition switch
        {
            "fixture" => evidence with { Case = evidence.Case with { FixtureRevision = "2" } },
            "operation revision" => evidence with { Case = evidence.Case with { OperationRevision = "2" } },
            "boundary" => evidence with { Case = evidence.Case with { Boundary = "other" } },
            "observer" => evidence with { Observations = [.. evidence.Observations.Select(static x => x with { Observer = "other" })] },
            "confidence" => evidence with
            {
                Observations = [evidence.Observations[0] with { Interval = new TimingInterval(98, 102, 0.95m) },
                evidence.Observations[1]]
            },
            "unknown identity" => evidence with { IdentityKnown = false },
            _ => evidence,
        };
        return (inventory with { Cases = [evidence] }, session with { Current = session.Current with { Conditions = conditions } });
    }
}
