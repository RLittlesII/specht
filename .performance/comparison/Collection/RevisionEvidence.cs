using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Specht.Comparison;

/// <summary>Derives a revision's selection, expected inventory, measurement conditions and child environment.</summary>
public static class RevisionEvidence
{
    public static ImmutableArray<string> Select(ImmutableArray<string> listing) =>
        listing.Where(static line => line.StartsWith("Specht.Benchmarks.", StringComparison.Ordinal)).Distinct(StringComparer.Ordinal)
            .ToImmutableArray();

    public static bool NeedsTool(ImmutableArray<string> selected) =>
        selected.Any(static name => name.StartsWith("Specht.Benchmarks.Startup.", StringComparison.Ordinal));

    public static ImmutableArray<WorkloadCase>? Inventory(ImmutableArray<IdentityMapping> mappings, string commit, ImmutableArray<string> selected)
    {
        var mapped = selected.Select(name => mappings.Where(mapping => (mapping.FullName == name
                || mapping.FullName.StartsWith(name + "(", StringComparison.Ordinal)) && mapping.ProductCommit == commit
            && EvidencePaths.IsRelative(mapping.SourceFile) && ComparisonJson.IsHash(mapping.SourceDigest, 64)).ToArray()).ToArray();
        if (mapped.Any(static matching => matching.Length == 0))
        {
            return null;
        }

        try
        {
            var keyed = mapped.SelectMany(static matching => matching)
                .Select(static mapping => (Key: WorkloadIdentity.Key(mapping.Case), mapping.Case)).ToArray();
            return keyed.Select(static entry => entry.Key).Distinct(StringComparer.Ordinal).Count() == keyed.Length
                ? keyed.OrderBy(static entry => entry.Key, StringComparer.Ordinal).Select(static entry => entry.Case).ToImmutableArray()
                : null;
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public static ExportReading Read(ImmutableArray<string> exports)
    {
        if (exports.IsDefaultOrEmpty)
        {
            return new ExportReading(null, null, NoExport);
        }

        var declared = new List<(string Exporter, MeasurementConditions Conditions)>();
        foreach (var export in exports)
        {
            if (Declared(export) is not { } conditions)
            {
                return new ExportReading(null, null, Malformed);
            }

            declared.AddRange(conditions);
        }

        return declared.Distinct().Count() == 1
            ? new ExportReading(declared[0].Exporter, declared[0].Conditions, null)
            : new ExportReading(null, null, Disagreeing);
    }

    public static ImmutableSortedDictionary<string, string> ChildEnvironment(IReadOnlyDictionary<string, string> parent, string home,
        string temporary, string packages)
    {
        var environment = ImmutableSortedDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach (var (name, value) in parent)
        {
            if (Inherited.Contains(name) || name.StartsWith("DOTNET_ROOT", StringComparison.OrdinalIgnoreCase))
            {
                environment[name] = value;
            }
        }

        foreach (var name in new[] { "HOME", "USERPROFILE", "DOTNET_CLI_HOME" })
        {
            environment[name] = home;
        }

        foreach (var name in new[] { "TMPDIR", "TMP", "TEMP" })
        {
            environment[name] = temporary;
        }

        environment["NUGET_PACKAGES"] = packages;
        environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        environment["DOTNET_NOLOGO"] = "1";
        return environment.ToImmutable();
    }

    private static ImmutableArray<(string Exporter, MeasurementConditions Conditions)>? Declared(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("HostEnvironmentInfo", out var host) || host.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("Benchmarks", out var benchmarks) || benchmarks.ValueKind != JsonValueKind.Array
                || benchmarks.GetArrayLength() == 0
                || Required(host, "BenchmarkDotNetVersion") is not { } exporter || Required(host, "OsVersion") is not { } system
                || Required(host, "RuntimeVersion") is not { } runtime || Required(host, "Architecture") is not { } architecture
                || Required(host, "DotNetCliVersion") is not { } toolchain)
            {
                return null;
            }

            var conditions = ImmutableArray.CreateBuilder<(string, MeasurementConditions)>();
            foreach (var benchmark in benchmarks.EnumerateArray())
            {
                if (benchmark.ValueKind != JsonValueKind.Object || Job(Required(benchmark, "DisplayInfo")) is not { } job)
                {
                    return null;
                }

                conditions.Add((exporter, new MeasurementConditions(system, runtime, architecture, toolchain, GarbageCollection(job), job)));
            }

            return conditions.ToImmutable();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? Job(string? displayInfo)
    {
        var separator = displayInfo?.IndexOf(": ", StringComparison.Ordinal) ?? -1;
        if (separator < 0)
        {
            return null;
        }

        var segment = displayInfo![(separator + 2)..];
        var parameters = segment.EndsWith(']') ? segment.LastIndexOf(" [", StringComparison.Ordinal) : -1;
        if (parameters >= 0)
        {
            segment = segment[..parameters];
        }

        return string.IsNullOrWhiteSpace(segment) ? null : segment;
    }

    private static string GarbageCollection(string job)
    {
        var open = job.IndexOf('(', StringComparison.Ordinal);
        if (open < 0 || !job.EndsWith(')'))
        {
            return JobDefault;
        }

        var declared = job[(open + 1)..^1].Split(", ")
            .Where(static characteristic => GarbageCollectionCharacteristics.Contains(characteristic.Split('=')[0])).ToArray();
        return declared.Length == 0 ? JobDefault : string.Join(", ", declared);
    }

    private static string? Required(JsonElement value, string name) =>
        value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
        && property.GetString() is { } text && !string.IsNullOrWhiteSpace(text) ? text : null;

    private const string JobDefault = "Job default";
    private const string NoExport = "Benchmark run produced no full JSON export.";
    private const string Malformed = "Benchmark export is malformed or names no exporter version.";
    private const string Disagreeing = "Benchmark exports disagree on their measurement conditions.";

    private static readonly ImmutableHashSet<string> Inherited = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
        "PATH", "LANG", "LC_ALL", "SystemRoot", "windir", "ComSpec", "PATHEXT");

    private static readonly ImmutableHashSet<string> GarbageCollectionCharacteristics = ImmutableHashSet.Create(StringComparer.Ordinal,
        "Server", "Concurrent", "Force", "HeapCount", "CpuGroups", "AllowVeryLargeObjects", "RetainVm", "HeapAffinitizeMask", "NoAffinitize");
}
