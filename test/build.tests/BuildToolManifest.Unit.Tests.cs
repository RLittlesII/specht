using System.Text.Json.Nodes;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Unit")]
public sealed class BuildToolManifestUnitTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ToolPins_WhenDerivingBuildPrerequisites_ShouldPreserveDevelopmentSettingsWithoutThePublishedChecker(bool checkerPresent)
    {
        // Given
        var development = JsonNode.Parse("""
            {"fixture.version":{"version":"2.3.4","commands":["fixture"],"rollForward":false}}
            """)!;
        var input = new JsonObject { ["version"] = 1, ["isRoot"] = true, ["tools"] = development.DeepClone() };
        if (checkerPresent)
        {
            input["tools"]!["specht.tool"] = JsonNode.Parse("""{"version":"0.1.90","commands":["specht"]}""");
        }

        var manifest = new BuildToolManifest();

        // When
        var result = JsonNode.Parse(manifest.ForBuild(input.ToJsonString()))!;

        // Then
        JsonNode.DeepEquals(result["tools"], development).Should().BeTrue();
        result["version"]!.GetValue<int>().Should().Be(1);
        result["isRoot"]!.GetValue<bool>().Should().BeTrue();
    }
}
