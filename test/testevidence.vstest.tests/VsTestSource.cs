using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Specht.Analysis.TestEvidence;

namespace Specht.TestEvidence.VsTest.Tests;

internal sealed class VsTestSource
{
    public VsTestSource(string? requiredCaseScope = null)
    {
        var target = new TestEvidenceTarget("claim", "0201", "F3", "B-010", "Active", "approved",
            "src/analyzers/TestEvidence/.spec/README.md", 16);
        var bound = new[] { "M:A.LocalCases.Theory(System.Int32)", "M:A.LocalCases.Fact", "M:A.LocalCases.Undiscovered" };
        var digest = new TestEvidenceFingerprint().ComputeDigests([]);
        var payload = new JsonObject
        {
            ["projectPath"] = "A/A.csproj",
            ["targetFramework"] = "net10.0",
            ["configurationName"] = "Debug",
            ["assemblyPath"] = Source,
            ["configurationFingerprint"] = new string('c', 64),
            ["sourceFingerprint"] = digest,
            ["policyFingerprint"] = digest,
            ["methods"] = JsonSerializer.SerializeToNode(Methods, Wire),
            ["bindings"] = JsonSerializer.SerializeToNode(bound.Select(symbolId => new TestEvidenceBinding(symbolId, "verifies",
                new Dictionary<string, string> { ["feature"] = "0201-F3", ["claim"] = "B-010" }, true, "resolved", true, target, [],
                new TestEvidenceLocation("A/LocalCases.cs", 1, 1))).ToArray(), Wire),
            ["inputs"] = new JsonArray(),
            ["requiredCaseScope"] = requiredCaseScope,
        };
        var tree = CSharpSyntaxTree.ParseText("namespace Specht.Generated { internal static class TestEvidenceCompilationExport { "
            + "internal const string Json = " + SymbolDisplay.FormatLiteral(payload.ToJsonString(), true) + "; } }");
        var compilation = CSharpCompilation.Create("A", [tree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var assembly = new MemoryStream();
        compilation.Emit(assembly).Success.Should().BeTrue();
        FileSystem.AddFile(Source, new MockFileData(assembly.ToArray()));
        FileSystem.AddFile("A/bin/specht.test-evidence.config.json", new MockFileData(
            new JsonObject { ["outputPath"] = Output, ["assemblyPath"] = Source }.ToJsonString()));
    }

    public MockFileSystem FileSystem { get; } = new();

    public string Source => "A/bin/A.dll";

    public string Output => "A/obj/specht-test-evidence/A-net10.0.json";

    public SyntheticLoggerEvents Attach()
    {
        var events = new SyntheticLoggerEvents();
        new TestEvidenceVsTestLogger(FileSystem).Initialize(events, new Dictionary<string, string?> { ["TestRunDirectory"] = "A/TestResults" });
        return events;
    }

    public TestCase Case(string id, string fullyQualifiedName, string? managedType, string? managedMethod, params (string Name, string Value)[] traits)
    {
        var testCase = new TestCase(fullyQualifiedName, new Uri("executor://xunit/VsTestRunner3/netcore/"), Source)
        {
            Id = Guid.Parse(id),
            DisplayName = fullyQualifiedName + "(" + id[..4] + ")",
        };
        if (managedType is not null)
        {
            testCase.SetPropertyValue(ManagedType, managedType);
        }

        if (managedMethod is not null)
        {
            testCase.SetPropertyValue(ManagedMethod, managedMethod);
        }

        foreach (var (name, value) in traits)
        {
            testCase.Traits.Add(name, value);
        }

        return testCase;
    }

    public JsonNode Artifact()
    {
        FileSystem.File.Exists(Output).Should().BeTrue("the logger writes the artifact at the configured output path");
        return JsonNode.Parse(FileSystem.File.ReadAllText(Output))!;
    }

    private static readonly JsonSerializerOptions Wire = new(JsonSerializerDefaults.Web);

    private static readonly TestProperty ManagedType = TestProperty.Register("TestCase.ManagedType", "ManagedType", typeof(string),
        typeof(TestCase));

    private static readonly TestProperty ManagedMethod = TestProperty.Register("TestCase.ManagedMethod", "ManagedMethod", typeof(string),
        typeof(TestCase));

    private static readonly TestEvidenceMethod[] Methods =
    [
        new("M:A.LocalCases.Theory(System.Int32)", "A.LocalCases", "Theory", 0, ["global::System.Int32"],
            new TestEvidenceLocation("A/LocalCases.cs", 10, 5)),
        new("M:A.LocalCases.Fact", "A.LocalCases", "Fact", 0, [], new TestEvidenceLocation("A/LocalCases.cs", 20, 5)),
        new("M:A.LocalCases.Run", "A.LocalCases", "Run", 0, [], new TestEvidenceLocation("A/LocalCases.cs", 30, 5)),
        new("M:A.LocalCases.Run(System.Int32)", "A.LocalCases", "Run", 0, ["global::System.Int32"],
            new TestEvidenceLocation("A/LocalCases.cs", 40, 5)),
        new("M:A.LocalCases.Undiscovered", "A.LocalCases", "Undiscovered", 0, [], new TestEvidenceLocation("A/LocalCases.cs", 50, 5)),
        new("M:A.Shared.SharedClaimsFeature.TwoRows(System.String,System.String[])", "A.Shared.SharedClaimsFeature", "TwoRows", 0,
            ["global::System.String", "global::System.String[]"], null),
    ];
}
