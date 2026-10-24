using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed partial class TestEvidenceVsTestIntegrationTests(TestEvidenceVsTestIntegrationTests.VsTestRun run)
    : IClassFixture<TestEvidenceVsTestIntegrationTests.VsTestRun>
{
    [Fact]
    public async Task BoundTheory_WhenDiscoveredThenRunUnderVsTest_ShouldKeepOneStableCasePerRowOnTheBoundMethod()
    {
        // Given
        var evidence = await Collected();

        // When
        var discovery = evidence.Artifact("A", afterRun: false);
        var executed = evidence.Artifact("A", afterRun: true);

        // Then
        discovery.GetProperty("formatVersion").GetInt32().Should().Be(3, evidence.Because);
        discovery.GetProperty("discovery").GetProperty("state").GetString().Should().Be("complete", evidence.Because);
        var discovered = CaseIds(discovery, TheoryMethod);
        discovered.Should().HaveCount(2, evidence.Because).And.OnlyHaveUniqueItems(evidence.Because);
        CaseIds(executed, TheoryMethod).Should().BeEquivalentTo(discovered, evidence.Because);
    }

    [Fact]
    public async Task BoundTheoryWithAFailingRow_WhenRunUnderVsTest_ShouldRecordEachRowsOwnOutcome()
    {
        // Given
        var evidence = await Collected();

        // When
        var executed = evidence.Artifact("A", afterRun: true);

        // Then
        var outcomes = CaseIds(executed, TheoryMethod).Select(caseId => Outcome(executed, caseId)).ToArray();
        outcomes.Should().BeEquivalentTo(new[] { "passed", "failed" }, evidence.Because);
    }

    [Fact]
    public async Task SkippedFact_WhenRunUnderVsTest_ShouldRecordSkippedAndNeverPassed()
    {
        // Given
        var evidence = await Collected();

        // When
        var executed = evidence.Artifact("A", afterRun: true);

        // Then
        var skipped = CaseIds(executed, SkippedMethod).Should().ContainSingle(evidence.Because).Subject;
        Outcome(executed, skipped).Should().Be("skipped", evidence.Because);
    }

    [Fact]
    public async Task MethodLinkedFromOutsideTheProject_WhenDiscoveredUnderVsTest_ShouldResolveToItsRepositoryRelativeSource()
    {
        // Given
        var evidence = await Collected();

        // When
        var discovery = evidence.Artifact("A", afterRun: false);

        // Then
        CaseIds(discovery, VisualMethod).Should().ContainSingle(evidence.Because);
        var method = discovery.GetProperty("discovery").GetProperty("methods").EnumerateArray()
            .Should().ContainSingle(static method => method.GetProperty("symbolId").GetString() == VisualMethod, evidence.Because).Subject;
        method.GetProperty("location").GetProperty("path").GetString().Should().Be("shared/SharedCases.cs", evidence.Because);
    }

    [Theory]
    [ClassData(typeof(CategoryRows))]
    public async Task CategorizedCase_WhenEachProjectRunsItsOwnCategoryFilter_ShouldRecordItsCategoriesAndItsRunsSelection(
        string project, string symbolId, string[] categories, bool selected)
    {
        // Given
        var evidence = await Collected();

        // When
        var executed = evidence.Artifact(project, afterRun: true);

        // Then
        executed.GetProperty("project").GetProperty("path").GetString().Should().Be($"{project}/{project}.csproj", evidence.Because);
        var record = Case(executed, symbolId, evidence.Because);
        record.GetProperty("categories").EnumerateArray().Select(static category => category.GetString())
            .Should().Equal(categories, evidence.Because);
        record.GetProperty("selected").GetBoolean().Should().Be(selected, evidence.Because);
    }

    [Theory]
    [ClassData(typeof(OwnershipRows))]
    public async Task RequiredCaseScope_WhenProjectsDeclareComplementaryScopes_ShouldMarkOnlyMatchingCasesOwned(
        string project, string symbolId, bool owned)
    {
        // Given
        var evidence = await Collected();

        // When
        var discovery = evidence.Artifact(project, afterRun: false);

        // Then
        Case(discovery, symbolId, evidence.Because).GetProperty("owned").GetBoolean().Should().Be(owned, evidence.Because);
    }

    [Fact]
    public async Task ReqnrollScenarioOutline_WhenRunUnderVsTest_ShouldRecordBothGeneratedCasesAndTheirOutcomes()
    {
        // Given
        var evidence = await Collected();

        // When
        var executed = evidence.Artifact("A", afterRun: true);

        // Then
        var outline = executed.GetProperty("discovery").GetProperty("methods").EnumerateArray()
            .Should().ContainSingle(static method => method.GetProperty("name").GetString() == "NonvisualClaim", evidence.Because)
            .Subject.GetProperty("symbolId").GetString()!;
        var cases = CaseIds(executed, outline);
        cases.Should().HaveCount(2, evidence.Because).And.OnlyHaveUniqueItems(evidence.Because);
        cases.Select(caseId => Outcome(executed, caseId)).Should().AllBeEquivalentTo("passed", evidence.Because);
    }

    public sealed class CategoryRows : TheoryData<string, string, string[], bool>
    {
        public CategoryRows()
        {
            Add("A", VisualMethod, ["smoke", "visual"], false);
            Add("A", NonvisualMethod, [], true);
            Add("B", VisualMethod, ["smoke", "visual"], true);
            Add("B", NonvisualMethod, [], false);
        }
    }

    public sealed class OwnershipRows : TheoryData<string, string, bool>
    {
        public OwnershipRows()
        {
            Add("A", VisualMethod, false);
            Add("A", NonvisualMethod, true);
            Add("B", VisualMethod, true);
            Add("B", NonvisualMethod, false);
        }
    }

    public sealed record VsTestEvidence(IReadOnlyDictionary<string, JsonElement?> Artifacts, string Because)
    {
        public JsonElement Artifact(string project, bool afterRun)
        {
            var artifact = Artifacts[project + (afterRun ? "-run" : "-list")];
            artifact.Should().NotBeNull(Because);
            return artifact!.Value;
        }
    }

    public sealed partial class VsTestRun : IAsyncDisposable
    {
        public VsTestRun()
        {
            var work = Path.Combine(_root, ".artifacts", "testevidence-vstest", Guid.NewGuid().ToString("N"));
            _probe = Path.Combine(work, "probe.txt");
            _feed = Path.Combine(work, "feed");
            _evidence = new Lazy<Task<VsTestEvidence>>(Collect);
        }

        public Task<VsTestEvidence> Evidence => _evidence.Value;

        public ValueTask DisposeAsync()
        {
            if (Directory.Exists(_consumer))
            {
                Directory.Delete(_consumer, recursive: true);
            }

            if (Directory.Exists(_feed))
            {
                Directory.Delete(_feed, recursive: true);
            }

            return ValueTask.CompletedTask;
        }

        private static void Describe(JsonElement element, ISet<string> cases, ISet<string> ids)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    Describe(item, cases, ids);
                }

                return;
            }

            if (element.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            var properties = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name is "Id" or "FullyQualifiedName" or "DisplayName" && property.Value.ValueKind == JsonValueKind.String)
                {
                    properties["TestCase." + property.Name] = property.Value.GetString()!;
                }
                else if (property.Name == "Properties" && property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var pair in property.Value.EnumerateArray())
                    {
                        if (pair.ValueKind == JsonValueKind.Object && pair.TryGetProperty("Key", out var key) && key.ValueKind == JsonValueKind.Object
                            && key.TryGetProperty("Id", out var id) && pair.TryGetProperty("Value", out var value))
                        {
                            properties[id.GetString()!] = value.ValueKind == JsonValueKind.String ? value.GetString()! : value.GetRawText();
                        }
                    }
                }
                else
                {
                    Describe(property.Value, cases, ids);
                    if (property.Name == "TestCase" && property.Value.ValueKind == JsonValueKind.Object
                        && property.Value.TryGetProperty("Id", out var testCase) && testCase.ValueKind == JsonValueKind.String)
                    {
                        properties["TestResult.TestCaseId"] = testCase.GetString()!;
                    }
                }
            }

            if (properties.TryGetValue("TestCase.FullyQualifiedName", out _) && properties.TryGetValue("TestCase.Id", out var caseId)
                && GuidPattern().IsMatch(caseId))
            {
                ids.Add(caseId);
                cases.Add(string.Join(" | ", properties.Where(static pair => pair.Key is not ("TestCase.Source" or "TestCase.CodeFilePath"))
                    .Select(static pair => pair.Key + "=" + pair.Value)));
            }
            else if (properties.Count > 0)
            {
                cases.Add(string.Join(" | ", properties.Where(static pair => pair.Key.StartsWith("TestResult.", StringComparison.Ordinal))
                    .Select(static pair => pair.Key + "=" + pair.Value)));
            }
        }

        private static async Task<int> Run(StringBuilder log, string directory, params string[] arguments)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            foreach (var argument in arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            process.Start();
            var stdout = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            var stderr = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            log.AppendLine($"== dotnet {string.Join(' ', arguments.Take(2))} exited {process.ExitCode}");
            log.AppendLine(await stdout + await stderr);
            return process.ExitCode;
        }

        private static string FindRepositoryRoot()
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "specht.slnx")))
                {
                    return directory.FullName;
                }
            }

            throw new DirectoryNotFoundException("Could not locate the repository root.");
        }

        [GeneratedRegex("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")]
        private static partial Regex GuidPattern();

        private async Task<VsTestEvidence> Collect()
        {
            var log = new StringBuilder();
            var artifacts = new Dictionary<string, JsonElement?>(StringComparer.Ordinal);
            Directory.CreateDirectory(_feed);
            var pack = await Run(log, _root, "pack", "src/codeanalysis/codeanalysis.csproj", "--configuration", "Debug", "--output", _feed,
                "--no-restore");
            var package = Directory.GetFiles(_feed, "Specht.CodeAnalysis.*.nupkg").SingleOrDefault();
            if (pack != 0 || package is null)
            {
                return await Finish(log, artifacts);
            }

            await Arrange(Path.GetFileNameWithoutExtension(package)["Specht.CodeAnalysis.".Length..]);
            foreach (var (project, filter) in new[] { ("A", "Category!=visual"), ("B", "Category=visual") })
            {
                var path = $"{project}/{project}.csproj";
                await Run(log, _consumer, "restore", path, "--packages", Path.Combine(_consumer, "packages"));
                await Run(log, _consumer, "build", path, "--no-restore");
                await Run(log, _consumer, "test", path, "--no-build", "--list-tests", "--diag", $".diag/{project}-list.log");
                artifacts[project + "-list"] = Artifact(log, project + "-list", project);
                await Run(log, _consumer, "test", path, "--no-build", "--filter", filter, "--diag", $".diag/{project}-run.log");
                artifacts[project + "-run"] = Artifact(log, project + "-run", project);
            }

            return await Finish(log, artifacts);
        }

        private async Task Arrange(string version)
        {
            var fixture = Path.Combine(_root, "test", "testevidence.vstest.fixture");
            foreach (var file in Directory.GetFiles(fixture, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(fixture, file);
                if (relative.Split(Path.DirectorySeparatorChar).Any(static part => part is "bin" or "obj"))
                {
                    continue;
                }

                var target = Path.Combine(_consumer, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target);
            }

            File.Copy(Path.Combine(_root, "Directory.Packages.props"), Path.Combine(_consumer, "Directory.Packages.props"));
            using var sdk = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(_root, "global.json")));
            await Write("global.json", $$"""
                {
                  "sdk": {{sdk.RootElement.GetProperty("sdk").GetRawText()}},
                  "test": { "runner": "VSTest" }
                }
                """);
            await Write("nuget.config", $"""
                <?xml version="1.0" encoding="utf-8"?>
                <configuration>
                  <packageSources>
                    <clear />
                    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
                    <add key="fixture" value="{_feed}" />
                  </packageSources>
                  <packageSourceMapping>
                    <clear />
                    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
                    <packageSource key="fixture"><package pattern="Specht.CodeAnalysis" /></packageSource>
                  </packageSourceMapping>
                </configuration>
                """);
            await Write("Directory.Build.props", $"""
                <Project>
                  <PropertyGroup><SpechtCodeAnalysisPackageVersion>{version}</SpechtCodeAnalysisPackageVersion></PropertyGroup>
                </Project>
                """);
            await Write(".specht/manifest.json", """
                {
                  "revision": "fixture-r1",
                  "idPatterns": { "feature": "^F[0-9]+$", "claim": "^B-[0-9]+$", "adr": "^ADR-[0-9]+$" },
                  "claimHeading": "Claims",
                  "lifecycleMarkers": { "approved": "approved", "withdrawn": "Withdrawn", "superseded": "Superseded" },
                  "codeAnalysis": {
                    "projects": [
                      { "path": "A/A.csproj", "targetFrameworks": ["net10.0", "net11.0"], "requiredCaseScope": "not @visual" },
                      { "path": "B/B.csproj", "targetFrameworks": ["net10.0", "net11.0"], "requiredCaseScope": "@visual" }
                    ],
                    "features": [{ "feature": "F1" }],
                    "mappings": {
                      "/schemaVersion": "/revision",
                      "/identifiers": "/idPatterns",
                      "/claimsSection": "/claimHeading",
                      "/markers": "/lifecycleMarkers"
                    }
                  }
                }
                """);
            await Write(".specht/schema/frontmatter/feature.schema.json", """
                {"type":"object","required":["id","spec_status"],"properties":{"id":{"type":"string"},"spec_status":{"type":"string"}}}
                """);
            await Write("claims/.spec/README.md", """
                ---
                id: F1
                spec_status: draft
                ---
                ## Claims
                | ID | Status |
                | --- | --- |
                | B-001 | Active |
                | B-002 | Active |
                """);
        }

        private async Task Write(string relative, string content)
        {
            var path = Path.Combine(_consumer, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(path, content, TestContext.Current.CancellationToken);
        }

        private JsonElement? Artifact(StringBuilder log, string step, string project)
        {
            var folder = Path.Combine(_consumer, project, "obj", "specht-test-evidence");
            var files = Directory.Exists(folder) ? Directory.GetFiles(folder, "*.json") : [];
            log.AppendLine($"== artifact after {step}: {files.Length} file(s) under {project}/obj/specht-test-evidence");
            if (files.Length != 1)
            {
                return null;
            }

            var text = File.ReadAllText(files[0]);
            log.AppendLine(text);
            using var document = JsonDocument.Parse(text);
            return document.RootElement.Clone();
        }

        private async Task<VsTestEvidence> Finish(StringBuilder log, Dictionary<string, JsonElement?> artifacts)
        {
            log.AppendLine(Probe());
            Directory.CreateDirectory(Path.GetDirectoryName(_probe)!);
            await File.WriteAllTextAsync(_probe, Relativized(log.ToString()), TestContext.Current.CancellationToken);
            foreach (var key in new[] { "A-list", "A-run", "B-list", "B-run" })
            {
                artifacts.TryAdd(key, null);
            }

            var because = "the VSTest fixture run is recorded in " + Path.GetRelativePath(_root, _probe).Replace('\\', '/');
            return new VsTestEvidence(artifacts, because);
        }

        private string Relativized(string text)
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            foreach (var (path, placeholder) in new[] { (_consumer, "<consumer>"), (_root, "<repository>"), (home, "<home>") })
            {
                foreach (var form in new[] { "/private" + path, path, path.Replace('\\', '/') })
                {
                    text = text.Replace(form, placeholder, StringComparison.Ordinal);
                }
            }

            return text;
        }

        private string Probe()
        {
            var probe = new StringBuilder();
            var diag = Path.Combine(_consumer, ".diag");
            var identities = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            foreach (var step in new[] { "A-list", "A-run", "B-list", "B-run" })
            {
                var cases = new SortedSet<string>(StringComparer.Ordinal);
                var ids = new HashSet<string>(StringComparer.Ordinal);
                var files = Directory.Exists(diag) ? Directory.GetFiles(diag, step + "*") : [];
                foreach (var line in files.SelectMany(File.ReadLines))
                {
                    var start = line.IndexOf('{');
                    if (start < 0 || !line.Contains("FullyQualifiedName", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    try
                    {
                        using var message = JsonDocument.Parse(line[start..]);
                        Describe(message.RootElement, cases, ids);
                    }
                    catch (JsonException)
                    {
                    }
                }

                identities[step] = ids;
                probe.AppendLine($"== VSTest test cases in {step} diagnostics ({files.Length} file(s)): {cases.Count}");
                foreach (var description in cases)
                {
                    probe.AppendLine(description);
                }
            }

            foreach (var project in new[] { "A", "B" })
            {
                var listed = identities[project + "-list"];
                var executed = identities[project + "-run"];
                probe.AppendLine($"== {project}: TestCase.Id listed {listed.Count}, run {executed.Count}, run ids absent from the list "
                    + executed.Except(listed).Count());
            }

            return probe.ToString();
        }

        private readonly string _root = FindRepositoryRoot();
        private readonly string _consumer = Path.Combine(Path.GetTempPath(), "specht-vstest-consumer-" + Guid.NewGuid().ToString("N"));
        private readonly string _probe;
        private readonly string _feed;
        private readonly Lazy<Task<VsTestEvidence>> _evidence;
    }

    private static string[] CaseIds(JsonElement artifact, string symbolId) =>
        artifact.GetProperty("discovery").GetProperty("cases").EnumerateArray()
            .Where(record => record.GetProperty("methodSymbolId").GetString() == symbolId)
            .Select(static record => record.GetProperty("caseId").GetString()!)
            .ToArray();

    private static JsonElement Case(JsonElement artifact, string symbolId, string because) =>
        artifact.GetProperty("discovery").GetProperty("cases").EnumerateArray()
            .Should().ContainSingle(record => record.GetProperty("methodSymbolId").GetString() == symbolId, because).Subject;

    private static string? Outcome(JsonElement artifact, string caseId) =>
        artifact.GetProperty("results").GetProperty("cases").EnumerateArray()
            .Where(result => result.GetProperty("caseId").GetString() == caseId)
            .Select(static result => result.GetProperty("outcome").GetString())
            .SingleOrDefault();

    private async Task<VsTestEvidence> Collected()
    {
        if (!AppContext.TargetFrameworkName!.EndsWith("v10.0", StringComparison.Ordinal))
        {
            Assert.Skip("The VSTest evidence fixture runs once against the repository's .NET 10 consumer runtime.");
        }

        return await run.Evidence;
    }

    private const string TheoryMethod = "M:Fixture.A.LocalCases.BoundTheory(System.Int32)";
    private const string SkippedMethod = "M:Fixture.A.LocalCases.SkippedFact";
    private const string VisualMethod = "M:Fixture.Shared.SharedCases.VisualCase";
    private const string NonvisualMethod = "M:Fixture.Shared.SharedCases.NonvisualCase";
}
