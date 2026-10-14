using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Specht.Comparison;

/// <summary>Pairs the complete inventories while retaining evidence gaps.</summary>
public static class CasePairing
{
    public static ImmutableArray<PairedCase> Pair(MeasurementInventory previous, MeasurementInventory current, SessionManifest session)
    {
        var before = Group(previous.Cases, session.Previous.ExpectedCases);
        var after = Group(current.Cases, session.Current.ExpectedCases);
        return before.Keys.Union(after.Keys, StringComparer.Ordinal).Order(StringComparer.Ordinal).Select(key =>
        {
            before.TryGetValue(key, out var left);
            after.TryGetValue(key, out var right);
            if (!previous.Known || !current.Known || !session.Previous.InventoryKnown || !session.Current.InventoryKnown)
                return new PairedCase(key, PairingStatus.InsufficientEvidence, left, right, [], "Native inventory is unavailable.");
            if (left is null || right is null)
            {
                var present = left ?? right!;
                var others = left is null ? before : after;
                var sameSide = left is null ? after : before;
                if (others.Any(entry => !sameSide.ContainsKey(entry.Key) && entry.Value.Case.OperationId == present.Case.OperationId))
                    return new PairedCase(key, PairingStatus.NotComparable, left, right, [], "Workload input parameters differ.");
                return new PairedCase(key, left is null ? PairingStatus.Added : PairingStatus.Removed, left, right, [], null);
            }
            var reason = Equivalence(left, right, session);
            var required = left.Case.RequiredMetrics.Union(right.Case.RequiredMetrics)
                .Union(left.Observations.Select(static o => o.Metric)).Union(right.Observations.Select(static o => o.Metric)).Order();
            var metrics = required.Select(metric => PairMetric(metric, left, right, reason)).ToImmutableArray();
            var status = metrics.Any(static m => m.Status == PairingStatus.NotComparable) ? PairingStatus.NotComparable
                : metrics.Any(static m => m.Status == PairingStatus.InsufficientEvidence) ? PairingStatus.InsufficientEvidence
                : reason is not null ? PairingStatus.NotComparable : PairingStatus.Comparable;
            return new PairedCase(key, status, left, right, metrics, reason);
        }).ToImmutableArray();
    }

    private static Dictionary<string, CaseEvidence> Group(ImmutableArray<CaseEvidence> cases, ImmutableArray<WorkloadCase> expected)
    {
        var result = cases.GroupBy(static e => WorkloadIdentity.Key(e.Case), StringComparer.Ordinal).ToDictionary(static g => g.Key, static g =>
        {
            var first = g.First();
            return first with
            {
                Observations = g.SelectMany(static e => e.Observations).ToImmutableArray(),
                IdentityKnown = g.All(static e => e.IdentityKnown),
                Failure = g.Select(static e => e.Failure).FirstOrDefault(static failure => failure is not null),
            };
        }, StringComparer.Ordinal);
        foreach (var workload in expected)
            result.TryAdd(WorkloadIdentity.Key(workload), new CaseEvidence(workload, [], true, "Expected measurement is missing."));
        return result;
    }

    private static string? Equivalence(CaseEvidence left, CaseEvidence right, SessionManifest session)
    {
        if (!left.IdentityKnown || !right.IdentityKnown) return "Unknown identity mapping.";
        if (string.IsNullOrWhiteSpace(left.Case.OperationRevision) || left.Case.OperationRevision != right.Case.OperationRevision)
            return "Workload operation revision is unknown or differs.";
        if (string.IsNullOrWhiteSpace(left.Case.FixtureId) || string.IsNullOrWhiteSpace(left.Case.FixtureRevision)
            || left.Case.FixtureId != right.Case.FixtureId || left.Case.FixtureRevision != right.Case.FixtureRevision)
            return "Workload fixture is unknown or differs.";
        if (string.IsNullOrWhiteSpace(left.Case.Boundary) || left.Case.Boundary != right.Case.Boundary)
            return "Measurement boundary is unknown or differs.";
        var a = session.Previous.Conditions;
        var b = session.Current.Conditions;
        foreach (var (name, first, second) in new[]
        {
            ("operating system", a.OperatingSystem, b.OperatingSystem), ("runtime", a.Runtime, b.Runtime),
            ("architecture", a.Architecture, b.Architecture), ("toolchain", a.Toolchain, b.Toolchain),
            ("garbage collection", a.GarbageCollection, b.GarbageCollection), ("job", a.Job, b.Job),
        })
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second) || first != second)
                return $"Measurement {name} is unknown or differs.";
        return null;
    }

    private static MetricPair PairMetric(MetricKind metric, CaseEvidence left, CaseEvidence right, string? equivalence)
    {
        var before = left.Observations.Where(o => o.Metric == metric).ToImmutableArray();
        var after = right.Observations.Where(o => o.Metric == metric).ToImmutableArray();
        if (before.Length != 1 || after.Length != 1)
            return new MetricPair(metric, PairingStatus.InsufficientEvidence, before, after, "Missing or duplicate metric evidence.");
        var first = before[0];
        var second = after[0];
        if (!Valid(first) || !Valid(second))
            return new MetricPair(metric, PairingStatus.InsufficientEvidence, before, after, "Invalid metric value or timing uncertainty.");
        var mismatch = equivalence;
        if (string.IsNullOrWhiteSpace(first.Observer) || first.Observer != second.Observer) mismatch = "Metric observer is unknown or differs.";
        if (metric == MetricKind.Time && first.Interval!.ConfidenceLevel != second.Interval!.ConfidenceLevel)
            mismatch = "Timing confidence levels differ.";
        if (!MetricUnits.IsRecognised(metric, first.Unit) || !MetricUnits.IsRecognised(metric, second.Unit))
            mismatch = "Metric units are unknown or incompatible.";
        return new MetricPair(metric, mismatch is null ? PairingStatus.Comparable : PairingStatus.NotComparable, before, after, mismatch);
    }

    private static bool Valid(Observation observation) => observation.InvalidReason is null && observation.Value is >= 0
        && (observation.Metric != MetricKind.Time || observation.Value > 0 && observation.Interval is { } interval
            && interval.ConfidenceLevel is > 0 and < 1
            && interval.Lower <= observation.Value && interval.Upper >= observation.Value);
}
