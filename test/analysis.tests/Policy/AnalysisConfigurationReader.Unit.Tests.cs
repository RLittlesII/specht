using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.Policy;

namespace Specht.Analysis.Tests.Policy;

[Trait("Tier", "Unit")]
public sealed class AnalysisConfigurationReaderUnitTests
{
    [Fact]
    public void ExplicitMappings_WhenProjectingDifferentConsumerKeys_ShouldMapArraysAndLeaveTheOriginalUntouched()
    {
        // Given
        const string original = """
            {"rev":"private-r7","build":{"targets":[{"file":"test/first.csproj"},{"file":"test/second.csproj"}]},"unrelated":true}
            """;
        var configuration = JsonNode.Parse(original)!;
        var mappings = new Dictionary<string, string>
        {
            ["/schemaVersion"] = "/rev",
            ["/policy/projects/*/path"] = "/build/targets/*/file",
        };

        // When
        var result = new AnalysisConfigurationReader().Read(configuration, mappings);

        // Then
        result["schemaVersion"]!.GetValue<string>().Should().Be("private-r7");
        result["policy"]!["projects"]!.AsArray().Select(static project => project!["path"]!.GetValue<string>())
            .Should().Equal("test/first.csproj", "test/second.csproj");
        result.ContainsKey("unrelated").Should().BeFalse();
        JsonNode.DeepEquals(configuration, JsonNode.Parse(original)).Should().BeTrue();
    }

    [Fact]
    public void LiteralArrayIndexAndEmptySelection_WhenMapped_ShouldHonorJsonPointersAndPreserveExplicitEmptyArrays()
    {
        // Given
        var configuration = JsonNode.Parse("{\"versions\":[\"consumer-r1\"],\"selected\":[]}")!;
        var mappings = new Dictionary<string, string>
        {
            ["/schemaVersion"] = "/versions/0",
            ["/policy/features/*/feature"] = "/selected/*/id",
        };

        // When
        var result = new AnalysisConfigurationReader().Read(configuration, mappings);

        // Then
        result["schemaVersion"]!.GetValue<string>().Should().Be("consumer-r1");
        result["policy"]!["features"]!.AsArray().Should().BeEmpty();
    }

    [Theory]
    [InlineData("/absent", "/value")]
    [InlineData("/items/*/value", "/value")]
    public void InvalidExplicitMapping_WhenProjected_ShouldRejectIt(string source, string target)
    {
        // Given
        var configuration = JsonNode.Parse("{\"items\":[{\"value\":1}]}")!;
        var mappings = new Dictionary<string, string> { [target] = source };

        // When
        var action = () => new AnalysisConfigurationReader().Read(configuration, mappings);

        // Then
        action.Should().Throw<ArgumentException>();
    }
}
