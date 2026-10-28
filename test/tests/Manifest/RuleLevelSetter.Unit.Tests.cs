using System.IO.Abstractions.TestingHelpers;
using System.Text;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>Proves setter validation, exact pins and atomic no-op behavior beneath 0001-F11.</summary>
[Trait("Tier", "Unit")]
public sealed class RuleLevelSetterUnitTests
{
    [Theory]
    [InlineData("0.1.0", "error")]
    [InlineData("0.2.0", "warning")]
    [InlineData("0.2.0", "off")]
    public async Task AcceptedManifest_WhenSetting_ShouldRemainAcceptedWithOnlyTheNamedOverride(string pin, string level)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["consumer/.specht/manifest.json"] = new($$"""{"schemaVersion":"{{pin}}","$comment":"kept"}"""),
            ["consumer/sentinel.txt"] = new("untouched"),
        });
        var parser = EngineServices.Parser();
        var setter = new RuleLevelSetter(new SpecManifestStore(files), parser,
            new ManifestRuleEditor(new ManifestPinEditor(parser, EngineServices.Versions())), new ManifestPinEditor(parser, EngineServices.Versions()));

        // When
        await setter.Set("consumer", "SPEC010", level, CancellationToken.None);

        // Then
        var changed = files.File.ReadAllText("consumer/.specht/manifest.json");
        var manifest = parser.Parse(changed);
        parser.PinnedVersion(manifest).Number.ToString().Should().Be(pin);
        manifest.Rules.Should().ContainSingle().Which.Key.Should().Be("SPEC010");
        manifest.Rules["SPEC010"].ToString().ToLowerInvariant().Should().Be(level);
        changed.Should().Contain("\"$comment\":\"kept\"");
        files.File.ReadAllText("consumer/sentinel.txt").Should().Be("untouched");
        files.AllFiles.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("{}", "SPEC999", "warning", 4)]
    [InlineData("{}", "SPEC010", "Warning", 4)]
    [InlineData("{}", "SPEC010", "loud", 4)]
    [InlineData("{", "SPEC999", "loud", 3)]
    [InlineData("{\"unknown\":true}", "SPEC999", "loud", 3)]
    [InlineData("{\"schemaVersion\":\"9.0.0\"}", "SPEC999", "loud", 3)]
    [InlineData("{\"rules\":{\"SPEC010\":\"loud\"}}", "SPEC010", "warning", 3)]
    [InlineData("{\"schemaVersion\":\"0.1.0\"}", "SPEC035", "off", 4)]
    public async Task RejectedInput_WhenSetting_ShouldValidateManifestFirstAndWriteNothing(string text, string id, string level, int code)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData> { ["consumer/.specht/manifest.json"] = new(text) });
        var setter = new RuleLevelSetter(new SpecManifestStore(files), EngineServices.Parser(),
            new ManifestRuleEditor(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions())),
            new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()));

        // When
        var exception = await Record.ExceptionAsync(() => setter.Set("consumer", id, level, CancellationToken.None));

        // Then
        exception.Should().NotBeNull();
        if (code == 4)
        {
            exception.Should().BeOfType<RuleLevelNotFoundException>();
        }
        else
        {
            (exception is SpechtManifestException or SpechtManifestUnreadableException).Should().BeTrue();
        }
        files.File.ReadAllBytes("consumer/.specht/manifest.json").Should().Equal(Encoding.UTF8.GetBytes(text));
        files.AllFiles.Should().ContainSingle();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingInput_WhenSetting_ShouldFailBeforeArgumentsAndCreateNothing(bool rootExists)
    {
        // Given
        var files = new MockFileSystem();
        if (rootExists)
        {
            files.Directory.CreateDirectory("consumer");
        }
        var setter = new RuleLevelSetter(new SpecManifestStore(files), EngineServices.Parser(),
            new ManifestRuleEditor(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions())),
            new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()));

        // When
        var exception = await Record.ExceptionAsync(() => setter.Set("consumer", "SPEC999", "loud", CancellationToken.None));

        // Then
        if (rootExists)
        {
            exception.Should().BeOfType<SpechtManifestNotFoundException>();
        }
        else
        {
            exception.Should().BeOfType<SpechtRootNotFoundException>();
        }
        files.AllFiles.Should().BeEmpty();
    }
}
