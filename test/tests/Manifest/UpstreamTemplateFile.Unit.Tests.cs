using System.Text;
using AwesomeAssertions;
using Specht.Manifest;

namespace Specht.Tests.Manifest;

/// <summary>
/// Whether bytes read from a local copy of a template or served by the source are the ones an upstream schema source
/// records for it (<c>0001-F7</c> B-130, B-132, B-136, C-9): the SHA-256 of the bytes as they are, without decoding,
/// written <c>sha256:</c> and lowercase hexadecimal as the manifest records it, is the recorded hash. The bytes are built in
/// the test; nothing is read or fetched.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class UpstreamTemplateFileUnitTests
{
    [Fact]
    public void TheTemplateBytesWhoseHashIsRecorded_WhenMatched_ShouldMatch()
    {
        // Given
        var recorded = Encoding.UTF8.GetBytes("# The feature template that was fetched\n");
        UpstreamTemplateFile file = new UpstreamTemplateFileFixture().Fetched(recorded);

        // When
        var matches = file.Matches([.. recorded]);

        // Then
        matches.Should().BeTrue();
    }

    [Theory]
    [ClassData(typeof(UpstreamTemplateFileMismatches))]
    public void TemplateBytesOtherThanTheRecordedOnes_WhenMatched_ShouldNotMatch(string because, byte[] recorded, byte[] bytes)
    {
        // Given
        UpstreamTemplateFile file = new UpstreamTemplateFileFixture().Fetched(recorded);

        // When
        var matches = file.Matches(bytes);

        // Then
        matches.Should().BeFalse(because);
    }
}
