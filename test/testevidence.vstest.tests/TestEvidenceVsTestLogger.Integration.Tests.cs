using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Specht.TestEvidence.VsTest.Tests;

/// <summary>The logger as VSTest constructs it, over the real file system (0201-F3 B-010; ADR-0001).</summary>
[Trait("Tier", "Integration")]
public sealed class TestEvidenceVsTestLoggerIntegrationTests : IDisposable
{
    [Fact]
    public void TheLoggerVsTestConstructs_WhenDiscoveryCompletes_ShouldWriteTheArtifactBesideTheSourceConfiguration()
    {
        // Given
        var source = new VsTestSource();
        var bin = Directory.CreateDirectory(Path.Combine(_root, "A", "bin")).FullName;
        var assembly = Path.Combine(bin, "A.dll");
        var output = Path.Combine(_root, "A", "obj", "A-net10.0.json");
        File.WriteAllBytes(assembly, source.FileSystem.File.ReadAllBytes(source.Source));
        File.WriteAllText(Path.Combine(bin, "specht.test-evidence.config.json"),
            new JsonObject { ["outputPath"] = output, ["assemblyPath"] = "A/bin/A.dll" }.ToJsonString());
        var events = new SyntheticLoggerEvents();
        new TestEvidenceVsTestLogger().Initialize(events, new Dictionary<string, string?> { ["TestRunDirectory"] = _root });
        var testCase = new TestCase("A.LocalCases.Fact", new Uri("executor://xunit/VsTestRunner3/netcore/"), assembly)
        {
            Id = Guid.Parse("11111111-1111-4111-8111-111111111111"),
        };

        // When
        events.Discover(assembly, testCase);

        // Then
        var artifact = JsonNode.Parse(File.ReadAllText(output))!;
        artifact["discovery"]!["cases"]!.AsArray().Should().ContainSingle()
            .Which!["methodSymbolId"]!.GetValue<string>().Should().Be("M:A.LocalCases.Fact");
        artifact["assembly"]!["path"]!.GetValue<string>().Should().Be("A/bin/A.dll");
    }

    public void Dispose() => Directory.Delete(_root, true);

    private readonly string _root = Directory.CreateTempSubdirectory("specht-vstest-logger-").FullName;
}
