using AwesomeAssertions;
using Specht.Analysis.TestEvidence;

namespace Specht.Analysis.Tests.TestEvidence;

[Trait("Tier", "Unit")]
public sealed class TestEvidenceFingerprintUnitTests
{
    [Fact]
    public void SameInputsInDifferentOrder_WhenHashed_ShouldProduceSameFingerprint()
    {
        // Given
        var source = new TestEvidenceFingerprintSource("src/Run.cs", "source", [1, 2, 3]);
        var manifest = new TestEvidenceFingerprintSource(".specht/manifest.json", "manifest", [4, 5]);
        var fingerprint = new TestEvidenceFingerprint();

        // When
        var first = fingerprint.Compute([source, manifest]);
        var second = fingerprint.Compute([manifest, source]);

        // Then
        first.Should().Be(second);
        first.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void ChangedLogicalInput_WhenHashed_ShouldProduceDifferentFingerprint()
    {
        // Given
        var fingerprint = new TestEvidenceFingerprint();

        // When
        var first = fingerprint.Compute([new TestEvidenceFingerprintSource("src/Run.cs", "source", [1, 2, 3])]);
        var changed = fingerprint.Compute([new TestEvidenceFingerprintSource("src/Other.cs", "source", [1, 2, 3])]);

        // Then
        changed.Should().NotBe(first);
    }
}
