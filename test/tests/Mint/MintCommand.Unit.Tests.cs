using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Identifiers;
using Specht.Tests.Identifiers;
using Specht.Tests.Shared;
using Specht.Tool.Features.Mint;
using Spectre.Console.Testing;

namespace Specht.Tests.Mint;

/// <summary>
/// The mint command's fold of the engine's answer into its output and exit code, without a parser pipeline
/// (<c>0001-F19</c> C-9, C-10; § 7's exit-code table): the minted id alone on standard output and exit <c>0</c>, or nothing
/// on standard output, a reason on the injected error writer that names no absolute path, and the table's exit.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class MintCommandUnitTests
{
    [Theory]
    [ClassData(typeof(MintRequestFolds))]
    public async Task ARequest_WhenExecuted_ShouldFoldTheEngineAnswerIntoItsOutputAndExitCode(
        string? manifest, string type, string? feature, bool rootExists, int expected)
    {
        // Given
        var files = new MockFileSystem();
        if (rootExists)
        {
            files.Directory.CreateDirectory("consumer");
        }

        if (manifest is not null)
        {
            files.AddFile("consumer/.specht/manifest.json", new MockFileData(manifest));
        }

        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new MintCommand(console, EngineServices.Reader(files), new Minter(new FixedRandomSource(0)), errors);
        var root = files.Path.GetFullPath("consumer");

        // When
        var exit = await command.ExecuteAsync(
            null!, new MintCommand.Settings { Type = type, Feature = feature, Root = root }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(expected, errors.ToString());
        if (expected == 0)
        {
            console.Output.Should().MatchRegex(@"\Azzz(\r?\n)?\z");
            errors.ToString().Should().BeEmpty();
        }
        else
        {
            console.Output.Should().BeEmpty();
            errors.ToString().Should().StartWith("specht: ").And.NotContain(root).And.NotContain("\\");
        }
    }
}
