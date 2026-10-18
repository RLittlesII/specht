using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.Bindings;
using Specht.Analysis.Policy;

namespace Specht.Analysis.Tests.Policy;

[Trait("Tier", "Unit")]
public sealed class AnalysisPolicyUnitTests
{
    [Theory]
    [ClassData(typeof(AuthoringKindSelections))]
    public void SelectedRoleAndDeclarationKind_WhenApplied_ShouldRequireOnlyTheConfiguredRelationships(
        string role, string configuredKind, string actualKind, string[] expectedRoles)
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs[0].Text!)!;
        var scope = configuration["codeAnalysis"]!["enforcement"]![0]!;
        scope["role"] = role;
        scope["declarationKinds"] = new JsonArray(configuredKind);
        inputs[0] = inputs[0] with { Text = configuration.ToJsonString() };
        AnalysisDeclaration declaration = new AnalysisDeclarationFixture().WithKind(actualKind);

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().BeTrue();
        snapshot.Policy!.Select(declaration).RequiredRoles.Should().Equal(expectedRoles);
    }

    [Fact]
    public void ReasonedDeclarationExemption_WhenApplied_ShouldPreserveValidationAndExposeItsReason()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs[0].Text!)!;
        configuration["codeAnalysis"]!["exemptions"] = JsonNode.Parse("""
            [{"target":{"kind":"declaration","project":"test/tests.csproj","symbolId":"M:Run"},
              "links":["test"],"reason":"Delegated to an external harness"}]
            """);
        inputs[0] = inputs[0] with { Text = configuration.ToJsonString() };
        AnalysisDeclaration declaration = new AnalysisDeclarationFixture().WithRecognizedTest(true);

        // When
        var result = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken).Policy!.Select(declaration);

        // Then
        result.ValidateExplicit.Should().BeTrue();
        result.RequiredRoles.Should().BeEmpty();
        result.ExemptionReasons.Should().Equal("Delegated to an external harness");
    }

    [Fact]
    public void AdditionalRequiredLinks_WhenATestLinkIsExempt_ShouldRetainTheOtherRequirements()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs[0].Text!)!;
        configuration["codeAnalysis"]!["requiredLinks"] = JsonNode.Parse("{\"implementation\":true,\"scenario\":true}");
        configuration["codeAnalysis"]!["exemptions"] = JsonNode.Parse("""
            [{"target":{"feature":"cap:read","claim":"need/7"},"links":["test"],"reason":"Externally verified"}]
            """);
        inputs[0] = inputs[0] with { Text = configuration.ToJsonString() };

        // When
        var policy = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken).Policy!;

        // Then
        policy.RequiredLinks(new ClaimReference(null, "cap:read", "need/7")).Should().Equal("implementation", "scenario");
        policy.RequiredLinks(new ClaimReference(null, "cap:other", "need/7")).Should().BeEmpty();
    }
}
