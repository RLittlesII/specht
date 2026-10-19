using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Specht.Analysis.TestEvidence;

namespace Specht.TestEvidence.Mtp;

public sealed class TestEvidenceMtpCollector : IDataConsumer
{
    public string Uid => "specht.test-evidence.mtp";
    public string Version => AdapterVersion;
    public string DisplayName => "Specht test evidence collector";
    public string Description => "Collects stable test case and outcome records from Microsoft Testing Platform.";
    public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage)];

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
    {
        if (value is not TestNodeUpdateMessage update)
        {
            return Task.CompletedTask;
        }

        _sessionId ??= ((DataWithSessionUid)update).SessionUid.Value;
        var node = update.TestNode;
        var method = node.Properties.SingleOrDefault<TestMethodIdentifierProperty>();
        if (method is null || string.IsNullOrWhiteSpace(node.Uid.Value))
        {
            return Task.CompletedTask;
        }

        var caseId = node.Uid.Value;
        var metadata = new TestEvidenceRunnerMethodRecord(
            string.IsNullOrEmpty(method.Namespace) ? method.TypeName : method.Namespace + "." + method.TypeName,
            method.MethodName, method.MethodArity,
            method.ParameterTypeFullNames.Select(static type => type.StartsWith("global::", StringComparison.Ordinal)
                ? type : "global::" + type).ToArray());
        Cases[caseId] = new TestEvidenceRunnerCaseRecord(caseId, null, null, node.DisplayName, metadata);

        var state = node.Properties.OfType<TestNodeStateProperty>().FirstOrDefault();
        var outcome = state switch
        {
            PassedTestNodeStateProperty => "passed",
            FailedTestNodeStateProperty => "failed",
            SkippedTestNodeStateProperty => "skipped",
            _ => null,
        };
        if (outcome is not null)
        {
            Results[caseId] = new TestEvidenceRunnerResultRecord(caseId, outcome, state!.Explanation);
        }

        if (node.Properties.Any<DiscoveredTestNodeStateProperty>())
        {
            Interlocked.Exchange(ref _enumerationComplete, 1);
        }

        return Task.CompletedTask;
    }

    internal static Task FinishSessionAsync()
    {
        try
        {
            Export();
        }
        finally
        {
            Cases.Clear();
            Results.Clear();
            _sessionId = null;
            Interlocked.Exchange(ref _enumerationComplete, 0);
        }

        return Task.CompletedTask;
    }

    internal static void StartSession()
    {
        Cases.Clear();
        Results.Clear();
        _sessionId = null;
        Interlocked.Exchange(ref _enumerationComplete, 0);
    }

    private static void Export()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, ConfigurationFileName);
        if (!File.Exists(configPath))
        {
            return;
        }

        using var config = JsonDocument.Parse(File.ReadAllText(configPath));
        var outputPath = config.RootElement.GetProperty("outputPath").GetString();
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            return;
        }

        var exportAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(static assembly =>
            assembly.GetType(ExportTypeName, throwOnError: false) is not null);
        var exportType = exportAssembly?.GetType(ExportTypeName, throwOnError: false);
        var exportJson = exportType?.GetField("Json", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
            ?.GetRawConstantValue() as string;
        if (string.IsNullOrWhiteSpace(exportJson))
        {
            return;
        }

        using var payload = JsonDocument.Parse(exportJson);
        var root = payload.RootElement;
        var methods = JsonSerializer.Deserialize<TestEvidenceMethod[]>(root.GetProperty("methods").GetRawText(), JsonOptions) ?? [];
        var bindings = JsonSerializer.Deserialize<TestEvidenceBinding[]>(root.GetProperty("bindings").GetRawText(), JsonOptions) ?? [];
        var inputs = JsonSerializer.Deserialize<TestEvidenceInput[]>(root.GetProperty("inputs").GetRawText(), JsonOptions) ?? [];
        var cases = Cases.Values.OrderBy(static item => item.CaseId, StringComparer.Ordinal).ToArray();
        var outcomes = Results.Values.OrderBy(static item => item.CaseId, StringComparer.Ordinal).ToArray();
        var collection = new TestEvidenceRunnerCollector().Collect("mtp", AdapterVersion, methods, cases,
            _sessionId, Volatile.Read(ref _enumerationComplete) == 1, _sessionId,
            outcomes);
        var assemblyPath = exportAssembly?.Location;
        if (string.IsNullOrWhiteSpace(assemblyPath) || !File.Exists(assemblyPath))
        {
            return;
        }

        using var sha = System.Security.Cryptography.SHA256.Create();
        using var stream = File.OpenRead(assemblyPath);
        var assemblyHash = string.Concat(sha.ComputeHash(stream).Select(static value => value.ToString("x2")));
        var assemblyName = exportAssembly!.GetName().Name ?? "test-assembly";
        var artifact = new TestEvidenceArtifact(
            root.GetProperty("projectPath").GetString()!,
            root.GetProperty("targetFramework").GetString()!,
            root.GetProperty("configurationName").GetString()!,
            root.GetProperty("configurationFingerprint").GetString()!,
            assemblyName, assemblyHash,
            root.GetProperty("assemblyPath").GetString(),
            root.GetProperty("sourceFingerprint").GetString()!,
            root.GetProperty("policyFingerprint").GetString()!, "mtp", AdapterVersion, "1",
            collection.Discovery with { Methods = methods }, bindings, collection.Results, inputs, []);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        File.WriteAllText(outputPath, new TestEvidenceArtifactWriter().Write(artifact));
    }

    private const string AdapterVersion = "1.0.0";
    private const string ExportTypeName = "Specht.Generated.TestEvidenceCompilationExport";
    private const string ConfigurationFileName = "specht.test-evidence.config.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly ConcurrentDictionary<string, TestEvidenceRunnerCaseRecord> Cases = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, TestEvidenceRunnerResultRecord> Results = new(StringComparer.Ordinal);
    private static string? _sessionId;
    private static int _enumerationComplete;
}
