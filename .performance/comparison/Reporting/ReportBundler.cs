using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Specht.Comparison;

/// <summary>Writes and validates the self-contained per-attempt report bundle.</summary>
public sealed class ReportBundler(IFileSystem fileSystem, string repositoryRoot)
{
    public static EvidenceManifest Validate(Stream zip)
    {
        var entries = Entries(zip);
        if (!entries.TryGetValue(ManifestPath, out var manifestBytes))
            throw new InvalidDataException("Report bundle has no evidence manifest.");
        var manifest = ComparisonJson.ReadManifest(Encoding.UTF8.GetString(manifestBytes));
        var listed = manifest.Files.ToDictionary(static file => file.Path, StringComparer.Ordinal);
        if (entries.Keys.Any(name => name != ManifestPath && !listed.ContainsKey(name))
            || manifest.Files.Any(file => !entries.TryGetValue(file.Path, out var content) || content.LongLength != file.Length
                || Sha256(content) != file.Sha256)
            || Roles.Keys.Any(path => !listed.ContainsKey(path))
            || manifest.Files.Any(static file => file.Role != Roles.GetValueOrDefault(file.Path, RawExportRole)
                || file.Role == RawExportRole && (file.Side is not ("previous" or "current")
                    || !file.Path.StartsWith($"raw/{file.Side}/", StringComparison.Ordinal) || file.Path.Count(static c => c == '/') != 2)))
        {
            throw new InvalidDataException("Report bundle entries do not match its evidence manifest.");
        }

        var session = ComparisonJson.ReadSession(Encoding.UTF8.GetString(entries["session.json"]));
        if (Sha256(entries["pair.json"]) != session.PairDigest)
            throw new InvalidDataException("Report bundle session does not identify its release pair.");
        var documents = new Dictionary<string, JsonDocument>(StringComparer.Ordinal);
        try
        {
            foreach (var (path, content) in entries.Where(static entry => entry.Key != "report.md"))
                documents.Add(path, JsonDocument.Parse(content));
            var root = documents["report.json"].RootElement;
            ComparisonJson.CheckDuplicateProperties(root);
            var comparison = root.GetProperty("comparison");
            if (root.GetProperty("formatVersion").GetInt32() != 1
                || root.GetProperty("pair").GetRawText() != Encoding.UTF8.GetString(entries["pair.json"])
                || ComparisonJson.Write(ComparisonJson.ReadSession(comparison.GetProperty("session").GetRawText()))
                    != Encoding.UTF8.GetString(entries["session.json"])
                || SourceFiles(comparison).Any(path => !listed.TryGetValue(path, out var file) || file.Role != RawExportRole)
                || Locators(root).Any(cited => cited.SourceFile is null || !listed.TryGetValue(cited.SourceFile, out var file)
                    || file.Role != RawExportRole || !ResolvesParent(documents[cited.SourceFile].RootElement, cited.Locator)))
            {
                throw new InvalidDataException("Report evidence references are not files and records of this bundle.");
            }

            if (documents.Any(static document => HoldsAbsolutePath(document.Value.RootElement, document.Key == "report.json")))
                throw new InvalidDataException("Report bundle holds an absolute path.");
            if (Encoding.UTF8.GetString(entries["report.md"]) != ComparisonReport.Markdown(ComparisonJson.ReadPair(root.GetProperty("pair").GetRawText()),
                    ComparisonJson.ReadDocument(comparison.GetRawText())))
            {
                throw new InvalidDataException("Report bundle Markdown is not the rendering of its JSON report.");
            }
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or KeyNotFoundException or FormatException
            or NullReferenceException)
        {
            throw new InvalidDataException("Report bundle document is malformed.");
        }
        finally
        {
            foreach (var document in documents.Values) document.Dispose();
        }
        return manifest;
    }

