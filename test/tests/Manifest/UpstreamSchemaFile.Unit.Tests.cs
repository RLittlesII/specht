using System.Text;
using AwesomeAssertions;
using Specht.Manifest;

namespace Specht.Tests.Manifest;

/// <summary>
/// Whether bytes read from a local copy or served by the source are the ones an upstream schema source records
/// (<c>0001-F7</c> B-033, C-9): the SHA-256 of the bytes as they are, without decoding, written <c>sha256:</c> and lowercase
/// hexadecimal as the manifest records it, is the recorded hash. The bytes are built in the test; nothing is read or fetched.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class UpstreamSchemaFileUnitTests
{
    [Fact]
    public void TheBytesWhoseHashIsRecorded_WhenMatched_ShouldMatch()
    {
        // Given
        var recorded = Encoding.UTF8.GetBytes("""{ "title": "the item schema that was fetched" }""");
        UpstreamSchemaFile file = new UpstreamSchemaFileFixture().Fetched(recorded);

        // When
        var matches = file.Matches([.. recorded]);

        // Then
        matches.Should().BeTrue();
    }

    [Theory]
    [ClassData(typeof(UpstreamSchemaFileMismatches))]
    public void BytesOtherThanTheRecordedOnes_WhenMatched_ShouldNotMatch(string because, byte[] recorded, byte[] bytes)
    {
        // Given
        UpstreamSchemaFile file = new UpstreamSchemaFileFixture().Fetched(recorded);

        // When
        var matches = file.Matches(bytes);

        // Then
        matches.Should().BeFalse(because);
    }
}
