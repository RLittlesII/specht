using System.IO.Abstractions;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Specht.Comparison.Tests.Evidence;

namespace Specht.Comparison.Tests.Reporting;

[Trait("Tier", "Integration")]
public sealed class ReportBundlerIntegrationTests
{
    [Fact]
    public void SuppliedEvidence_WhenBundled_ShouldRetainEveryEntryWithItsDigestAndLinkage()
    {
        // Given
        var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "previous"));
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "current"));
            File.WriteAllText(Path.Combine(root, ComparisonDocumentFixture.PreviousExport), ExportData.FullJson);
            File.WriteAllText(Path.Combine(root, ComparisonDocumentFixture.CurrentExport), ExportData.CurrentJson);
            ReleasePair pair = ReportFixtures.Pair();
            ComparisonDocument document = new ComparisonDocumentFixture();
            IdentityMapping later = ReportFixtures.Mapping().WithFullName("Synthetic.Check(2)");
            IdentityMapping earlier = ReportFixtures.Mapping();
            RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
            RawExport current = new RawExportFixture();
            var previousDigest = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(ExportData.FullJson)));
            var currentDigest = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(ExportData.CurrentJson)));

            // When
            var bundle = new ReportBundler(new FileSystem(), root).Create(pair, document, [later, earlier], [current, previous],
                ".artifacts/benchmarks/comparison");

            // Then
            bundle.Name.Should().Be("comparison-20-run-1-1.zip");
            bundle.Path.Should().Be(".artifacts/benchmarks/comparison/comparison-20-run-1-1.zip");
            var zip = File.ReadAllBytes(Path.Combine(root, bundle.Path));
            bundle.Sha256.Should().Be(Convert.ToHexStringLower(SHA256.HashData(zip)));
            var entries = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            using (var archive = new ZipArchive(new MemoryStream(zip)))
            {
                foreach (var entry in archive.Entries)
                {
                    using var content = new MemoryStream();
                    using (var stream = entry.Open())
                    {
                        stream.CopyTo(content);
                    }

                    entries.Add(entry.FullName, content.ToArray());
                }
            }

            entries.Keys.Should().Equal("evidence-manifest.json", "identity-mappings.json", "pair.json", "raw/current/Synthetic-report-full.json",
                "raw/previous/Synthetic-report-full.json", "report.json", "report.md", "session.json");
            bundle.Manifest.FormatVersion.Should().Be(1);
            bundle.Manifest.Files.Select(static file => (file.Path, file.Role, file.MediaType)).Should().Equal(
                ("identity-mappings.json", "identity-mappings", "application/json"),
                ("pair.json", "pair", "application/json"),
                ("raw/current/Synthetic-report-full.json", "raw-export", "application/json"),
                ("raw/previous/Synthetic-report-full.json", "raw-export", "application/json"),
                ("report.json", "report", "application/json"),
                ("report.md", "report", "text/markdown"),
                ("session.json", "session", "application/json"));
            bundle.Manifest.Files.Should().OnlyContain(file => file.Length == entries[file.Path].Length
                && file.Sha256 == Convert.ToHexStringLower(SHA256.HashData(entries[file.Path])));
            bundle.Manifest.Files.Where(static file => file.Role != "raw-export").Should().OnlyContain(static file =>
                file.Side == null && file.Cases.IsEmpty && file.Transformation == "none");
            bundle.Manifest.Files.Where(static file => file.Role == "raw-export")
                .Select(static file => (file.Side, file.Source, file.SourceSha256, file.Transformation, string.Join(" ", file.Cases)))
                .Should().Equal(
                    ("current", ComparisonDocumentFixture.CurrentExport, currentDigest, "none", $"{SmallKey} {LargeKey}"),
                    ("previous", ComparisonDocumentFixture.PreviousExport, previousDigest, "none", SmallKey));
            entries["raw/current/Synthetic-report-full.json"].Should().Equal(Encoding.UTF8.GetBytes(ExportData.CurrentJson));
            entries["raw/previous/Synthetic-report-full.json"].Should().Equal(Encoding.UTF8.GetBytes(ExportData.FullJson));
            ComparisonJson.ReadPair(Encoding.UTF8.GetString(entries["pair.json"]), pair.Current).Should().BeEquivalentTo(pair);
            ComparisonJson.ReadSession(Encoding.UTF8.GetString(entries["session.json"])).Should().BeEquivalentTo(document.Session);
            using var mappings = JsonDocument.Parse(entries["identity-mappings.json"]);
            mappings.RootElement.EnumerateArray().Select(static mapping => mapping.GetProperty("fullName").GetString())
                .Should().Equal("Synthetic.Check(1)", "Synthetic.Check(2)");
            Encoding.UTF8.GetString(entries["report.json"]).Should().Be(ComparisonReport.Json(pair, document));
            Encoding.UTF8.GetString(entries["report.md"]).Should().Be(ComparisonReport.Markdown(pair, document));
            entries.Values.Should().OnlyContain(content => !Encoding.UTF8.GetString(content).Contains(root, StringComparison.Ordinal));
            ReportBundler.Validate(new MemoryStream(zip)).Should().BeEquivalentTo(bundle.Manifest);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void SameInputs_WhenBundledTwice_ShouldWriteByteIdenticalArchivesOfRegularFiles()
    {
        // Given
        var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "previous"));
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "current"));
            File.WriteAllText(Path.Combine(root, ComparisonDocumentFixture.PreviousExport), ExportData.FullJson);
            File.WriteAllText(Path.Combine(root, ComparisonDocumentFixture.CurrentExport), ExportData.CurrentJson);
            ReleasePair pair = ReportFixtures.Pair();
            ComparisonDocument document = new ComparisonDocumentFixture();
            RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
            var bundler = new ReportBundler(new FileSystem(), root);

            // When
            var first = bundler.Create(pair, document, [ReportFixtures.Mapping()], [previous, new RawExportFixture()], ".artifacts/benchmarks/first");
            var second = bundler.Create(pair, document, [ReportFixtures.Mapping()], [previous, new RawExportFixture()], ".artifacts/benchmarks/second");

            // Then
            var zip = File.ReadAllBytes(Path.Combine(root, first.Path));
            File.ReadAllBytes(Path.Combine(root, second.Path)).Should().Equal(zip);
            second.Sha256.Should().Be(first.Sha256);
            using var archive = new ZipArchive(new MemoryStream(zip));
            var names = archive.Entries.Select(static entry => entry.FullName).ToArray();
            names.Should().Equal(names.Order(StringComparer.Ordinal));
            names.Should().NotContain(static name => name.EndsWith('/'));
            archive.Entries.Select(static entry => entry.LastWriteTime).Distinct().Should().ContainSingle();
            archive.Entries.Should().OnlyContain(static entry => ((entry.ExternalAttributes >> 16) & 0xF000) == 0
                || ((entry.ExternalAttributes >> 16) & 0xF000) == 0x8000);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void ExistingArchive_WhenBundling_ShouldNotOverwriteIt()
    {
        // Given
        var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "previous"));
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "current"));
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "comparison"));
            File.WriteAllText(Path.Combine(root, ComparisonDocumentFixture.PreviousExport), ExportData.FullJson);
            File.WriteAllText(Path.Combine(root, ComparisonDocumentFixture.CurrentExport), ExportData.CurrentJson);
            var existing = Path.Combine(root, ".artifacts", "benchmarks", "comparison", "comparison-20-run-1-1.zip");
            File.WriteAllBytes(existing, [1, 2, 3]);
            RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);

            // When
            var create = () => new ReportBundler(new FileSystem(), root).Create(ReportFixtures.Pair(), new ComparisonDocumentFixture(),
                [ReportFixtures.Mapping()], [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");

            // Then
            create.Should().ThrowExactly<IOException>();
            File.ReadAllBytes(existing).Should().Equal(1, 2, 3);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Theory]
    [ClassData(typeof(RootedPathRows))]
    public void PathBearingMetadata_WhenBundled_ShouldRemoveRootedPathsAndKeepRawNumbers(string rooted)
    {
        // Given
        var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "previous"));
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "current"));
            var export = $$$"""
                {"Title":"Synthetic workload","HostEnvironmentInfo":{"BenchmarkDotNetVersion":"0.15.8","Configuration":"Release",
                 "HasAttachedDebugger":false,"RuntimeVersion":".NET 10.0.0","WorkingDirectory":{{{JsonSerializer.Serialize(rooted)}}}},
                 "Benchmarks":[{"FullName":"Synthetic.Check(1)","Source":"src/Check.cs","Executable":{{{JsonSerializer.Serialize(rooted)}}},
                 "Statistics":{"Mean":100.250,"ConfidenceInterval":{"Mean":100.250,"Level":12,"Lower":98.5000,"Upper":1.0205E2}},
                 "Memory":{"BytesAllocatedPerOperation":32},"Measurements":[]},
                 {"FullName":"Synthetic.Check(10)","Statistics":{"Mean":300,"ConfidenceInterval":{"Mean":300,"Level":12,"Lower":290,"Upper":310}},
                 "Memory":{"BytesAllocatedPerOperation":64},"Measurements":[]}]}
                """;
            var original = Encoding.UTF8.GetBytes(export);
            File.WriteAllText(Path.Combine(root, ComparisonDocumentFixture.PreviousExport), ExportData.FullJson);
            File.WriteAllBytes(Path.Combine(root, ComparisonDocumentFixture.CurrentExport), original);
            RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);

            // When
            var bundle = new ReportBundler(new FileSystem(), root).Create(ReportFixtures.Pair(), new ComparisonDocumentFixture(),
                [ReportFixtures.Mapping()], [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");

            // Then
            using var archive = new ZipArchive(new MemoryStream(File.ReadAllBytes(Path.Combine(root, bundle.Path))));
            using var content = new MemoryStream();
            using (var stream = archive.GetEntry("raw/current/Synthetic-report-full.json")!.Open())
            {
                stream.CopyTo(content);
            }

            var retained = content.ToArray();
            var scalars = new List<(string Pointer, JsonValueKind Kind, string Text)>[2];
            foreach (var (index, bytes) in new[] { (0, original), (1, retained) })
            {
                scalars[index] = [];
                using var json = JsonDocument.Parse(bytes);
                var pending = new Stack<(string, JsonElement)>([("", json.RootElement)]);
                while (pending.TryPop(out var next))
                {
                    var (pointer, element) = next;
                    if (element.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var property in element.EnumerateObject())
                        {
                            pending.Push(($"{pointer}/{property.Name}", property.Value));
                        }
                    }
                    else if (element.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var (item, position) in element.EnumerateArray().Select(static (item, position) => (item, position)))
                        {
                            pending.Push(($"{pointer}/{position}", item));
                        }
                    }
                    else
                    {
                        scalars[index].Add((pointer, element.ValueKind,
                            element.ValueKind == JsonValueKind.String ? element.GetString()! : element.GetRawText()));
                    }
                }
            }

            scalars[1].Should().BeEquivalentTo(scalars[0].Select(static scalar => scalar.Pointer is "/HostEnvironmentInfo/WorkingDirectory"
                or "/Benchmarks/0/Executable" ? scalar with { Text = "[path removed]" } : scalar));
            scalars[1].Where(static scalar => scalar.Kind == JsonValueKind.Number).Select(static scalar => scalar.Text)
                .Should().BeEquivalentTo("100.250", "100.250", "12", "98.5000", "1.0205E2", "32", "300", "300", "12", "290", "310", "64");
            Encoding.UTF8.GetString(retained).Should().NotContain(rooted).And.NotContain(JsonSerializer.Serialize(rooted).Trim('"'));
            var file = bundle.Manifest.Files.Single(static file => file.Path == "raw/current/Synthetic-report-full.json");
            file.Transformation.Should().Be("absolute-paths-removed");
            file.SourceSha256.Should().Be(Convert.ToHexStringLower(SHA256.HashData(original)));
            file.Sha256.Should().Be(Convert.ToHexStringLower(SHA256.HashData(retained)));
            bundle.Manifest.Files.Single(static file => file.Path == "raw/previous/Synthetic-report-full.json").Transformation.Should().Be("none");
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void FourPartProducerVersion_WhenBundled_ShouldRetainTheExportUnchanged()
    {
        // Given
        var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "previous"));
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "current"));
            var original = Encoding.UTF8.GetBytes(ExportData.CurrentJson.Replace("\"0.15.8\"", "\"0.15.8.0\"", StringComparison.Ordinal));
            File.WriteAllText(Path.Combine(root, ComparisonDocumentFixture.PreviousExport), ExportData.FullJson);
            File.WriteAllBytes(Path.Combine(root, ComparisonDocumentFixture.CurrentExport), original);
            RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);

            // When
            var bundle = new ReportBundler(new FileSystem(), root).Create(ReportFixtures.Pair(), new ComparisonDocumentFixture(),
                [ReportFixtures.Mapping()], [previous, new RawExportFixture()], ".artifacts/benchmarks/comparison");

            // Then
            using var archive = new ZipArchive(new MemoryStream(File.ReadAllBytes(Path.Combine(root, bundle.Path))));
            using var content = new MemoryStream();
            using (var stream = archive.GetEntry("raw/current/Synthetic-report-full.json")!.Open())
            {
                stream.CopyTo(content);
            }

            content.ToArray().Should().Equal(original);
            var file = bundle.Manifest.Files.Single(static file => file.Path == "raw/current/Synthetic-report-full.json");
            (file.Transformation, file.SourceSha256).Should().Be(("none", Convert.ToHexStringLower(SHA256.HashData(original))));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Theory]
    [ClassData(typeof(UnsupportedExportRows))]
    public void UnsupportedExport_WhenBundling_ShouldRejectItAndWriteNoArchive(string fileName, string content)
    {
        // Given
        var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "previous"));
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "current"));
            var source = $".artifacts/benchmarks/current/{fileName}";
            File.WriteAllText(Path.Combine(root, ComparisonDocumentFixture.PreviousExport), ExportData.FullJson);
            File.WriteAllText(Path.Combine(root, source), content);
            RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);

            // When
            var create = () => new ReportBundler(new FileSystem(), root).Create(ReportFixtures.Pair(),
                new ComparisonDocumentFixture(ComparisonDocumentFixture.PreviousExport, source), [ReportFixtures.Mapping()],
                [previous, new RawExportFixture().WithSourcePath(source)], ".artifacts/benchmarks/comparison");

            // Then
            create.Should().Throw<InvalidDataException>();
            var workspace = Path.Combine(root, ".artifacts", "benchmarks", "comparison");
            (Directory.Exists(workspace) ? Directory.GetFiles(workspace, "*", SearchOption.AllDirectories) : Array.Empty<string>()).Should().BeEmpty();
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Theory]
    [ClassData(typeof(BrokenClosureRows))]
    public void BrokenEvidenceClosure_WhenBundling_ShouldRejectItAndWriteNoArchive(string defect)
    {
        // Given
        var parent = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var root = Path.Combine(parent, "repository");
        try
        {
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "previous"));
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "current"));
            Directory.CreateDirectory(Path.Combine(root, ".artifacts", "benchmarks", "other"));
            Directory.CreateDirectory(Path.Combine(parent, "outside"));
            File.WriteAllText(Path.Combine(root, ComparisonDocumentFixture.PreviousExport), ExportData.FullJson);
            File.WriteAllText(Path.Combine(root, ComparisonDocumentFixture.CurrentExport), ExportData.CurrentJson);
            File.WriteAllText(Path.Combine(root, SameName), ExportData.CurrentJson);
            File.WriteAllText(Path.Combine(root, CaseOnlyName), ExportData.CurrentJson);
            File.WriteAllText(Path.Combine(parent, "outside", "Synthetic-report-full.json"), ExportData.CurrentJson);
            const string escaping = "../outside/Synthetic-report-full.json";
            ReleasePair pair = ReportFixtures.Pair();
            ComparisonDocument document = new ComparisonDocumentFixture();
            RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
            RawExport current = new RawExportFixture();
            RawExport[] exports = [previous, current];
            if (defect == "unsupplied cited file")
            {
                exports = [previous];
            }
            else if (defect == "escaping source")
            {
                document = new ComparisonDocumentFixture(ComparisonDocumentFixture.PreviousExport, escaping);
                exports = [previous, current with { SourcePath = escaping }];
            }
            else if (defect == "session for another pair")
            {
                pair = pair with { CatalogDigest = new string('9', 64) };
            }
            else if (defect == "observation citing the other side's export")
            {
                document = new ComparisonDocumentFixture(ComparisonDocumentFixture.PreviousExport, ComparisonDocumentFixture.PreviousExport);
            }
            else if (defect == "two exports sharing a bundle path")
            {
                exports = [previous, current, current with { SourcePath = SameName }];
            }
            else if (defect == "two exports differing only by case")
            {
                exports = [previous, current, current with { SourcePath = CaseOnlyName }];
            }
            else if (defect == "symbolic link export")
            {
                File.Delete(Path.Combine(root, ComparisonDocumentFixture.CurrentExport));
                File.CreateSymbolicLink(Path.Combine(root, ComparisonDocumentFixture.CurrentExport),
                    Path.Combine(parent, "outside", "Synthetic-report-full.json"));
            }
            else
            {
                Directory.Delete(Path.Combine(root, ".artifacts", "benchmarks", "current"), true);
                Directory.CreateSymbolicLink(Path.Combine(root, ".artifacts", "benchmarks", "current"), Path.Combine(parent, "outside"));
            }

            // When
            var create = () => new ReportBundler(new FileSystem(), root).Create(pair, document, [ReportFixtures.Mapping()], [.. exports],
                ".artifacts/benchmarks/comparison");

            // Then
            create.Should().Throw<InvalidDataException>().Which.Message.Should().NotContain(parent);
            var workspace = Path.Combine(root, ".artifacts", "benchmarks", "comparison");
            (Directory.Exists(workspace) ? Directory.GetFiles(workspace, "*", SearchOption.AllDirectories) : Array.Empty<string>()).Should().BeEmpty();
        }
        finally
        {
            if (Directory.Exists(parent))
            {
                Directory.Delete(parent, true);
            }
        }
    }

    public sealed class RootedPathRows : TheoryData<string>
    {
        public RootedPathRows()
        {
            Add("/home/runner/work/tool/bin/Synthetic.dll");
            Add("~/work/tool/bin");
            Add(@"C:\runner\work\tool\bin");
            Add(@"\\server\share\tool");
        }
    }

    public sealed class UnsupportedExportRows : TheoryData<string, string>
    {
        public UnsupportedExportRows()
        {
            Add("Synthetic-report-full.json", ExportData.CurrentJson.Replace("\"0.15.8\"", "\"0.14.0\"", StringComparison.Ordinal));
            Add("Synthetic-report-full.json", "{ \"Title\": ");
            Add("Synthetic-report-full.json", ExportData.CurrentJson.Replace("\"Title\": \"Synthetic workload\",",
                "\"Title\": \"Synthetic workload\", \"Title\": \"Duplicate\",", StringComparison.Ordinal));
            Add("Synthetic-report.json", ExportData.CurrentJson);
            Add("Synthetic-report-github.md", "| Method | Mean |\n|------- |-----:|\n| Check  | 100 ns |\n");
            Add("Synthetic.log", "// Benchmark process started\n");
        }
    }

    public sealed class BrokenClosureRows : TheoryData<string>
    {
        public BrokenClosureRows()
        {
            Add("unsupplied cited file");
            Add("escaping source");
            Add("session for another pair");
            Add("observation citing the other side's export");
            Add("two exports sharing a bundle path");
            Add("two exports differing only by case");
            Add("symbolic link export");
            Add("symbolic link parent directory");
        }
    }

    private const string SameName = ".artifacts/benchmarks/other/Synthetic-report-full.json";
    private const string CaseOnlyName = ".artifacts/benchmarks/other/synthetic-report-full.json";

    private const string SmallKey = """{"operation":"check","parameters":[{"kind":"number","name":"Specifications","value":"1"}]}""";
    private const string LargeKey = """{"operation":"check","parameters":[{"kind":"number","name":"Specifications","value":"10"}]}""";
}
