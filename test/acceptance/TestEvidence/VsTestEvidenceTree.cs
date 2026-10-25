using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Specht.Analysis.TestEvidence;
using Specht.TestEvidence.VsTest;
using Specht.Tool.Features.Evidence;
using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace Specht.Acceptance.TestEvidence;

/// <summary>
/// A consumer root on disk whose projects compile the same shared claim and test files, each with the compiler export a
/// build would generate, and the VSTest logger as VSTest constructs it, driven by synthetic discovery and run events
/// (0201-F3 § 7 VSTest collection).
/// </summary>
internal sealed class VsTestEvidenceTree : IDisposable
{
    public string Root { get; } = Directory.CreateTempSubdirectory("specht-vstest-evidence-").FullName;

    public void Declare(string project, string? requiredCaseScope) => _scopes[project] = requiredCaseScope;

    public void BindUndiscovered() => _bindUndiscovered = true;

    public TestCase Case(string project, string id, string type, string method, params string[] categories)
    {
        var testCase = new TestCase("Shared." + type + "." + method, new Uri("executor://xunit/VsTestRunner3/netcore/"), Assembly(project))
        {
            Id = Guid.Parse(id),
            DisplayName = type + "." + method + "(" + id[..4] + ")",
        };
        testCase.SetPropertyValue(ManagedType, "Shared." + type);
        testCase.SetPropertyValue(ManagedMethod, method);
        foreach (var category in categories)
        {
            testCase.Traits.Add("Category", category);
        }

        return testCase;
    }

    public void Discover(string project, params TestCase[] cases)
    {
        var events = Attach();
        events.Raise(new DiscoveryStartEventArgs(new DiscoveryCriteria([Assembly(project)], 1, null)));
        events.Raise(new DiscoveredTestsEventArgs(cases));
        events.Raise(new DiscoveryCompleteEventArgs(cases.Length, false) { FullyDiscoveredSources = [Assembly(project)] });
    }

    public void Run(string project, IReadOnlyList<TestCase>? named, IEnumerable<VsTestResult> results)
    {
        var events = Attach();
        events.Raise(new TestRunStartEventArgs(named is null ? new TestRunCriteria([Assembly(project)], 1) : new TestRunCriteria(named, 1)));
        foreach (var result in results)
        {
            events.Raise(new TestResultEventArgs(result));
        }

        events.Raise(new TestRunCompleteEventArgs(new TestRunStatistics(), false, false, null, new Collection<AttachmentSet>(), TimeSpan.Zero));
    }

    public JsonNode Artifact(string project) => JsonNode.Parse(File.ReadAllText(Path.Combine(Root, Output(project))))!;

    public EvidenceResult Reconcile() =>
        new EvidenceReconciler().Reconcile(Root, _scopes.Keys.Order(StringComparer.Ordinal).Select(Output).ToArray(), null);

    public EvidenceResult ReconcileWithoutEvidence()
    {
        Prepare();
        return new EvidenceReconciler().Reconcile(Root, [], null);
    }

    public void EditSource() => File.AppendAllText(Path.Combine(Root, "test/shared/SharedCases.cs"), "\n");

    public void Dispose() => Directory.Delete(Root, true);

    private static string Output(string project) => $"test/{project}/obj/{project}-net10.0.json";

    private static string AssemblyPath(string project) => $"test/{project}/bin/{project}.dll";

    private string Assembly(string project) => Path.Combine(Root, AssemblyPath(project));

    private LoggerEvents Attach()
    {
        Prepare();
        var events = new LoggerEvents();
        new TestEvidenceVsTestLogger().Initialize(events, new Dictionary<string, string?> { ["TestRunDirectory"] = Root });
        return events;
    }

