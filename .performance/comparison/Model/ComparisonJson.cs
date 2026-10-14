using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Specht.Comparison;

/// <summary>Versioned, canonical wire encoding for durable pair and session evidence.</summary>
public static class ComparisonJson
{
    public static string Write<T>(T value)
    {
        object? canonical = value is SessionManifest session
            ? session with { Previous = CanonicalSide(session.Previous), Current = CanonicalSide(session.Current) }
            : value;
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(canonical, Options));
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer)) WriteCanonical(document.RootElement, writer);
        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    public static string Digest<T>(T value) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(Write(value))));

    public static ReleasePair ReadPair(string json, ReleaseRecord current)
    {
        var pair = Read<ReleasePair>(json);
        ValidatePair(pair, current);
        return pair;
    }

    public static SessionManifest ReadSession(string json)
    {
        var session = Read<SessionManifest>(json);
        if (session.FormatVersion != 1 || !IsHash(session.PairDigest, 64) || !IsCommit(session.ComparatorCommit)
            || string.IsNullOrWhiteSpace(session.ComparatorVersion) || string.IsNullOrWhiteSpace(session.RunId)
            || session.Attempt < 1 || string.IsNullOrWhiteSpace(session.SessionId) || string.IsNullOrWhiteSpace(session.RunnerIdentity))
            throw new InvalidDataException("Session manifest identity or version is invalid.");
        ValidateSide(session.Previous);
        ValidateSide(session.Current);
        return session;
    }

    public static void ValidatePair(ReleasePair pair, ReleaseRecord current)
    {
        if (pair.FormatVersion != 1 || pair.SelectionPolicy != "publication-order-v1"
            || !IsHash(pair.CatalogDigest, 64) || string.IsNullOrWhiteSpace(pair.SelectingRunId))
            throw new InvalidDataException("Saved release pair has an unsupported version or invalid selection evidence.");
        ValidateRelease(pair.Current);
        if (pair.Current.Draft || pair.Current.PublishedAt is null
            || pair.Current.Repository != current.Repository || pair.Current.Id != current.Id)
            throw new InvalidDataException("Saved release pair does not identify this published release.");
        if (pair.Previous is not { } previous) return;
        ValidateRelease(previous);
        if (previous.Draft || previous.PublishedAt is null || previous.Repository != pair.Current.Repository
            || previous.PublishedAt > pair.Current.PublishedAt
            || previous.PublishedAt == pair.Current.PublishedAt && previous.Id >= pair.Current.Id)
            throw new InvalidDataException("Saved predecessor does not precede the current publication.");
    }

    public static bool IsCommit(string? value) => IsHash(value, 40) || IsHash(value, 64);

    public static bool IsHash(string? value, int length) => value?.Length == length && value.All(static c => char.IsAsciiHexDigit(c));

    public static void ValidateRelease(ReleaseRecord? release)
    {
        if (release is null || !IsRepository(release.Repository) || release.Id <= 0 || string.IsNullOrWhiteSpace(release.Tag)
            || release.Commit is not null && !IsCommit(release.Commit)
            || release.Commit is null && string.IsNullOrWhiteSpace(release.ResolutionFailure))
            throw new InvalidDataException("Release record identity or commit resolution evidence is invalid.");
    }

    public static bool IsRepository(string? repository) => repository is not null && repository.Split('/') is { Length: 2 } parts
        && parts.All(static part => part.Length > 0 && part is not "." and not ".."
            && part.All(static c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.'));

    internal static void CheckDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var properties = element.EnumerateObject().ToArray();
            if (properties.GroupBy(static p => p.Name, StringComparer.Ordinal).Any(static group => group.Count() != 1))
                throw new InvalidDataException("Evidence document has duplicate JSON properties.");
            foreach (var property in properties) CheckDuplicateProperties(property.Value);
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var child in element.EnumerateArray()) CheckDuplicateProperties(child);
    }

    private static T Read<T>(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            CheckDuplicateProperties(document.RootElement);
            return JsonSerializer.Deserialize<T>(json, Options) ?? throw new InvalidDataException("Evidence document is null.");
        }
        catch (JsonException)
        {
            throw new InvalidDataException("Evidence document is malformed or has an unsupported wire shape.");
        }
    }

    private static void WriteCanonical(JsonElement element, Utf8JsonWriter writer)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            writer.WriteStartObject();
            foreach (var property in element.EnumerateObject().OrderBy(static p => p.Name, StringComparer.Ordinal))
            {
                writer.WritePropertyName(property.Name);
                WriteCanonical(property.Value, writer);
            }
            writer.WriteEndObject();
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            writer.WriteStartArray();
            foreach (var child in element.EnumerateArray()) WriteCanonical(child, writer);
            writer.WriteEndArray();
        }
        else element.WriteTo(writer);
    }

    private static SessionSide CanonicalSide(SessionSide side) => side with
    {
        ExpectedCases = side.ExpectedCases.OrderBy(static workload => WorkloadIdentity.Key(workload), StringComparer.Ordinal)
            .Select(static workload => workload with { RequiredMetrics = workload.RequiredMetrics.Order().ToImmutableArray() }).ToImmutableArray(),
    };

    private static void ValidateSide(SessionSide? side)
    {
        if (side is null || !IsCommit(side.ProductCommit) || string.IsNullOrWhiteSpace(side.HarnessVersion)
            || string.IsNullOrWhiteSpace(side.ExporterVersion) || side.Invocation.IsDefault || side.ExpectedCases.IsDefault
            || side.Conditions is null)
            throw new InvalidDataException("Session side is missing native harness, inventory or environment evidence.");
        foreach (var workload in side.ExpectedCases) _ = WorkloadIdentity.Key(workload);
        if (side.ExpectedCases.GroupBy(static workload => WorkloadIdentity.Key(workload), StringComparer.Ordinal)
            .Any(static group => group.Count() > 1))
            throw new InvalidDataException("Session side has duplicated expected case identities.");
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter() },
    };
}
