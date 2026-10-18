using AwesomeAssertions;
using Specht.Analysis.Documentation;

namespace Specht.Analysis.Tests.Documentation;

[Trait("Tier", "Unit")]
public sealed class DocumentationDiagnosticsUnitTests
{
    [Theory]
    [ClassData(typeof(DiagnosticHistoryCases))]
    public void PublishedHistory_WhenValidated_ShouldRejectReassignmentRemovalAndRevival(DiagnosticPublication previous, bool rejected)
    {
        // Given
        DocumentationDiagnostics? registry = null;
        // When
        Action publish = () => registry = new DocumentationDiagnostics([previous]);

        // Then
        if (rejected)
        {
            publish.Should().Throw<ArgumentException>();
        }
        else
        {
            publish.Should().NotThrow();
            registry!.Publications.Should().Contain(entry => entry.Id == previous.Id && entry.Finding == previous.Finding && !entry.Retired);
        }
    }

    [Fact]
    public void AgreedRegistry_WhenLoaded_ShouldKeepEveryAssignedDiagnosticAndItsSeverity()
    {
        // Given
        var registry = new DocumentationDiagnostics();

        // When
        var descriptors = registry.Descriptors;

        // Then
        descriptors.Select(static descriptor => descriptor.Id)
            .Should().Equal("SPCT0002", "SPCT0003", "SPCT0004", "SPCT0005", "SPCT0006", "SPCT0007", "SPCT0008", "SPCT0009");
        descriptors.Should().OnlyContain(static descriptor => descriptor.Id.StartsWith("SPCT", StringComparison.Ordinal));
        descriptors.Select(static descriptor => descriptor.DefaultSeverity)
            .Should().Equal(Enumerable.Repeat(Microsoft.CodeAnalysis.DiagnosticSeverity.Warning, 7)
                .Append(Microsoft.CodeAnalysis.DiagnosticSeverity.Info));
    }
}
