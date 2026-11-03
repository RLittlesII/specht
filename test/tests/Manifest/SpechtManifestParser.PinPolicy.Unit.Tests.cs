using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Manifest;

/// <summary>
/// The <c>pinPolicy</c> key beneath <c>0001-F7</c> B-053, B-054, B-056, B-145, B-147, B-150 and B-153 (decisions 0020 and
/// 0021): from schema <c>0.3.0</c> the parser maps <c>exact</c>, <c>latest-patch</c> and <c>latest-minor</c> to the
/// structure's policy, and a manifest with no key keeps <c>exact</c> (B-055); any other value is rejected naming it;
/// <c>latest-minor</c> on a <c>0.y.z</c> pin is rejected naming <c>latest-patch</c>; an unshipped pin under a floating
/// policy is rejected naming the pin and every held version; and a pin below <c>0.3.0</c> rejects the key as one that
/// version does not know. Each set beyond the embedded one holds the embedded <c>0.3.0</c> under another number.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtManifestParserPinPolicyUnitTests
{
    [Theory]
    [ClassData(typeof(PinPolicyValues))]
    public void APinPolicyTheEngineDefines_WhenParsed_ShouldBeThePolicyTheStructureCarries(string pin, string? value, PinPolicy expected)
    {
        // Given
        var parser = new SpechtManifestParser(EmbeddedWith("1.0.0"));
        var manifest = value is null
            ? $$"""{ "schemaVersion": "{{pin}}" }"""
            : $$"""{ "schemaVersion": "{{pin}}", "pinPolicy": "{{value}}" }""";

        // When
        var structure = parser.Parse(manifest);

        // Then
        structure.PinPolicy.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(UndefinedPinPolicies))]
    public void APinPolicyTheEngineDoesNotDefine_WhenParsed_ShouldRejectTheManifestNamingTheValue(string value, string named)
    {
        // Given
        var manifest = """{ "schemaVersion": "0.3.0", "pinPolicy": """ + value + " }";

        // When
        var parse = () => EngineServices.Parser().Parse(manifest);

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message
            .Should().MatchRegex(Naming(named), "the rejection names the value {0}", named);
    }

    [Fact]
    public void ALatestMinorPolicyOnAZeroMajorPin_WhenParsed_ShouldRejectTheManifestNamingLatestPatch()
    {
        // Given
        var parser = new SpechtManifestParser(EmbeddedWith("0.4.0"));

        // When
        var parse = () => parser.Parse("""{ "schemaVersion": "0.3.0", "pinPolicy": "latest-minor" }""");

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message
            .Should().MatchRegex(Naming("latest-patch"));
    }

    [Theory]
    [ClassData(typeof(UnshippedFloatingPins))]
    public void AnUnshippedPinUnderAFloatingPolicy_WhenParsed_ShouldRejectTheManifestNamingThePinAndEveryHeldVersion(
        string held, string pin, string policy)
    {
        // Given
        var parser = new SpechtManifestParser(new SchemaVersions([ZeroThreeZero() with { Number = Version(held) }]));

        // When
        var parse = () => parser.Parse($$"""{ "schemaVersion": "{{pin}}", "pinPolicy": "{{policy}}" }""");

        // Then
        var message = parse.Should().ThrowExactly<SpechtManifestException>().Which.Message;
        foreach (var named in new[] { pin, held })
        {
            message.Should().MatchRegex(Naming(named), "the rejection names version {0}", named);
        }
    }

    [Theory]
    [ClassData(typeof(PinsBeforePinPolicy))]
    public void APinBelowZeroThreeZeroDeclaringAPinPolicy_WhenParsed_ShouldRejectTheKeyAsOneTheEngineDoesNotKnow(string pin)
    {
        // Given
        var manifest = $$"""{ "schemaVersion": "{{pin}}", "pinPolicy": "latest-patch" }""";

        // When
        var parse = () => EngineServices.Parser().Parse(manifest);

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message
            .Should().Contain("does not know the key 'pinPolicy'");
    }

    private static SchemaVersions EmbeddedWith(string number) =>
        new([.. EngineServices.Versions().Versions, ZeroThreeZero() with { Number = Version(number) }]);

    private static SchemaVersion ZeroThreeZero() => EngineServices.Versions().Find(new SemanticVersion(0, 3, 0))!;

    private static SemanticVersion Version(string text) =>
        SemanticVersion.TryParse(text, out var version) ? version : throw new FormatException(text);

    private static string Naming(string value) => $@"(?<![\w.-]){Regex.Escape(value)}(?![\w-]|\.\w)";
}
