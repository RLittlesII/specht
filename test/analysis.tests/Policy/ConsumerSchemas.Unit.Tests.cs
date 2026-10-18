using AwesomeAssertions;
using Specht.Analysis.Policy;

namespace Specht.Analysis.Tests.Policy;

[Trait("Tier", "Unit")]
public sealed class ConsumerSchemasUnitTests
{
    [Fact]
    public void SuppliedSchemaReference_WhenValidated_ShouldUseTheSuppliedDependencyWithoutFetching()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var index = inputs.FindIndex(static input => input.Kind == "schema:feature");
        inputs[index] = inputs[index] with { Text = """
            {"$id":"https://consumer.invalid/feature","$ref":"https://consumer.invalid/shared"}
            """ };
        inputs.Add(new AnalysisInputFixture().WithKind("schema:shared").WithRelativePath("contracts/shared.json").WithText("""
            {"$id":"https://consumer.invalid/shared","type":"object","required":["id","spec_status"]}
            """));

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().BeTrue();
        snapshot.Catalog!.Features.Should().ContainSingle();
    }

    [Fact]
    public void ConsumerSchemaConstraint_WhenNotSatisfied_ShouldMakeAnalysisUnavailable()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var index = inputs.FindIndex(static input => input.Kind == "schema:feature");
        inputs[index] = inputs[index] with { Text = "{\"required\":[\"consumerRequiredField\"]}" };

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().BeFalse();
        snapshot.Failures.Should().ContainSingle().Which.RelativePath.Should().Be("src/read/.spec/README.md");
    }
}
