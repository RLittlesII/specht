using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>Pins manifest failure ordering through both schema paths (0001-F1 C-9; 0001-F5 C-5).</summary>
[Trait("Tier", "Unit")]
public sealed class ManifestCompatibilityUnitTests
{
    /// <summary>Gets conflicting faults and the rejection that must precede schema reads.</summary>
    public static TheoryData<bool, string, string> Faults => new()
    {
        { false, """{"future":true,"schemaVersion":false}""", "the engine does not know the key 'future'." },
        { true, """{"future":true,"schemaVersion":false}""", "the engine does not know the key 'future'." },
        { false, """{"schemaVersion":"7.0.0","markers":{"missing":""}}""", "the marker 'missing' in markers is empty." },
        { true, """{"schemaVersion":"7.0.0","markers":{"missing":""}}""", "the marker 'missing' in markers is empty." },
        { false, """{"rules":{"SPEC999":"loud"}}""", "the rule 'SPEC999' in rules is set to 'loud', which is not 'error', 'warning' or 'off'." },
        { true, """{"rules":{"SPEC999":"loud"}}""", "the rule 'SPEC999' in rules is set to 'loud', which is not 'error', 'warning' or 'off'." },
    };

    [Theory]
    [MemberData(nameof(Faults))]
    public void AManifestWithFaultsInTwoValidationPhases_WhenLoaded_ShouldPreserveTheFirstRejectionBeforeSchemaReads(
        bool onDisk, string manifest, string fault)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpecTree.ManifestPath)] = new(manifest),
        });

        // When
        var load = () => onDisk
            ? EngineServices.LoadOnDiskSchemas(fileSystem, "repo")
            : EngineServices.LoadSchemas(fileSystem, "repo", EngineServices.Versions());

        // Then
        load.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Be($".specht/manifest.json: {fault}");
    }
}
