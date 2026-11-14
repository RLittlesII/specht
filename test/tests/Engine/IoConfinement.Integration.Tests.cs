using System.IO.Abstractions;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Report;
using Specht.Resources;
using Specht.Tests.Shared;
using Specht.Tree;

namespace Specht.Tests.Engine;

/// <summary>
/// Where the engine touches I/O (<c>0001-F1</c> C-12; ADR-0010 § "I/O is confined" and § "The guard"; item <c>0167</c>):
/// the library <c>src/specht</c> builds, read from disk, has only the four stores and <c>Specht.Composition</c>, which
/// registers it, reference <c>IFileSystem</c>, and only <c>SpechtResources</c> read an embedded resource; and the same read
/// over an assembly whose type takes a file system by constructor, and whose type reads a resource, finds each, so the
/// guard is not vacuous.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class IoConfinementIntegrationTests
{
    [Fact]
    public void TheEngineAssembly_WhenItsFileSystemReferencesAreRead_ShouldFindThemOnlyInTheFourStoresAndTheComposition()
    {
        // Given
        string[] stores =
        [
            typeof(SpechtManifestStore).FullName!,
            typeof(SpechtDiscovery).FullName!,
            typeof(SpechtTreeStore).FullName!,
            typeof(SpechtReportStore).FullName!,
        ];

        // When
        var types = EngineAssembly.FileSystemTypesIn(EngineAssembly.Built);

        // Then
        types.Where(type => !stores.Contains(type) && !type.StartsWith("Specht.Composition.", StringComparison.Ordinal))
            .Should().BeEmpty();
    }

    [Fact]
    public void TheEngineAssembly_WhenItsEmbeddedResourceReadsAreRead_ShouldFindThemOnlyInTheResources()
    {
        // Given
        var library = EngineAssembly.Built;

        // When
        var types = EngineAssembly.ResourceReadingTypesIn(library);

        // Then
        types.Where(static type => type != typeof(SpechtResources).FullName).Should().BeEmpty();
    }

    [Fact]
    public void AnAssemblyWithATypeTakingAFileSystemByConstructor_WhenItsFileSystemReferencesAreRead_ShouldFindThatType()
    {
        // Given
        var assembly = typeof(FileSystemHolder).Assembly.Location;

        // When
        var types = EngineAssembly.FileSystemTypesIn(assembly);

        // Then
        types.Should().Contain(typeof(FileSystemHolder).FullName!);
    }

    [Fact]
    public void AnAssemblyWithATypeReadingAnEmbeddedResource_WhenItsEmbeddedResourceReadsAreRead_ShouldFindThatType()
    {
        // Given
        var assembly = typeof(ResourceReader).Assembly.Location;

        // When
        var types = EngineAssembly.ResourceReadingTypesIn(assembly);

        // Then
        types.Should().Contain(typeof(ResourceReader).FullName!);
    }

    /// <summary>Reads the names of this assembly's embedded resources; it is never called.</summary>
    private static class ResourceReader
    {
        public static string[] Names() => typeof(ResourceReader).Assembly.GetManifestResourceNames();
    }

    /// <summary>Takes a file system by constructor and touches no member of it, so only its constructor's signature names it.</summary>
    private sealed class FileSystemHolder
    {
        public FileSystemHolder(IFileSystem fileSystem) => ArgumentNullException.ThrowIfNull(fileSystem);
    }
}
