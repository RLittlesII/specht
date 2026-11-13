using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Manifest;

/// <summary>
/// The <c>ids</c> records the shipped schema 0.4.0 manifest gives a type a manifest declares no record for
/// (<c>0001-F5</c> B-089, B-090, B-099; A-8; decision 0024; item <c>0436</c>), read from the shipped catalog. A type's
/// grammar for a Feature without a claim prefix is its record's <c>plain</c> without the closing <c>{id}</c>, matched as
/// literal text, then a whole match of the record's <c>id</c>, case-sensitive.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ShippedIdRecordsUnitTests
{
    [Theory]
    [ClassData(typeof(ShippedIdGrammarSamples))]
    public void AnIdUnderTheShippedZeroFourZeroRecordForItsType_WhenMatchedForAFeatureWithoutAPrefix_ShouldBeAdmittedExactlyWhenTheClaimSaysSo(
        string type,
        string id,
        bool admitted)
    {
        // Given
        var version = EngineServices.Versions().Find(new SemanticVersion(0, 4, 0));
        version.Should().NotBeNull("schema 0.4.0 ships the ids records");
        var record = JsonNode.Parse(version!.Manifest)!["ids"]![type]!;
        var plain = record["plain"]!.GetValue<string>();

        // When
        var match = Regex.IsMatch(id, "^" + Regex.Escape(plain[..^"{id}".Length]) + "(?:" + record["id"]!.GetValue<string>() + ")$");

        // Then
        match.Should().Be(admitted);
    }
}
