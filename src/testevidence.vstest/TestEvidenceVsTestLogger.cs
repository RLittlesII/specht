using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Specht.Analysis.Policy;
using Specht.Analysis.TestEvidence;

namespace Specht.TestEvidence.VsTest;

/// <summary>Writes the test-evidence artifact for each test source from VSTest discovery and run events (0201-F3 ADR-0001).</summary>
[FriendlyName("specht")]
[ExtensionUri("logger://specht/test-evidence")]
public sealed class TestEvidenceVsTestLogger : ITestLoggerWithParameters
{
    public TestEvidenceVsTestLogger()
        : this(new FileSystem())
    {
    }

    public TestEvidenceVsTestLogger(IFileSystem fileSystem) => _fileSystem = fileSystem;

    public void Initialize(TestLoggerEvents events, string testRunDirectory) => Subscribe(events);

    public void Initialize(TestLoggerEvents events, Dictionary<string, string?> parameters) => Subscribe(events);

    private static (string AssemblyName, string? Export) Export(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var portable = new PEReader(stream);
        var metadata = portable.GetMetadataReader();
        var assemblyName = metadata.GetString(metadata.GetAssemblyDefinition().Name);
        foreach (var handle in metadata.TypeDefinitions)
        {
            var type = metadata.GetTypeDefinition(handle);
            if (metadata.GetString(type.Namespace) != "Specht.Generated" || metadata.GetString(type.Name) != "TestEvidenceCompilationExport")
            {
                continue;
            }

            foreach (var field in type.GetFields().Select(metadata.GetFieldDefinition))
            {
                var constant = field.GetDefaultValue();
                if (metadata.GetString(field.Name) == "Json" && !constant.IsNil)
                {
                    var blob = metadata.GetBlobReader(metadata.GetConstant(constant).Value);
                    return (assemblyName, blob.ReadUTF16(blob.Length));
                }
            }
        }

        return (assemblyName, null);
    }

    private static string? Resolve(TestCase testCase, IReadOnlyList<TestEvidenceMethod> methods)
    {
        var type = Property(testCase, "TestCase.ManagedType");
        var name = Property(testCase, "TestCase.ManagedMethod");
        if (type is null && name is null)
        {
            var separator = testCase.FullyQualifiedName.LastIndexOf('.');
            if (separator <= 0)
            {
                return null;
            }

            type = testCase.FullyQualifiedName.Substring(0, separator);
            name = testCase.FullyQualifiedName.Substring(separator + 1);
        }

        var matches = methods.Where(method => method.DeclaringType == type && method.Name == name).ToArray();
        return matches.Length == 1 ? matches[0].SymbolId : null;
    }

    private static string? Property(TestCase testCase, string id) => testCase.Properties.FirstOrDefault(property => property.Id == id)
        is { } property ? testCase.GetPropertyValue(property) as string : null;

    private static string[] Categories(TestCase testCase) => testCase.Traits
        .Where(static trait => trait.Name == "Category" && !string.IsNullOrEmpty(trait.Value)).Select(static trait => trait.Value)
        .Distinct(StringComparer.Ordinal).OrderBy(static category => category, StringComparer.Ordinal).ToArray();

    private static string Outcome(TestOutcome outcome) => outcome switch
    {
        TestOutcome.Passed => "passed",
        TestOutcome.Failed => "failed",
        TestOutcome.Skipped => "skipped",
        _ => "notRun",
    };

    private static string CaseId(TestCase testCase) => testCase.Id.ToString("D");

    private static string Text(JsonNode payload, string name) => payload[name]!.GetValue<string>();

    private static string Hash(byte[] bytes)
    {
        using var sha = SHA256.Create();
        return string.Concat(sha.ComputeHash(bytes).Select(static value => value.ToString("x2")));
    }

