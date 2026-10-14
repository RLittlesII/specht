using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Specht.Comparison;

/// <summary>Canonical operation and typed parameter identities, independent of labels.</summary>
public static class WorkloadIdentity
{
    public static string Key(WorkloadCase workload)
    {
        if (workload is null || string.IsNullOrWhiteSpace(workload.OperationId) || workload.Parameters is null
            || workload.RequiredMetrics.IsDefault || workload.RequiredMetrics.Distinct().Count() != workload.RequiredMetrics.Length)
            throw new InvalidDataException("Workload contract or parameters are invalid.");
        return ComparisonJson.Write(new
        {
            operation = workload.OperationId,
            parameters = workload.Parameters.OrderBy(static p => p.Key, StringComparer.Ordinal)
                .Select(static p => new { name = p.Key, kind = p.Value.Kind, value = CanonicalParameter(p.Value) }).ToArray(),
        });
    }

    private static string CanonicalParameter(ParameterValue value) => value.Kind switch
    {
        "number" when decimal.TryParse(value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            => number.ToString("G29", CultureInfo.InvariantCulture),
        "boolean" when bool.TryParse(value.Value, out var boolean) => boolean ? "true" : "false",
        "string" when value.Value is not null => value.Value,
        "null" when value.Value == "null" => "null",
        _ => throw new InvalidDataException("Workload parameter has an unsupported type or value."),
    };
}
