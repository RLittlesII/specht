using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Tool.Features.Evidence;

namespace Specht.Tests.Evidence;

/// <summary>The normalized consumer policy decisions used by 0201-F5 B-008 to B-010.</summary>
[Trait("Tier", "Unit")]
public sealed class EvidencePolicyUnitTests
{
    [Fact]
    public void SelectedPolicy_WhenProjected_ShouldRequireTestByDefaultAndOptionalKindsOnlyWhenEnabled()
    {
        // Given
        var configuration = Configuration();
        configuration["codeAnalysis"]!["requiredLinks"]!["scenario"] = true;

        // When
        var policy = EvidencePolicy.Read(configuration);

        // Then
        policy.RequiredKinds.Should().ContainInOrder("test", "scenario");
        policy.RequiredKinds.Should().NotContain("implementation");
    }

    [Fact]
    public void ClaimExemption_WhenProjected_ShouldKeepItsExactReasonAndLinkKind()
    {
        // Given
        var configuration = Configuration();
        configuration["codeAnalysis"]!["exemptions"]!.AsArray().Add(new JsonObject
        {
            ["target"] = new JsonObject { ["feature"] = "cap:read", ["claim"] = "need/7" },
            ["links"] = new JsonArray("test"),
            ["reason"] = "A reviewed exception.",
        });

        // When
        var policy = EvidencePolicy.Read(configuration);

        // Then
        policy.Exemption(null, "cap:read", "need/7", "test").Should().Be("A reviewed exception.");
        policy.Exemption(null, "cap:read", "need/7", "scenario").Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ExemptionWithoutReason_WhenProjected_ShouldBeRejected(string reason)
    {
        // Given
        var configuration = Configuration();
        configuration["codeAnalysis"]!["exemptions"]!.AsArray().Add(new JsonObject
        {
            ["target"] = new JsonObject { ["feature"] = "cap:read", ["claim"] = "need/7" },
            ["links"] = new JsonArray("test"),
            ["reason"] = reason,
        });

        // When
        var read = () => EvidencePolicy.Read(configuration);

        // Then
        read.Should().Throw<EvidenceConfigurationException>();
    }

    [Fact]
    public void EscapingProjectPath_WhenProjected_ShouldBeRejected()
    {
        // Given
        var configuration = Configuration();
        configuration["codeAnalysis"]!["projects"]![0]!["path"] = "../escape.csproj";

        // When
        var read = () => EvidencePolicy.Read(configuration);

        // Then
        read.Should().Throw<EvidenceConfigurationException>();
    }

    private static JsonObject Configuration() => JsonNode.Parse("""
        {
          "revision":"0.1.0",
          "idPatterns":{"feature":"^cap:[a-z]+$","claim":"^need/[0-9]+$"},
          "claimHeading":"3. Acceptance Criteria",
          "codeAnalysis":{
            "mappings":{"/schemaVersion":"/revision","/identifiers":"/idPatterns",
              "/claimsSection":"/claimHeading","/policy":"/codeAnalysis"},
            "projects":[{"path":"test/tests.csproj","targetFrameworks":["net10.0"]}],
            "features":[{"feature":"cap:read"}],
            "requiredLinks":{"test":true},
            "exemptions":[]
          }
        }
        """)!.AsObject();
}