    private void Prepare()
    {
        if (_prepared)
        {
            return;
        }

        _prepared = true;
        Write("src/read/.spec/README.md", """
            ---
            id: cap:read
            spec_status: approved
            ---
            ## 3. Acceptance Criteria
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | need/7 | A synthetic need | Agreement | Active |
            """);
        Write("test/shared/SharedCases.cs", "namespace Shared { class SharedCases { } }");
        var projects = new JsonArray();
        foreach (var (project, scope) in _scopes.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            var entry = new JsonObject { ["path"] = $"test/{project}/{project}.csproj", ["targetFrameworks"] = new JsonArray("net10.0") };
            if (scope is not null)
            {
                entry["requiredCaseScope"] = scope;
            }

            projects.Add(entry);
        }

        Write(".specht/manifest.json", new JsonObject
        {
            ["revision"] = "0.1.0",
            ["idPatterns"] = new JsonObject { ["feature"] = "^cap:[a-z]+$", ["claim"] = "^need/[0-9]+$" },
            ["claimHeading"] = "3. Acceptance Criteria",
            ["lifecycleMarkers"] = new JsonObject { ["approved"] = "approved", ["withdrawn"] = "Withdrawn", ["superseded"] = "Superseded" },
            ["codeAnalysis"] = new JsonObject
            {
                ["mappings"] = new JsonObject
                {
                    ["/schemaVersion"] = "/revision",
                    ["/identifiers"] = "/idPatterns",
                    ["/claimsSection"] = "/claimHeading",
                    ["/markers"] = "/lifecycleMarkers",
                    ["/policy"] = "/codeAnalysis",
                },
                ["projects"] = projects,
                ["features"] = new JsonArray(new JsonObject { ["feature"] = "cap:read" }),
                ["requiredLinks"] = new JsonObject { ["test"] = true },
                ["enforcement"] = new JsonArray(),
                ["exemptions"] = new JsonArray(),
            },
        }.ToJsonString());
        var inputs = new[]
        {
            Input(".specht/manifest.json", "manifest"),
            Input("src/read/.spec/README.md", "record"),
            Input("test/shared/SharedCases.cs", "source"),
        };
        var fingerprint = new TestEvidenceFingerprint();
        var target = new TestEvidenceTarget("claim", null, "cap:read", "need/7", "Active", "approved", "src/read/.spec/README.md", 8);
        foreach (var (project, scope) in _scopes)
        {
            var payload = new JsonObject
            {
                ["projectPath"] = $"test/{project}/{project}.csproj",
                ["targetFramework"] = "net10.0",
                ["configurationName"] = "Debug",
                ["assemblyPath"] = AssemblyPath(project),
                ["configurationFingerprint"] = new string('c', 64),
                ["sourceFingerprint"] = fingerprint.ComputeDigests(inputs.Where(static input => input.Kind == "source")),
                ["policyFingerprint"] = fingerprint.ComputeDigests(inputs.Where(static input => input.Kind != "source")),
                ["methods"] = JsonSerializer.SerializeToNode(Methods, Wire),
                ["bindings"] = JsonSerializer.SerializeToNode(Methods
                    .Where(method => method.Location is not null && (_bindUndiscovered || method.Name != "Undiscovered"))
                    .Select(method => new TestEvidenceBinding(method.SymbolId, "verifies", new Dictionary<string, string> { ["claim"] = "need/7" },
                        true, "resolved", true, target, [], method.Location)).ToArray(), Wire),
                ["inputs"] = JsonSerializer.SerializeToNode(inputs, Wire),
                ["requiredCaseScope"] = scope,
            };
            var tree = CSharpSyntaxTree.ParseText("namespace Specht.Generated { internal static class TestEvidenceCompilationExport { "
                + "internal const string Json = " + SymbolDisplay.FormatLiteral(payload.ToJsonString(), true) + "; } }");
            var compilation = CSharpCompilation.Create(project, [tree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            Directory.CreateDirectory(Path.GetDirectoryName(Assembly(project))!);
            using (var assembly = File.Create(Assembly(project)))
            {
                if (!compilation.Emit(assembly).Success)
                {
                    throw new InvalidOperationException("The synthetic compiler export did not compile.");
                }
            }

            Write($"test/{project}/bin/specht.test-evidence.config.json", new JsonObject
            {
                ["outputPath"] = Path.Combine(Root, Output(project)),
                ["assemblyPath"] = AssemblyPath(project),
            }.ToJsonString());
        }
    }

    private void Write(string relativePath, string text)
    {
        var path = Path.Combine(Root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    private TestEvidenceInput Input(string relativePath, string kind) =>
        new(relativePath, Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(Root, relativePath)))), kind);

    private sealed class LoggerEvents : TestLoggerEvents
    {
        public override event EventHandler<TestRunMessageEventArgs>? TestRunMessage;
        public override event EventHandler<TestRunStartEventArgs>? TestRunStart;
        public override event EventHandler<TestResultEventArgs>? TestResult;
        public override event EventHandler<TestRunCompleteEventArgs>? TestRunComplete;
        public override event EventHandler<DiscoveryStartEventArgs>? DiscoveryStart;
        public override event EventHandler<TestRunMessageEventArgs>? DiscoveryMessage;
        public override event EventHandler<DiscoveredTestsEventArgs>? DiscoveredTests;
        public override event EventHandler<DiscoveryCompleteEventArgs>? DiscoveryComplete;

        public void Raise(EventArgs arguments)
        {
            switch (arguments)
            {
                case DiscoveryStartEventArgs start:
                    DiscoveryStart?.Invoke(this, start);
                    DiscoveryMessage?.Invoke(this, new TestRunMessageEventArgs(TestMessageLevel.Informational, "discovering"));
                    break;
                case DiscoveredTestsEventArgs discovered:
                    DiscoveredTests?.Invoke(this, discovered);
                    break;
                case DiscoveryCompleteEventArgs complete:
                    DiscoveryComplete?.Invoke(this, complete);
                    break;
                case TestRunStartEventArgs run:
                    TestRunStart?.Invoke(this, run);
                    TestRunMessage?.Invoke(this, new TestRunMessageEventArgs(TestMessageLevel.Informational, "running"));
                    break;
                case TestResultEventArgs result:
                    TestResult?.Invoke(this, result);
                    break;
                case TestRunCompleteEventArgs completed:
                    TestRunComplete?.Invoke(this, completed);
                    break;
            }
        }
    }

    private static readonly JsonSerializerOptions Wire = new(JsonSerializerDefaults.Web);

    private static readonly TestProperty ManagedType = TestProperty.Register("TestCase.ManagedType", "ManagedType", typeof(string),
        typeof(TestCase));

    private static readonly TestProperty ManagedMethod = TestProperty.Register("TestCase.ManagedMethod", "ManagedMethod", typeof(string),
        typeof(TestCase));

    private static readonly TestEvidenceMethod[] Methods =
    [
        new("M:Shared.LocalCases.Theory(System.Int32)", "Shared.LocalCases", "Theory", 0, ["global::System.Int32"],
            new TestEvidenceLocation("test/shared/SharedCases.cs", 10, 5)),
        new("M:Shared.LocalCases.Fact", "Shared.LocalCases", "Fact", 0, [], new TestEvidenceLocation("test/shared/SharedCases.cs", 20, 5)),
        new("M:Shared.LocalCases.Undiscovered", "Shared.LocalCases", "Undiscovered", 0, [],
            new TestEvidenceLocation("test/shared/SharedCases.cs", 30, 5)),
        new("M:Shared.SharedClaimsFeature.TwoRows(System.String,System.String[])", "Shared.SharedClaimsFeature", "TwoRows", 0,
            ["global::System.String", "global::System.String[]"], null),
    ];

    private readonly Dictionary<string, string?> _scopes = new(StringComparer.Ordinal);
    private bool _prepared;
    private bool _bindUndiscovered;
}
