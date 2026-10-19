using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Specht.Analysis.Bindings;
using Specht.Analysis.Documentation;
using Specht.Analysis.TestEvidence;

namespace Specht.Analysis.Tests.TestEvidence;

[Trait("Tier", "Unit")]
public sealed class TestEvidenceBindingExporterUnitTests
{
    [Fact]
    public void ResolvedF2Relationship_WhenExported_ShouldPreserveRawTargetAndEligibility()
    {
        // Given
        var target = new BindingTarget(BindingTargetKind.Claim, "0201", "F3", "B-001", "Active", "approved",
            "src/analyzers/TestEvidence/.spec/README.md", 54);
        var binding = new DocumentationBinding("M:Example.Run", "verifies",
            new Dictionary<string, string> { ["feature"] = "0201-F3", ["claim"] = "B-001" },
            Location.Create("test/Example.cs", new TextSpan(0, 1),
                new LinePositionSpan(new LinePosition(3, 4), new LinePosition(3, 5))),
            true, new BindingResult(BindingResolution.Resolved, target, true, [target]));
        var analysis = new DocumentationAnalysis([], [binding], []);

        // When
        var result = new TestEvidenceBindingExporter().Export(analysis, "test/Example.cs");

        // Then
        result.Should().ContainSingle();
        result[0].Attributes["claim"].Should().Be("B-001");
        result[0].Status.Should().Be("resolved");
        result[0].IsEligible.Should().BeTrue();
        result[0].Target!.Id.Should().Be("B-001");
        result[0].Location.Should().Be(new TestEvidenceLocation("test/Example.cs", 4, 5));
    }
}