    private void Subscribe(TestLoggerEvents events)
    {
        events.DiscoveryStart += (_, args) => Request(args.DiscoveryCriteria.Sources, null);
        events.DiscoveredTests += (_, args) => Cases(args.DiscoveredTestCases ?? []);
        events.DiscoveryComplete += (_, args) => CompleteDiscovery(args);
        events.TestRunStart += (_, args) => Request(args.TestRunCriteria.Sources ?? [],
            args.TestRunCriteria.HasSpecificTests ? args.TestRunCriteria.Tests : null);
        events.TestResult += (_, args) => Result(args.Result);
        events.TestRunComplete += (_, _) => CompleteRun();
    }

    private void Request(IEnumerable<string> sources, IEnumerable<TestCase>? named)
    {
        lock (_gate)
        {
            _sources.Clear();
            _cases.Clear();
            _named.Clear();
            _results.Clear();
            _sources.UnionWith(sources);
            Cases(named ?? []);
            _named.UnionWith((named ?? []).Select(static testCase => testCase.Id));
        }
    }

    private void Cases(IEnumerable<TestCase> cases)
    {
        lock (_gate)
        {
            foreach (var testCase in cases)
            {
                _cases[testCase.Id] = testCase;
                _sources.Add(testCase.Source);
            }
        }
    }

    private void Result(TestResult result)
    {
        lock (_gate)
        {
            Cases([result.TestCase]);
            if (!_results.TryGetValue(result.TestCase.Id, out var results))
            {
                _results[result.TestCase.Id] = results = [];
            }

            results.Add(result);
        }
    }

    private void CompleteDiscovery(DiscoveryCompleteEventArgs args)
    {
        lock (_gate)
        {
            foreach (var source in _sources)
            {
                var incomplete = new[] { args.PartiallyDiscoveredSources, args.SkippedDiscoveredSources, args.NotDiscoveredSources }
                    .Any(list => list?.Contains(source, StringComparer.Ordinal) == true);
                Write(source, args.IsAborted ? null : Guid.NewGuid().ToString(), !args.IsAborted && !incomplete, false);
            }
        }
    }

    private void CompleteRun()
    {
        lock (_gate)
        {
            foreach (var source in _sources)
            {
                Write(source, Guid.NewGuid().ToString(), false, true);
            }
        }
    }