    public ReportBundle Create(ReleasePair pair, ComparisonDocument document, ImmutableArray<IdentityMapping> mappings,
        ImmutableArray<RawExport> exports, string workspace)
    {
        if (!EvidencePaths.IsRelative(workspace) || mappings.IsDefault || exports.IsDefault)
            throw new InvalidDataException("Report bundle inputs are missing or the workspace is not repository-relative.");
        ComparisonJson.ValidatePair(pair, pair.Current);
        var session = ComparisonJson.ReadSession(ComparisonJson.Write(document.Session));
        if (session.PairDigest != ComparisonJson.Digest(pair))
            throw new InvalidDataException("Session manifest does not identify this release pair.");
        var name = Name(pair.Current.Id, session);
        var citations = ComparisonReport.Citations(document).ToArray();
        if (citations.Any(cited => !exports.Any(export => export.Side == cited.Side && export.SourcePath == cited.Observation.SourceFile)))
            throw new InvalidDataException("An observation cites a raw export that was not supplied.");
        List<(EvidenceFile File, byte[] Content)> files =
        [
            .. exports.Select(export => Raw(export, citations)),
            Generated("report.md", "text/markdown", ComparisonReport.Markdown(pair, document)),
            Generated("report.json", "application/json", ComparisonReport.Json(pair, document)),
            Generated("pair.json", "application/json", ComparisonJson.Write(pair)),
            Generated("session.json", "application/json", ComparisonJson.Write(document.Session)),
            Generated("identity-mappings.json", "application/json", ComparisonJson.Write(mappings
                .OrderBy(static mapping => mapping.FullName, StringComparer.Ordinal)
                .ThenBy(static mapping => mapping.ProductCommit, StringComparer.Ordinal).ToImmutableArray())),
        ];
        if (files.GroupBy(static file => file.File.Path, StringComparer.OrdinalIgnoreCase).Any(static group => group.Count() > 1))
            throw new InvalidDataException("Two raw exports would share one bundle path.");
        var manifest = new EvidenceManifest(1, [.. files.Select(static file => file.File).OrderBy(static file => file.Path, StringComparer.Ordinal)]);
        var zip = Archive(files.Select(static file => (file.File.Path, file.Content))
            .Append((ManifestPath, Encoding.UTF8.GetBytes(ComparisonJson.Write(manifest)))));
        _ = Validate(new MemoryStream(zip));
        Write(workspace, name, zip);
        return new ReportBundle(name, $"{workspace}/{name}", Sha256(zip), manifest);
    }

    internal static string Name(long releaseId, SessionManifest session)
    {
        var name = string.Create(CultureInfo.InvariantCulture, $"comparison-{releaseId}-{session.RunId}-{session.Attempt}.zip");
        if (!EvidencePaths.IsRelative(name) || name.Contains('/'))
            throw new InvalidDataException("Run identity cannot name a report bundle.");
        return name;
    }

    internal static bool IsRooted(string value) => value.StartsWith('/') || value.StartsWith('~') || value.StartsWith('\\')
        || value.Length >= 3 && char.IsAsciiLetter(value[0]) && value[1] == ':' && value[2] is '\\' or '/';

    internal static bool EmbedsPath(string value)
    {
        for (var start = 0; start < value.Length; start++)
        {
            if (start > 0 && !IsBoundary(value[start - 1])) continue;
            var rest = value.AsSpan(start);
            if (rest.StartsWith(@"\\") || rest.StartsWith("~/") || rest.StartsWith(@"~\")
                || rest.Length >= 3 && char.IsAsciiLetter(rest[0]) && rest[1] == ':' && rest[2] is '\\' or '/'
                || rest.Length >= 6 && rest[..5].Equals("file:", StringComparison.OrdinalIgnoreCase) && rest[5] == '/')
            {
                return true;
            }

            if (rest.Length < 3 || rest[0] != '/') continue;
            var end = 1;
            while (end < rest.Length && rest[end] != '/' && rest[end] != '\\' && !IsBoundary(rest[end])) end++;
            if (end > 1 && end < rest.Length && rest[end] == '/') return true;
        }
        return false;
    }

    private static Dictionary<string, byte[]> Entries(Stream zip)
    {
        var entries = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var archive = new ZipArchive(zip, ZipArchiveMode.Read, true);
            foreach (var entry in archive.Entries)
            {
                if (!EvidencePaths.IsRelative(entry.FullName) || ((entry.ExternalAttributes >> 16) & 0xF000) is not (0 or 0x8000)
                    || !names.Add(entry.FullName))
                {
                    throw new InvalidDataException("Report bundle has an unsafe or duplicated entry.");
                }

                using var content = new MemoryStream();
                using (var stream = entry.Open()) stream.CopyTo(content);
                entries.Add(entry.FullName, content.ToArray());
            }
        }
        catch (Exception exception) when (exception is IOException or ArgumentException or NotSupportedException)
        {
            throw new InvalidDataException("Report bundle is not a readable archive.");
        }
        return entries;
    }

