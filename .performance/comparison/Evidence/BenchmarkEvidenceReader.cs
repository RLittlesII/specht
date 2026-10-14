using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Specht.Comparison;

/// <summary>Decodes recognised full JSON against explicit native workload mappings.</summary>
public static class BenchmarkEvidenceReader
{
    public static MeasurementInventory Read(string json, string sourceFile, SessionSide side, ImmutableArray<IdentityMapping> mappings)
    {
        var cases = ImmutableArray.CreateBuilder<CaseEvidence>();
        if (!EvidencePaths.IsRelative(sourceFile)) return Unavailable("Exporter source path is not repository-relative.");
        if (side.ExporterVersion != "0.15.8") return Unavailable("Unsupported BenchmarkDotNet producer version.");
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            ComparisonJson.CheckDuplicateProperties(root);
            var host = root.GetProperty("HostEnvironmentInfo");
            if (Text(host, "BenchmarkDotNetVersion") is not ("0.15.8" or "0.15.8.0")
                || !string.Equals(Text(host, "Configuration"), "Release", StringComparison.OrdinalIgnoreCase)
                || !host.TryGetProperty("HasAttachedDebugger", out var debugger) || debugger.ValueKind != JsonValueKind.False)
                return Unavailable("Exporter version or Release/debugger evidence is unsupported.");
            var benchmarks = root.GetProperty("Benchmarks");
            if (benchmarks.ValueKind != JsonValueKind.Array) return Unavailable("Exporter inventory is malformed.");
            var index = 0;
            foreach (var benchmark in benchmarks.EnumerateArray())
            {
                var name = Text(benchmark, "FullName");
                if (string.IsNullOrWhiteSpace(name) || !benchmark.TryGetProperty("Measurements", out var measurements)
                    || measurements.ValueKind != JsonValueKind.Array)
                    return Unavailable("Unsupported full-JSON record shape.");
                var matching = mappings.Where(mapping => mapping.FullName == name && mapping.ProductCommit == side.ProductCommit
                    && EvidencePaths.IsRelative(mapping.SourceFile) && ComparisonJson.IsHash(mapping.SourceDigest, 64)).ToArray();
                var known = matching.Length == 1;
                var workload = known ? matching[0].Case : new WorkloadCase(name, "", "", "",
                    ImmutableSortedDictionary<string, ParameterValue>.Empty, "", [MetricKind.Time, MetricKind.ManagedAllocation]);
                _ = WorkloadIdentity.Key(workload);
                var location = $"/Benchmarks/{index}";
                var time = ReadTime(benchmark, sourceFile, location + "/Statistics");
                var allocation = ReadAllocation(benchmark, sourceFile, location + "/Memory");
                cases.Add(new CaseEvidence(workload, [time, allocation], known, known ? null : "Audited workload identity is unavailable."));
                index++;
            }
            foreach (var expected in side.ExpectedCases)
                if (!cases.Any(e => WorkloadIdentity.Key(e.Case) == WorkloadIdentity.Key(expected)))
                    cases.Add(new CaseEvidence(expected, [], true, "Expected native case has no exporter record."));
            return new MeasurementInventory(side.InventoryKnown, cases.ToImmutable(), []);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or System.Collections.Generic.KeyNotFoundException
            or System.IO.InvalidDataException or FormatException)
        {
            return Unavailable("Exporter document or identity mapping is malformed.");
        }

        MeasurementInventory Unavailable(string reason) => new(false, cases.ToImmutable(), [reason]);
    }

    private static Observation ReadTime(JsonElement benchmark, string source, string locator)
    {
        decimal? mean = null;
        TimingInterval? interval = null;
        if (benchmark.TryGetProperty("Statistics", out var statistics) && statistics.ValueKind == JsonValueKind.Object)
        {
            mean = Number(statistics, "Mean");
            if (statistics.TryGetProperty("ConfidenceInterval", out var confidence) && confidence.ValueKind == JsonValueKind.Object)
            {
                var lower = Number(confidence, "Lower");
                var upper = Number(confidence, "Upper");
                var level = Number(confidence, "Level");
                var probability = level switch
                {
                    0 => 0.50m,
                    1 => 0.70m,
                    2 => 0.75m,
                    3 => 0.80m,
                    4 => 0.85m,
                    5 => 0.90m,
                    6 => 0.92m,
                    7 => 0.95m,
                    8 => 0.96m,
                    9 => 0.97m,
                    10 => 0.98m,
                    11 => 0.99m,
                    12 => 0.999m,
                    _ => (decimal?)null,
                };
                if (lower is not null && upper is not null && probability is not null)
                    interval = new TimingInterval(lower.Value, upper.Value, probability.Value);
            }
        }
        var valid = mean > 0 && interval is not null && interval.Lower <= mean && interval.Upper >= mean;
        return new Observation(MetricKind.Time, mean, "ns", "benchmark-process", interval,
            valid ? null : "Missing or invalid timing value or uncertainty.", source, locator);
    }

    private static Observation ReadAllocation(JsonElement benchmark, string source, string locator)
    {
        var value = benchmark.TryGetProperty("Memory", out var memory) && memory.ValueKind == JsonValueKind.Object
            ? Number(memory, "BytesAllocatedPerOperation") : null;
        return new Observation(MetricKind.ManagedAllocation, value, "B", "benchmark-process", null,
            value >= 0 ? null : "Missing or invalid managed allocation.", source, locator);
    }

    private static string? Text(JsonElement value, string name) => value.ValueKind == JsonValueKind.Object
        && value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

    private static decimal? Number(JsonElement value, string name) => value.TryGetProperty(name, out var property)
        && property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var number) ? number : null;
}