    private void Write(string source, string? sessionId, bool complete, bool run)
    {
        var directory = _fileSystem.Path.GetDirectoryName(source) ?? string.Empty;
        var configurationPath = _fileSystem.Path.Combine(directory, ConfigurationFileName);
        if (!_fileSystem.File.Exists(configurationPath) || !_fileSystem.File.Exists(source))
        {
            return;
        }

        var configuration = JsonNode.Parse(_fileSystem.File.ReadAllText(configurationPath))!;
        var outputPath = configuration["outputPath"]?.GetValue<string>();
        var bytes = _fileSystem.File.ReadAllBytes(source);
        var (assemblyName, export) = Export(bytes);
        if (string.IsNullOrWhiteSpace(outputPath) || export is null)
        {
            return;
        }

        var payload = JsonNode.Parse(export)!;
        var methods = payload["methods"].Deserialize<TestEvidenceMethod[]>(Json) ?? [];
        var scope = new CaseScope(payload["requiredCaseScope"]?.GetValue<string>());
        var identity = new TestEvidenceArtifact(Text(payload, "projectPath"), Text(payload, "targetFramework"),
            Text(payload, "configurationName"), Text(payload, "configurationFingerprint"), assemblyName, Hash(bytes),
            configuration["assemblyPath"]?.GetValue<string>(), Text(payload, "sourceFingerprint"), Text(payload, "policyFingerprint"),
            AdapterId, AdapterVersion, ContractVersion,
            new TestEvidenceDiscovery(TestEvidenceDiscoveryState.Deferred, sessionId, methods, []),
            payload["bindings"].Deserialize<TestEvidenceBinding[]>(Json) ?? [],
            new TestEvidenceResults("absent", null, []),
            payload["inputs"].Deserialize<TestEvidenceInput[]>(Json) ?? [], []);
        var cases = _cases.Values.Where(testCase => testCase.Source == source).ToDictionary(static testCase => CaseId(testCase),
            StringComparer.Ordinal);
        var records = cases.Values.Select(testCase => new TestEvidenceRunnerCaseRecord(CaseId(testCase), Resolve(testCase, methods), null,
            testCase.DisplayName)).ToList();
        var categories = cases.ToDictionary(static entry => entry.Key, static entry => (IReadOnlyList<string>)Categories(entry.Value),
            StringComparer.Ordinal);
        var selected = new HashSet<string>(StringComparer.Ordinal);
        var outcomes = new List<TestEvidenceRunnerResultRecord>();
        string? runId = null;
        if (run)
        {
            runId = Guid.NewGuid().ToString();
            var previous = Previous(outputPath!, identity);
            if (previous is not null)
            {
                complete = previous.State == TestEvidenceDiscoveryState.Complete && cases.Keys.All(caseId =>
                    previous.Cases.Any(item => item.CaseId == caseId));
                sessionId = previous.SessionId;
                foreach (var item in previous.Cases.Where(item => !cases.ContainsKey(item.CaseId)))
                {
                    records.Add(new TestEvidenceRunnerCaseRecord(item.CaseId, item.MethodSymbolId, item.RowId, item.DisplayName));
                    categories[item.CaseId] = item.Categories ?? [];
                }
            }

            foreach (var (caseId, testCase) in cases.Where(entry => _named.Contains(entry.Value.Id) || _results.ContainsKey(entry.Value.Id)))
            {
                selected.Add(caseId);
                var results = _results.TryGetValue(testCase.Id, out var reported) ? reported : [];
                if (results.Count > 1)
                {
                    complete = false;
                    continue;
                }

                outcomes.Add(results.Count == 0 ? new TestEvidenceRunnerResultRecord(caseId, "notRun", null)
                    : new TestEvidenceRunnerResultRecord(caseId, Outcome(results[0].Outcome), results[0].ErrorMessage));
            }
        }

        var collection = new TestEvidenceRunnerCollector().Collect(AdapterId, AdapterVersion, methods,
            records.OrderBy(static record => record.CaseId, StringComparer.Ordinal).ToArray(), sessionId, complete, runId,
            outcomes.OrderBy(static outcome => outcome.CaseId, StringComparer.Ordinal).ToArray());
        var discovered = collection.Discovery.Cases.Select(item => item with
        {
            Categories = categories[item.CaseId],
            Owned = scope.Owns(categories[item.CaseId]),
            Selected = selected.Contains(item.CaseId),
        }).ToArray();
        var artifact = identity with
        {
            Discovery = collection.Discovery with { Cases = discovered },
            Results = collection.Results,
        };
        _fileSystem.Directory.CreateDirectory(_fileSystem.Path.GetDirectoryName(_fileSystem.Path.GetFullPath(outputPath!))!);
        _fileSystem.File.WriteAllText(outputPath!, new TestEvidenceArtifactWriter().Write(artifact));
    }

    private TestEvidenceDiscovery? Previous(string outputPath, TestEvidenceArtifact identity)
    {
        if (!_fileSystem.File.Exists(outputPath))
        {
            return null;
        }

        var read = new TestEvidenceArtifactReader().Read(_fileSystem.File.ReadAllText(outputPath));
        return read.Artifact is { Discovery.State: TestEvidenceDiscoveryState.Complete or TestEvidenceDiscoveryState.Deferred } previous
            && new TestEvidenceCaseCorrelation().Correlate(previous, identity).IsCurrent
            ? previous.Discovery
            : null;
    }

    private const string AdapterId = "vstest";
    private const string AdapterVersion = "1.0.0";
    private const string ContractVersion = "1";
    private const string ConfigurationFileName = "specht.test-evidence.config.json";
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private readonly IFileSystem _fileSystem;
    private readonly object _gate = new();
    private readonly SortedSet<string> _sources = new(StringComparer.Ordinal);
    private readonly Dictionary<Guid, TestCase> _cases = [];
    private readonly HashSet<Guid> _named = [];
    private readonly Dictionary<Guid, List<TestResult>> _results = [];
}
