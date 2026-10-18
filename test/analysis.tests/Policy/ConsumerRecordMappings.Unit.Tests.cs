using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.Bindings;
using Specht.Analysis.Policy;

namespace Specht.Analysis.Tests.Policy;

[Trait("Tier", "Unit")]
public sealed class ConsumerRecordMappingsUnitTests
{
    [Fact]
    public void ConsumerFrontmatterKeys_WhenExplicitlyMapped_ShouldUseTheSuppliedIdentityAndLifecycle()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        inputs[0] = inputs[0] with { Text = inputs[0].Text!.Replace("\"records\":{", """
            "records":{"featureIdentityKey":"capability","featureLifecycleKey":"maturity",
            """) };
        foreach (var index in inputs.Select(static (input, index) => (input, index))
            .Where(static item => item.input.Kind is "feature" or "schema:feature").Select(static item => item.index).ToArray())
        {
            inputs[index] = inputs[index] with
            {
                Text = inputs[index].Text!.Replace("id:", "capability:").Replace("\"id\"", "\"capability\"")
                    .Replace("spec_status", "maturity"),
            };
        }

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().BeTrue();
        new BindingResolver(snapshot.Catalog!, snapshot.Policy!.Bindings).Resolve(new ClaimReference(null, "cap:read", "need/7"))
            .IsEligible.Should().BeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FrontmatterAdrProfile_WhenRead_ShouldUseItsDeclaredIdentityLifecycleAndOwner(bool featureOwned)
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs[0].Text!)!;
        configuration["codeAnalysis"]!["records"]!["adrProfiles"] = JsonNode.Parse("""
            [{"glob":"**/adr/*.md","owner":"repository","identity":{"frontmatterKey":"decision"},
              "lifecycle":{"frontmatterKey":"maturity"}}]
            """);
        configuration["codeAnalysis"]!["records"]!["adrProfiles"]![0]!["owner"] = featureOwned ? "containingFeature" : "repository";
        inputs[0] = inputs[0] with { Text = configuration.ToJsonString() };
        var index = inputs.FindIndex(static input => input.Kind == "adr");
        inputs[index] = inputs[index] with { RelativePath = "src/read/.spec/adr/decision.md", Text = """
            ---
            decision: RULE-ALPHA
            maturity: Accepted
            ---
            # A consumer decision
            """ };
        var schema = inputs.FindIndex(static input => input.Kind == "schema:adr");
        inputs[schema] = inputs[schema] with { Text = "{\"type\":\"object\",\"required\":[\"decision\",\"maturity\"]}" };

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().BeTrue();
        var adr = snapshot.Catalog!.Adrs.Should().ContainSingle().Subject;
        adr.Id.Should().Be("RULE-ALPHA");
        adr.State.Should().Be("accepted");
        adr.Owner.Should().Be(featureOwned ? new FeatureIdentity(null, "cap:read") : null);
    }
}
