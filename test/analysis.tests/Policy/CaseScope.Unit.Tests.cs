using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.Policy;

namespace Specht.Analysis.Tests.Policy;

/// <summary>A project's required-case scope, read from its codeAnalysis entry and evaluated over case categories (0201-F6 B-026).</summary>
[Trait("Tier", "Unit")]
public sealed class CaseScopeUnitTests
{
    [Theory]
    [ClassData(typeof(CaseScopeRows))]
    public void ARequiredCaseScope_WhenItsProjectIsRead_ShouldOwnACaseExactlyWhenTheTagExpressionHolds(string expression, string[] categories,
        bool owns)
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs[0].Text!)!;
        configuration["codeAnalysis"]!["projects"]![0]!["requiredCaseScope"] = expression;
        inputs[0] = inputs[0] with { Text = configuration.ToJsonString() };

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Failures.Should().BeEmpty();
        snapshot.Policy!.Projects.Single().Scope.Owns(categories).Should().Be(owns);
    }

    [Fact]
    public void AProjectWithoutARequiredCaseScope_WhenRead_ShouldOwnEveryCase()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        var scope = snapshot.Policy!.Projects.Single().Scope;
        scope.Owns([]).Should().BeTrue();
        scope.Owns(["visual"]).Should().BeTrue();
    }

    [Theory]
    [ClassData(typeof(InvalidCaseScopes))]
    public void AnEmptyOrMalformedRequiredCaseScope_WhenRead_ShouldFailAtTheConfigurationWithTheTagExpressionMessage(string expression)
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs[0].Text!)!;
        configuration["codeAnalysis"]!["projects"]![0]!["requiredCaseScope"] = expression;
        inputs[0] = inputs[0] with { Text = configuration.ToJsonString() };

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().BeFalse();
        var failure = snapshot.Failures.Should().ContainSingle().Subject;
        failure.RelativePath.Should().Be("config/consumer.json");
        failure.Message.Should().Be("Project required-case scope is not a valid tag expression.");
    }
}
