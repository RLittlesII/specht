using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Specht.Comparison.Tests.Evidence;

namespace Specht.Comparison.Tests.Reporting;

[Trait("Tier", "Unit")]
public sealed class ReportBundlerUnitTests
{
    [Fact]
    public void IntactBundle_WhenValidated_ShouldReturnItsManifest()
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
        var bundle = new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), new ComparisonDocumentFixture(),
            [new IdentityMappingFixture()], [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");
        var zip = fileSystem.File.ReadAllBytes(Path.Combine("repo", bundle.Path));
        using var rewritten = new MemoryStream();
        rewritten.Write(zip);
        using (var archive = new ZipArchive(rewritten, ZipArchiveMode.Update, true))
        {
            archive.GetEntry("evidence-manifest.json")!.Delete();
            using var writer = archive.CreateEntry("evidence-manifest.json").Open();
            writer.Write(Encoding.UTF8.GetBytes(ComparisonJson.Write(bundle.Manifest)));
        }
        rewritten.Position = 0;

        // When
        var manifest = ReportBundler.Validate(new MemoryStream(zip));
        var rewrittenManifest = ReportBundler.Validate(rewritten);

        // Then
        manifest.Should().BeEquivalentTo(bundle.Manifest);
        rewrittenManifest.Should().BeEquivalentTo(bundle.Manifest);
    }

    [Theory]
    [ClassData(typeof(BrokenBundleRows))]
    public void BrokenBundle_WhenValidated_ShouldRejectIt(string defect, string value)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        ComparisonDocument document = new ComparisonDocumentFixture();
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
        var bundle = new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), document,
            [new IdentityMappingFixture()], [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");
        var manifest = bundle.Manifest;
        using var broken = new MemoryStream();
        broken.Write(fileSystem.File.ReadAllBytes(Path.Combine("repo", bundle.Path)));
        using (var archive = new ZipArchive(broken, ZipArchiveMode.Update, true))
        {
            void Write(string name, byte[] content, int attributes)
            {
                var entry = archive.CreateEntry(name);
                entry.ExternalAttributes = attributes;
                using var stream = entry.Open();
                stream.Write(content);
            }

            EvidenceFile Listed(string name, byte[] content) => new(name, "application/json",
                Convert.ToHexStringLower(SHA256.HashData(content)), content.Length, "report", null, null, null, "none", []);

            void Add(string name, int attributes = 0)
            {
                var content = "{}"u8.ToArray();
                Write(name, content, attributes);
                manifest = manifest with { Files = [.. manifest.Files.Add(Listed(name, content)).OrderBy(static f => f.Path, StringComparer.Ordinal)] };
            }

            byte[] Read(string name)
            {
                using var source = new MemoryStream();
                using (var stream = archive.GetEntry(name)!.Open()) stream.CopyTo(source);
                return source.ToArray();
            }

            void Rewrite(string name, string from, string to)
            {
                var text = Encoding.UTF8.GetString(Read(name));
                if (!text.Contains(from, StringComparison.Ordinal)) throw new InvalidOperationException($"{name} does not hold {from}.");
                var content = Encoding.UTF8.GetBytes(text.Replace(from, to, StringComparison.Ordinal));
                archive.GetEntry(name)!.Delete();
                Write(name, content, 0);
                manifest = manifest with
                {
                    Files = [.. manifest.Files.Select(f => f.Path == name
                        ? f with { Sha256 = Convert.ToHexStringLower(SHA256.HashData(content)), Length = content.Length } : f)],
                };
            }

            switch (defect)
            {
                case "mutated report":
                    using (var source = new MemoryStream())
                    {
                        using (var stream = archive.GetEntry("report.md")!.Open()) stream.CopyTo(source);
                        archive.GetEntry("report.md")!.Delete();
                        Write("report.md", [.. source.ToArray(), .. "edited"u8], 0);
                    }
                    break;
                case "deleted raw export":
                    archive.GetEntry("raw/previous/Synthetic-report-full.json")!.Delete();
                    break;
                case "unlisted extra entry":
                    Write("notes.txt", "{}"u8.ToArray(), 0);
                    break;
                case "duplicate entry name":
                    using (var source = new MemoryStream())
                    {
                        using (var stream = archive.GetEntry("session.json")!.Open()) stream.CopyTo(source);
                        Write("session.json", source.ToArray(), 0);
                    }
                    break;
                case "traversal entry":
                    Add("../escape.json");
                    break;
                case "rooted entry":
                    Add("/rooted.json");
                    break;
                case "backslash entry":
                    Add(@"raw\current\copy.json");
                    break;
                case "symbolic link entry":
                    var linked = Read("raw/previous/Synthetic-report-full.json");
                    archive.GetEntry("raw/previous/Synthetic-report-full.json")!.Delete();
                    Write("raw/previous/Synthetic-report-full.json", linked, unchecked((int)0xA1FF0000));
                    break;
                case "case-only duplicate entry":
                    var copied = manifest.Files.Single(static f => f.Path == "raw/previous/Synthetic-report-full.json");
                    Write("raw/previous/synthetic-report-full.json", Read(copied.Path), 0);
                    manifest = manifest with
                    {
                        Files = [.. manifest.Files.Add(copied with { Path = "raw/previous/synthetic-report-full.json" })
                            .OrderBy(static f => f.Path, StringComparer.Ordinal)],
                    };
                    break;
                case "rooted value in session":
                    Rewrite("session.json", "\"runnerIdentity\":\"runner-1\"", $"\"runnerIdentity\":\"{Rooted}\"");
                    Rewrite("report.json", "\"runnerIdentity\":\"runner-1\"", $"\"runnerIdentity\":\"{Rooted}\"");
                    break;
                case "rooted value in report":
                    Rewrite("report.json", "Mean time rose more than 10%", Rooted);
                    break;
                case "rooted value in pair":
                    Rewrite("pair.json", "\"selectingRunId\":\"run-1\"", $"\"selectingRunId\":\"{Rooted}\"");
                    Rewrite("report.json", "\"selectingRunId\":\"run-1\"", $"\"selectingRunId\":\"{Rooted}\"");
                    var digest = Convert.ToHexStringLower(SHA256.HashData(Read("pair.json")));
                    Rewrite("session.json", document.Session.PairDigest, digest);
                    Rewrite("report.json", document.Session.PairDigest, digest);
                    break;
                case "rooted value in identity mappings":
                    Rewrite("identity-mappings.json", "\"sourceFile\":\"Check.cs\"", $"\"sourceFile\":\"{Rooted}\"");
                    break;
                case "rooted value in unsanitised raw export":
                    Rewrite("raw/previous/Synthetic-report-full.json", "\"Title\": \"Synthetic workload\"", $"\"Title\": \"{Rooted}\"");
                    break;
                case "report session for another run":
                    Rewrite("report.json", "\"runId\":\"run-1\"", "\"runId\":\"run-2\"");
                    break;
                case "report session for another attempt":
                    Rewrite("report.json", "\"attempt\":1,", "\"attempt\":2,");
                    break;
                case "report pair for another selecting run":
                    Rewrite("report.json", "\"selectingRunId\":\"run-1\"", "\"selectingRunId\":\"run-2\"");
                    break;
                case "manifest length":
                    manifest = manifest with
                    {
                        Files = [.. manifest.Files.Select(static f => f.Path == "report.md" ? f with { Length = f.Length + 1 } : f)],
                    };
                    break;
                case "manifest version":
                    manifest = manifest with { FormatVersion = 2 };
                    break;
                case "session for another pair":
                    var session = Encoding.UTF8.GetBytes(ComparisonJson.Write(document.Session with { PairDigest = new string('9', 64) }));
                    archive.GetEntry("session.json")!.Delete();
                    Write("session.json", session, 0);
                    manifest = manifest with
                    {
                        Files = [.. manifest.Files.Select(f => f.Path == "session.json"
                            ? f with { Sha256 = Convert.ToHexStringLower(SHA256.HashData(session)), Length = session.Length } : f)],
                    };
                    break;
                case "report evidence not in manifest":
                    archive.GetEntry("raw/current/Synthetic-report-full.json")!.Delete();
                    manifest = manifest with { Files = [.. manifest.Files.Where(static f => f.Path != "raw/current/Synthetic-report-full.json")] };
                    break;
                case "embedded path in session invocation":
                    Rewrite("session.json", "\"*Check*\"", JsonSerializer.Serialize(value));
                    Rewrite("report.json", "\"*Check*\"", JsonSerializer.Serialize(value));
                    break;
                case "embedded path in workload parameter":
                    Rewrite("identity-mappings.json", "\"parameters\":{",
                        $"\"parameters\":{{\"Root\":{{\"kind\":\"string\",\"value\":{JsonSerializer.Serialize(value)}}},");
                    break;
                case "embedded path in reason":
                    Rewrite("report.json", "\"reason\":null,\"status\":", $"\"reason\":{JsonSerializer.Serialize(value)},\"status\":");
                    break;
                case "embedded path in raw export":
                    Rewrite("raw/previous/Synthetic-report-full.json", "\"Title\": \"Synthetic workload\"", $"\"Title\": {JsonSerializer.Serialize(value)}");
                    break;
                case "unresolved locator":
                    Rewrite("report.json", "\"locator\":\"/Benchmarks/1/Statistics\"", $"\"locator\":{JsonSerializer.Serialize(value)}");
                    break;
                case "locator without a source file":
                    Rewrite("report.json", "\"locator\":\"/Benchmarks/1/Statistics\",\"metric\":\"Time\",\"observer\":\"benchmark-process\","
                        + "\"sourceFile\":\"raw/current/Synthetic-report-full.json\",",
                        "\"locator\":\"/Benchmarks/1/Statistics\",\"metric\":\"Time\",\"observer\":\"benchmark-process\",");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(defect), defect, "Unbound bundle defect.");
            }
            archive.GetEntry("evidence-manifest.json")!.Delete();
            Write("evidence-manifest.json", Encoding.UTF8.GetBytes(ComparisonJson.Write(manifest)), 0);
        }
        broken.Position = 0;

        // When
        var validate = () => ReportBundler.Validate(broken);

        // Then
        validate.Should().Throw<InvalidDataException>();
    }

    [Theory]
    [ClassData(typeof(RootedGeneratedValueRows))]
    public void RootedGeneratedValue_WhenBundling_ShouldRejectItAndWriteNoArchive(string location)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        ComparisonDocument document = new ComparisonDocumentFixture();
        IdentityMapping mapping = new IdentityMappingFixture();
        var flagged = document.Cases[0];
        var workload = flagged.Evidence.Current!.Case;
        if (location == "session invocation")
            document = document with { Session = document.Session with { Previous = document.Session.Previous with { Invocation = ["--artifacts", Rooted] } } };
        else if (location == "workload parameter")
            workload = workload with { Parameters = workload.Parameters.SetItem("Output", new ParameterValue("string", Rooted)) };
        else if (location == "workload boundary") workload = workload with { Boundary = Rooted };
        else if (location == "case reason") flagged = flagged with { Reason = Rooted };
        else if (location == "gap reason") document = document with { Gaps = [document.Gaps[0] with { Reason = Rooted }] };
        else mapping = mapping with { SourceFile = Rooted };
        flagged = flagged with { Evidence = flagged.Evidence with { Current = flagged.Evidence.Current with { Case = workload } } };
        document = document with { Cases = [flagged, document.Cases[1]] };
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);

        // When
        var create = () => new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), document, [mapping],
            [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");

        // Then
        create.Should().Throw<InvalidDataException>();
        var workspace = Path.Combine("repo", ".artifacts", "benchmarks", "comparison");
        (fileSystem.Directory.Exists(workspace) ? fileSystem.Directory.GetFiles(workspace, "*", SearchOption.AllDirectories) : Array.Empty<string>())
            .Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(EmbeddedPathRows))]
    public void EmbeddedAbsolutePath_WhenBundling_ShouldRejectItAndWriteNoArchive(string placement, string value)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        var document = Place(new ComparisonDocumentFixture(), fileSystem, placement, value);
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);

        // When
        var create = () => new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), document, [new IdentityMappingFixture()],
            [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");

        // Then
        create.Should().Throw<InvalidDataException>();
        var workspace = Path.Combine("repo", ".artifacts", "benchmarks", "comparison");
        (fileSystem.Directory.Exists(workspace) ? fileSystem.Directory.GetFiles(workspace, "*", SearchOption.AllDirectories) : Array.Empty<string>())
            .Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(KeptValueRows))]
    public void RecordedValueWithSlashes_WhenBundledAndValidated_ShouldKeepItUnchanged(string placement, string value)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        var document = Place(new ComparisonDocumentFixture(), fileSystem, placement, value);
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);

        // When
        var bundle = new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), document, [new IdentityMappingFixture()],
            [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");
        var zip = fileSystem.File.ReadAllBytes(Path.Combine("repo", bundle.Path));
        var manifest = ReportBundler.Validate(new MemoryStream(zip));

        // Then
        manifest.Should().BeEquivalentTo(bundle.Manifest);
        var name = placement switch
        {
            "session invocation" => "session.json",
            "raw export" => "raw/current/Synthetic-report-full.json",
            _ => "report.json",
        };
        using var archive = new ZipArchive(new MemoryStream(zip));
        using var reader = new StreamReader(archive.GetEntry(name)!.Open(), Encoding.UTF8);
        reader.ReadToEnd().Should().Contain(JsonSerializer.Serialize(value));
        manifest.Files.Should().OnlyContain(static file => file.Transformation == "none");
    }

    [Theory]
    [ClassData(typeof(UnresolvedLocatorRows))]
    public void UnresolvedLocator_WhenBundling_ShouldRejectItAndWriteNoArchive(string locator)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        ComparisonDocument document = new ComparisonDocumentFixture();
        var large = document.Cases[1];
        var evidence = large.Evidence.Current!;
        document = document with
        {
            Cases =
            [
                document.Cases[0],
                large with
                {
                    Evidence = large.Evidence with
                    {
                        Current = evidence with { Observations = [evidence.Observations[0] with { Locator = locator }, evidence.Observations[1]] },
                    },
                },
            ],
        };
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);

        // When
        var create = () => new ReportBundler(fileSystem, "repo").Create(new ReleasePairFixture(), document, [new IdentityMappingFixture()],
            [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");

        // Then
        create.Should().Throw<InvalidDataException>();
        var workspace = Path.Combine("repo", ".artifacts", "benchmarks", "comparison");
        (fileSystem.Directory.Exists(workspace) ? fileSystem.Directory.GetFiles(workspace, "*", SearchOption.AllDirectories) : Array.Empty<string>())
            .Should().BeEmpty();
    }

    public sealed class BrokenBundleRows : TheoryData<string, string>
    {
        public BrokenBundleRows()
        {
            Add("mutated report", "");
            Add("deleted raw export", "");
            Add("unlisted extra entry", "");
            Add("duplicate entry name", "");
            Add("traversal entry", "");
            Add("rooted entry", "");
            Add("backslash entry", "");
            Add("symbolic link entry", "");
            Add("manifest length", "");
            Add("manifest version", "");
            Add("session for another pair", "");
            Add("report evidence not in manifest", "");
            Add("case-only duplicate entry", "");
            Add("rooted value in session", "");
            Add("rooted value in report", "");
            Add("rooted value in pair", "");
            Add("rooted value in identity mappings", "");
            Add("rooted value in unsanitised raw export", "");
            Add("report session for another run", "");
            Add("report session for another attempt", "");
            Add("report pair for another selecting run", "");
            foreach (var placement in new[] { "session invocation", "workload parameter", "reason", "raw export" })
                foreach (var embedded in EmbeddedPaths) Add($"embedded path in {placement}", embedded);
            foreach (var locator in UnresolvedLocators) Add("unresolved locator", locator);
            Add("locator without a source file", "");
        }
    }

    public sealed class RootedGeneratedValueRows : TheoryData<string>
    {
        public RootedGeneratedValueRows()
        {
            Add("session invocation");
            Add("workload parameter");
            Add("workload boundary");
            Add("case reason");
            Add("gap reason");
            Add("identity mapping source");
        }
    }

    public sealed class EmbeddedPathRows : TheoryData<string, string>
    {
        public EmbeddedPathRows()
        {
            foreach (var placement in new[] { "session invocation", "workload parameter", "case reason", "raw export" })
                foreach (var embedded in EmbeddedPaths) Add(placement, embedded);
        }
    }

    public sealed class KeptValueRows : TheoryData<string, string>
    {
        public KeptValueRows()
        {
            foreach (var placement in new[] { "session invocation", "workload parameter", "case reason", "metric unit", "raw export" })
            {
                foreach (var kept in new[]
                    {
                        "ns/op", "1/2 of the samples failed", "raw/current/x.json", "--artifacts raw/current", "dotnet build /p:Configuration=Release",
                        "https://example.com/a",
                    })
                    Add(placement, kept);
            }
        }
    }

    public sealed class UnresolvedLocatorRows : TheoryData<string>
    {
        public UnresolvedLocatorRows()
        {
            foreach (var locator in UnresolvedLocators) Add(locator);
        }
    }

    private static ComparisonDocument Place(ComparisonDocument document, MockFileSystem fileSystem, string placement, string value)
    {
        var flagged = document.Cases[0];
        var evidence = flagged.Evidence.Current!;
        if (placement == "session invocation")
            return document with { Session = document.Session with { Previous = document.Session.Previous with { Invocation = ["--filter", value] } } };
        if (placement == "workload parameter")
            flagged = flagged with
            {
                Evidence = flagged.Evidence with
                {
                    Current = evidence with
                    {
                        Case = evidence.Case with { Parameters = evidence.Case.Parameters.SetItem("Root", new ParameterValue("string", value)) },
                    },
                },
            };
        else if (placement == "case reason") flagged = flagged with { Reason = value };
        else if (placement == "metric unit") flagged = flagged with { Metrics = [flagged.Metrics[0] with { Unit = value }, flagged.Metrics[1]] };
        else
            fileSystem.File.WriteAllText(Path.Combine("repo", ComparisonDocumentFixture.CurrentExport),
                ExportData.CurrentJson.Replace("\"Title\": \"Synthetic workload\"", $"\"Title\": {JsonSerializer.Serialize(value)}", StringComparison.Ordinal));
        return document with { Cases = [flagged, document.Cases[1]] };
    }

    private const string Rooted = "/home/runner/work/x";

    private static readonly string[] EmbeddedPaths =
        ["Root=/home/runner/x", "--artifacts=/tmp/a/b", "cache at ~/x", @"Path=C:\x", "dirs;D:/x", @"share (\\server\share)", "read FILE:///tmp"];

    private static readonly string[] UnresolvedLocators = ["/home/runner/x", "/Benchmarks/9/Statistics", "/home"];
}
