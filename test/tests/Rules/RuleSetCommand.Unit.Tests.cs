using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Tool.Features.Rules.Set;

namespace Specht.Tests.Rules;

/// <summary>Proves the command's exit-code and diagnostic folds without a CLI parser pipeline.</summary>
[Trait("Tier", "Unit")]
public sealed class RuleSetCommandUnitTests
{
    [Theory]
    [InlineData("{}", "SPEC010", "warning", false, 0)]
    [InlineData("{}", "SPEC999", "warning", false, 4)]
    [InlineData("{}", "SPEC999", "loud", false, 4)]
    [InlineData("{}", "SPEC010", "Warning", false, 4)]
    [InlineData("{", "SPEC999", "loud", false, 3)]
    [InlineData(null, "SPEC999", "loud", false, 2)]
    [InlineData(null, "SPEC999", "loud", true, 2)]
    public async Task Input_WhenExecuted_ShouldFoldTheEngineFailureIntoTheExistingExitCode(
        string? manifest, string id, string level, bool missingRoot, int expected)
    {
        // Given
        var files = new MockFileSystem();
        if (!missingRoot)
        {
            files.Directory.CreateDirectory("consumer");
        }
        if (manifest is not null)
        {
            files.AddFile("consumer/.specht/manifest.json", new MockFileData(manifest));
        }
        var parser = EngineServices.Parser();
        var pinEditor = new ManifestPinEditor(parser, EngineServices.Versions());
        var setter = new RuleLevelSetter(new SpecManifestStore(files), parser, new ManifestRuleEditor(pinEditor), pinEditor);
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new RuleSetCommand(setter, errors);
        var root = files.Path.GetFullPath("consumer");

        // When
        var result = await command.ExecuteAsync(null!, new RuleSetCommand.Settings { Root = root, Id = id, Level = level },
            TestContext.Current.CancellationToken);

        // Then
        result.Should().Be(expected);
        if (expected == 0)
        {
            errors.ToString().Should().BeEmpty();
        }
        else
        {
            errors.ToString().Should().StartWith("specht: ").And.NotContain(root).And.NotContain("\\");
            if (expected == 4 && id == "SPEC999")
            {
                errors.ToString().Should().Contain(id);
            }
            if (expected == 4 && level is not ("error" or "warning" or "off"))
            {
                errors.ToString().Should().Contain(level).And.Contain("error").And.Contain("warning").And.Contain("off");
            }
            if (manifest is not null)
            {
                files.File.ReadAllText("consumer/.specht/manifest.json").Should().Be(manifest);
            }
        }
    }
}
