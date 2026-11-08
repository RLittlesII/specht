using AwesomeAssertions;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>
/// The Feature key role a claim prefix is read by (<c>0001-F5</c> B-004, B-049; decision 0022; A-7): schema 0.3.1's
/// shipped defaults name <c>keys.feature.claimPrefix</c> as <c>claim_prefix</c>, and the published 0.3.0 defaults name
/// no such role.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtManifestParserClaimPrefixUnitTests
{
    [Theory]
    [InlineData("0.3.0", null)]
    [InlineData("0.3.1", "claim_prefix")]
    public void AManifestLeavingOutItsKeys_WhenParsedAsADocument_ShouldNameTheClaimPrefixRoleFromZeroThreeOne(string pin, string? key)
    {
        // Given
        var text = $$"""{ "schemaVersion": "{{pin}}" }""";

        // When
        var document = EngineServices.Parser().ParseDocument(text).Document;

        // Then
        ((string?) document["keys"]!["feature"]!["claimPrefix"]).Should().Be(key);
    }
}
