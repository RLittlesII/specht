using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Specht.Analysis.Bindings.Gherkin;
using Specht.Analysis.Documentation;

namespace Specht.Analysis.Tests.Gherkin;

[Trait("Tier", "Unit")]
public sealed class GherkinDiagnosticsUnitTests
{
    [Fact]
    public void AgreedRegistry_WhenLoaded_ShouldHoldSpct0010ToSpct0012AsWarnings()
    {
        // Given
        var registry = new GherkinDiagnostics();

        // When
        var descriptors = registry.Descriptors;

        // Then
        descriptors.Select(static descriptor => descriptor.Id).Should().Equal("SPCT0010", "SPCT0011", "SPCT0012");
        descriptors.Select(static descriptor => descriptor.DefaultSeverity).Should().OnlyContain(static severity => severity == DiagnosticSeverity.Warning);
        registry.Publications.Select(static publication => publication.Id).Should().Equal("SPCT0010", "SPCT0011", "SPCT0012");
    }

    [Fact]
    public void BothRegistries_WhenUnioned_ShouldAssignEveryIdOnceAndLeaveSpct0001ToTheInputAnalyzer()
    {
        // Given
        var documentation = new DocumentationDiagnostics().Descriptors;
        var native = new GherkinDiagnostics().Descriptors;

        // When
        var ids = documentation.Concat(native).Select(static descriptor => descriptor.Id).ToArray();

        // Then
        ids.Should().OnlyHaveUniqueItems();
        ids.Should().Contain(["SPCT0009", "SPCT0010", "SPCT0011", "SPCT0012"]);
        ids.Should().NotContain("SPCT0001");
    }

    [Fact]
    public void RegistryPublications_WhenLoadedAsHistory_ShouldBeAccepted()
    {
        // Given
        var published = new GherkinDiagnostics().Publications;

        // When
        Action load = () => _ = new GherkinDiagnostics(published);

        // Then
        published.Should().NotBeEmpty();
        load.Should().NotThrow();
    }

    [Theory]
    [InlineData("reassigned")]
    [InlineData("removed")]
    [InlineData("revived")]
    public void PublishedHistory_WhenReassignedRemovedOrRevived_ShouldBeRejected(string change)
    {
        // Given
        var first = new GherkinDiagnostics().Publications.FirstOrDefault();
        first.Should().NotBeNull();
        var previous = change switch
        {
            "reassigned" => first! with { Finding = first.Finding + "Changed" },
            "removed" => new DiagnosticPublication("SPCT0099", "Removed"),
            _ => first! with { Retired = true },
        };

        // When
        Action load = () => _ = new GherkinDiagnostics([previous]);

        // Then
        load.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ClaimAnalyzer_WhenSupportedDiagnosticsAreListed_ShouldDeclareExactlyTheRegistry()
    {
        // Given
        var registry = new GherkinDiagnostics().Descriptors.Select(static descriptor => descriptor.Id);

        // When
        var supported = new GherkinClaimAnalyzer().SupportedDiagnostics.Select(static descriptor => descriptor.Id);

        // Then
        supported.Should().Equal(registry);
        supported.Should().NotBeEmpty();
    }
}
