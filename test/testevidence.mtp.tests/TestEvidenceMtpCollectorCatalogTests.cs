using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.TestEvidence.Mtp;

namespace Specht.TestEvidence.Mtp.Tests;

/// <summary>The collector carries the compiler payload's native claim catalogue into the artifact (0259 B-030).</summary>
[Trait("Tier", "Unit")]
public sealed class TestEvidenceMtpCollectorCatalogTests : IDisposable
{
    /// <verifies epic="0201" feature="F7" claim="B-030" />
    [Fact]
    public void PayloadWithACatalog_WhenWritten_ShouldCarryTheCatalogInTheArtifact()
    {
        // Given
        var payload = new JsonObject
        {
            ["projectPath"] = "A/A.csproj",
            ["targetFramework"] = "net10.0",
            ["configurationName"] = "Debug",
            ["assemblyPath"] = "A/bin/A.dll",
            ["configurationFingerprint"] = new string('c', 64),
            ["sourceFingerprint"] = new string('a', 64),
            ["policyFingerprint"] = new string('b', 64),
            ["methods"] = new JsonArray(),
            ["bindings"] = new JsonArray(),
            ["inputs"] = new JsonArray(),
            ["catalog"] = new JsonObject
            {
                ["features"] = new JsonArray(new JsonObject
                {
                    ["epic"] = "0201",
                    ["feature"] = "F7",
                    ["state"] = "approved",
                    ["path"] = "A/arithmetic.feature",
                    ["claims"] = new JsonArray(new JsonObject
                    {
                        ["id"] = "REQ-FIX-001",
                        ["state"] = "Active",
                        ["element"] = "Scenario",
                        ["path"] = "A/arithmetic.feature",
                        ["line"] = 3,
                        ["cases"] = new JsonArray(),
                    }),
                }),
            },
        };
        var output = Path.Combine(_root, "A-net10.0.json");

        // When
        new TestEvidenceMtpCollector().Write(output, typeof(TestEvidenceMtpCollectorCatalogTests).Assembly, payload.ToJsonString());

        // Then
        var catalog = JsonNode.Parse(File.ReadAllText(output))!["catalog"]!["features"]!.AsArray();
        var feature = catalog.Should().ContainSingle().Which!;
        feature["feature"]!.GetValue<string>().Should().Be("F7");
        feature["claims"]!.AsArray().Should().ContainSingle().Which!["id"]!.GetValue<string>().Should().Be("REQ-FIX-001");
    }

    /// <inheritdoc />
    public void Dispose() => Directory.Delete(_root, true);

    private readonly string _root = Directory.CreateTempSubdirectory("specht-mtp-collector-").FullName;
}
