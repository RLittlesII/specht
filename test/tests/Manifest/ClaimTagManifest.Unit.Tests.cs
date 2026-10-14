using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Manifest;

/// <summary>Tag-form validation and the old-pin key boundary (0001-F5 B-038, B-049).</summary>
[Trait("Tier", "Unit")]
public sealed class ClaimTagManifestUnitTests
{
    [Theory]
    [InlineData("0.2.0", "@old:{claim}", false)]
    [InlineData("0.3.0", "@NEW:{claim}", true)]
    public void OmittedTagSettings_WhenParsed_ShouldUseTheExactPinsDefaults(string pin, string form, bool ignoreCase)
    {
        // Given
        var original = EngineServices.Versions().Find(new SemanticVersion(0, 1, 0))!;
        var older = JsonNode.Parse(original.Manifest)!.AsObject();
        older["claimTagForm"] = "@old:{claim}";
        older["claimTagIgnoreCase"] = false;
        var newer = older.DeepClone().AsObject();
        newer["claimTagForm"] = "@NEW:{claim}";
        newer["claimTagIgnoreCase"] = true;
        var parser = new SpecManifestParser(new SchemaVersions([
            original,
            new SchemaVersionFixture().WithNumber(new SemanticVersion(0, 2, 0)).WithManifest(older.ToJsonString()),
            new SchemaVersionFixture().WithNumber(new SemanticVersion(0, 3, 0)).WithManifest(newer.ToJsonString()),
        ]));

        // When
        var structure = parser.Parse(new JsonObject { ["schemaVersion"] = pin }.ToJsonString());
        var explicitSettings = parser.Parse(new JsonObject
        {
            ["schemaVersion"] = pin,
            ["claimTagForm"] = "@consumer:{claim}",
            ["claimTagIgnoreCase"] = !ignoreCase,
        }.ToJsonString());

        // Then
        structure.ClaimTagForm.Should().Be(form);
        structure.ClaimTagIgnoreCase.Should().Be(ignoreCase);
        explicitSettings.ClaimTagForm.Should().Be("@consumer:{claim}");
        explicitSettings.ClaimTagIgnoreCase.Should().Be(!ignoreCase);
    }

    [Theory]
    [InlineData("@req:")]
    [InlineData("@{claim}:{claim}")]
    [InlineData("")]
    public void AFormWithoutExactlyOneClaimPlaceholder_WhenParsed_ShouldBeRejectedNamingTheForm(string form)
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0", ["claimTagForm"] = form };

        // When
        var parse = () => parser.Parse(manifest.ToJsonString());

        // Then
        parse.Should().Throw<SpechtManifestException>().Which.Message.Should().Contain("claimTagForm").And.Contain($"'{form}'");
    }

    [Theory]
    [InlineData("0.1.0", "claimTagForm", "\"@{claim}\"")]
    [InlineData("0.1.1", "claimTagForm", "\"@{claim}\"")]
    [InlineData("0.1.0", "claimTagIgnoreCase", "true")]
    [InlineData("0.1.1", "claimTagIgnoreCase", "true")]
    public void ANewTagKeyUnderAnOlderPin_WhenParsed_ShouldRemainUnknown(string pin, string key, string json)
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject { ["schemaVersion"] = pin, [key] = JsonNode.Parse(json) };

        // When
        var parse = () => parser.Parse(manifest.ToJsonString());

        // Then
        parse.Should().Throw<SpechtManifestException>().Which.Message.Should().Contain($"does not know the key '{key}'");
    }
}