    private static IEnumerable<string> SourceFiles(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => element.EnumerateObject().SelectMany(static property =>
            property.Name == "sourceFile" && property.Value.ValueKind == JsonValueKind.String
                ? [property.Value.GetString()!]
                : SourceFiles(property.Value)),
        JsonValueKind.Array => element.EnumerateArray().SelectMany(SourceFiles),
        _ => [],
    };

    private static IEnumerable<(string? SourceFile, JsonElement Locator)> Locators(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("locator", out var locator))
            {
                yield return (element.TryGetProperty("sourceFile", out var source) && source.ValueKind == JsonValueKind.String
                    ? source.GetString() : null, locator);
            }

            foreach (var property in element.EnumerateObject())
                foreach (var cited in Locators(property.Value)) yield return cited;
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var item in element.EnumerateArray())
                foreach (var cited in Locators(item)) yield return cited;
    }

    private static bool ResolvesParent(JsonElement export, JsonElement locator)
    {
        if (locator.ValueKind != JsonValueKind.String || locator.GetString() is not { } pointer || !pointer.StartsWith('/')) return false;
        var tokens = pointer[1..].Split('/');
        if (tokens.Length < 2) return false;
        var element = export;
        foreach (var token in tokens[..^1]
            .Select(static token => token.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal)))
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(token, out var child)) element = child;
            else if (element.ValueKind == JsonValueKind.Array && (token == "0" || token.Length > 0 && token[0] != '0')
                && int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out var index) && index < element.GetArrayLength())
                element = element[index];
            else return false;
        }
        return true;
    }

    private static bool HoldsAbsolutePath(JsonElement element, bool locators) => element.ValueKind switch
    {
        JsonValueKind.Object => element.EnumerateObject()
            .Any(property => !(locators && property.Name == "locator") && HoldsAbsolutePath(property.Value, locators)),
        JsonValueKind.Array => element.EnumerateArray().Any(item => HoldsAbsolutePath(item, locators)),
        JsonValueKind.String => IsRooted(element.GetString()!) || EmbedsPath(element.GetString()!),
        _ => false,
    };

    private static (EvidenceFile File, byte[] Content) Generated(string path, string mediaType, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return (new EvidenceFile(path, mediaType, Sha256(bytes), bytes.LongLength, Roles[path], null, null, null, "none", []), bytes);
    }

    private static byte[] Sanitised(byte[] original)
    {
        try
        {
            using var document = JsonDocument.Parse(original);
            var root = document.RootElement;
            ComparisonJson.CheckDuplicateProperties(root);
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("HostEnvironmentInfo", out var host)
                || host.ValueKind != JsonValueKind.Object || !host.TryGetProperty("BenchmarkDotNetVersion", out var version)
                || version.ValueKind != JsonValueKind.String || !BenchmarkEvidenceReader.IsSupportedProducer(version.GetString()))
            {
                throw new InvalidDataException("Raw export is not a BenchmarkDotNet 0.15.8 full JSON export.");
            }

            var removed = false;
            using var buffer = new MemoryStream();
            using (var writer = new Utf8JsonWriter(buffer)) Sanitise(root, writer, ref removed);
            return removed ? buffer.ToArray() : original;
        }
        catch (JsonException)
        {
            throw new InvalidDataException("Raw export is malformed JSON.");
        }
    }

    private static void Sanitise(JsonElement element, Utf8JsonWriter writer, ref bool removed)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            writer.WriteStartObject();
            foreach (var property in element.EnumerateObject())
            {
                writer.WritePropertyName(property.Name);
                Sanitise(property.Value, writer, ref removed);
            }
            writer.WriteEndObject();
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            writer.WriteStartArray();
            foreach (var item in element.EnumerateArray()) Sanitise(item, writer, ref removed);
            writer.WriteEndArray();
        }
        else if (element.ValueKind == JsonValueKind.String && IsRooted(element.GetString()!))
        {
            writer.WriteStringValue("[path removed]");
            removed = true;
        }
        else writer.WriteRawValue(element.GetRawText(), true);
    }

    private static bool IsBoundary(char character) =>
        char.IsWhiteSpace(character) || character is '=' or ',' or ';' or ':' or '(' or ')' or '[' or ']' or '"' or '\'';

    private static byte[] Archive(IEnumerable<(string Path, byte[] Content)> entries)
    {
        using var buffer = new MemoryStream();
        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, true))
        {
            foreach (var (path, content) in entries.OrderBy(static entry => entry.Path, StringComparer.Ordinal))
            {
                var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
                entry.LastWriteTime = EntryTime;
                entry.ExternalAttributes = RegularFile;
                using var stream = entry.Open();
                stream.Write(content);
            }
        }
        return buffer.ToArray();
    }

    private static string Sha256(byte[] content) => Convert.ToHexStringLower(SHA256.HashData(content));

    private (EvidenceFile File, byte[] Content) Raw(RawExport export, (string Key, string Side, Observation Observation)[] citations)
    {
        if (export is null || export.Side is not ("previous" or "current") || !EvidencePaths.IsRelative(export.SourcePath)
            || !export.SourcePath.EndsWith("-report-full.json", StringComparison.Ordinal))
        {
            throw new InvalidDataException("Raw export is not a repository-relative BenchmarkDotNet full JSON export.");
        }

        byte[] original;
        try
        {
            var path = repositoryRoot;
            foreach (var segment in export.SourcePath.Split('/'))
            {
                path = fileSystem.Path.Combine(path, segment);
                if ((fileSystem.File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Raw export or a directory above it is a symbolic link.");
            }
            original = fileSystem.File.ReadAllBytes(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new InvalidDataException("Raw export is unreadable.");
        }
        var retained = Sanitised(original);
        var cases = citations.Where(cited => cited.Side == export.Side && cited.Observation.SourceFile == export.SourcePath)
            .Select(static cited => cited.Key).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
        return (new EvidenceFile(ComparisonReport.BundlePath(export.Side, export.SourcePath), "application/json", Sha256(retained),
            retained.LongLength, RawExportRole, export.Side, export.SourcePath, Sha256(original),
            ReferenceEquals(retained, original) ? "none" : "absolute-paths-removed", [.. cases]), retained);
    }

    private void Write(string workspace, string name, byte[] zip)
    {
        var directory = fileSystem.Path.Combine(repositoryRoot, workspace);
        var target = fileSystem.Path.Combine(directory, name);
        if (fileSystem.File.Exists(target)) throw new IOException("Report bundle already exists and is never overwritten.");
        fileSystem.Directory.CreateDirectory(directory);
        var temporary = fileSystem.Path.Combine(directory, fileSystem.Path.GetRandomFileName());
        try
        {
            using (var stream = fileSystem.FileStream.New(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)) stream.Write(zip);
            fileSystem.File.Move(temporary, target, false);
        }
        catch (IOException)
        {
            throw new IOException("Report bundle could not be written without overwriting an existing file.");
        }
        finally
        {
            if (fileSystem.File.Exists(temporary)) fileSystem.File.Delete(temporary);
        }
    }

    private const string ManifestPath = "evidence-manifest.json";
    private const string RawExportRole = "raw-export";
    private const int RegularFile = unchecked((int)0x81A40000);

    private static readonly DateTimeOffset EntryTime = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly ImmutableDictionary<string, string> Roles = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["report.md"] = "report",
        ["report.json"] = "report",
        ["pair.json"] = "pair",
        ["session.json"] = "session",
        ["identity-mappings.json"] = "identity-mappings",
    }.ToImmutableDictionary(StringComparer.Ordinal);
}
