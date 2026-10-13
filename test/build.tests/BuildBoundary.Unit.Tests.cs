using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Unit")]
public sealed class BuildBoundaryUnitTests
{
    [Fact]
    public void TheBuildAssembly_WhenInspected_ShouldNotReferenceApplicationAssemblies()
    {
        // Given
        var assembly = typeof(SchemaVersionLock).Assembly;

        // When
        var references = assembly.GetReferencedAssemblies().Select(static reference => reference.Name);

        // Then
        references.Should().NotContain(static name =>
            string.Equals(name, "specht", StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, "specht.tool", StringComparison.OrdinalIgnoreCase));
    }
}
