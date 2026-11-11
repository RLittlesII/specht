using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Identifiers;
using Specht.Tests.Discovery;
using Specht.Tests.Identifiers;
using Specht.Tests.Shared;
using Specht.Tool.Features.Mint;
using Spectre.Console.Testing;

namespace Specht.Tests.Mint;

/// <summary>
/// The mint command's fold of the engine's answer into its output and exit code, without a parser pipeline
/// (<c>0001-F19</c> C-9, C-10; § 7's exit-code table): the minted id alone on standard output and exit <c>0</c>, or nothing
/// on standard output, a reason on the injected error writer that names no absolute path, and the table's exit. The guard's
/// refusal (item <c>0416</c>; B-017, B-036, B-037) mints from a size of 1 over a short alphabet, each draw from
/// <see cref="FixedRandomSource"/> one candidate, with files named for the held candidates in the root's <c>.spec/adr/</c>.
/// A request over a tree whose ADRs are already numbered, minted or refused, leaves every file and folder of the file system
/// it is handed as it was (B-013; C-1; item <c>0419</c>), the time from a <see cref="FixedClock"/>.
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
        var command = new MintCommand(
            console,
            EngineServices.Reader(files),
            new Minter(new FixedRandomSource(0), new HeldIds(new SpechtDiscovery(files, new UnexpectedChildProcess())), new FixedClock(FixedClock.CheckTime)),
            errors);
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

    [Theory]
    [ClassData(typeof(GuardsHoldingEveryAttempt))]
    public async Task AnAdrEntryWithEveryCandidateItsGuardTriesHeld_WhenRequested_ShouldPrintNothingAndExitWithACodeTheCheckGives(
        int? attempts, int held)
    {
        // Given
        var alphabet = "abcdefghi"[..(held + 1)];
        var limit = attempts is null ? string.Empty : $", \"attempts\": {attempts}";
        var files = new MockFileSystem();
        files.AddFile(
            "consumer/.specht/manifest.json",
            new MockFileData(
                $$"""
                {
                  "schemaVersion": "0.3.1",
                  "identifiers": { "adr": "^[a-i]$" },
                  "mint": { "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "{{alphabet}}", "size": 1 }{{limit}} } }
                }
                """));
        foreach (var candidate in alphabet[..held])
        {
            files.AddFile($"consumer/.spec/adr/{candidate}-held.md", new MockFileData("# Held\n"));
        }

        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var minter = new Minter(
            new FixedRandomSource([.. Enumerable.Range(0, held + 1)]),
            new HeldIds(new SpechtDiscovery(files, new UnexpectedChildProcess())),
            new FixedClock(FixedClock.CheckTime));
        var command = new MintCommand(console, EngineServices.Reader(files), minter, errors);

        // When
        var exit = await command.ExecuteAsync(
            null!,
            new MintCommand.Settings { Type = "adr", Root = files.Path.GetFullPath("consumer") },
            TestContext.Current.CancellationToken);

        // Then
        console.Output.Should().BeEmpty(errors.ToString());
        exit.Should().BeOneOf([2, 3, 4], errors.ToString());
    }

    [Theory]
    [ClassData(typeof(RequestsOverNumberedAdrs))]
    public async Task ARequestOverATreeWithNumberedAdrs_WhenExecuted_ShouldCreateModifyAndDeleteNoFile(string? mint)
    {
        // Given
        var manifest = JsonNode.Parse("""{ "schemaVersion": "0.3.1", "identifiers": { "adr": "^[0-9a-z]+$" } }""")!.AsObject();
        if (mint is not null)
        {
            manifest["mint"] = JsonNode.Parse(mint);
        }

        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["consumer/.specht/manifest.json"] = new(manifest.ToJsonString()),
            ["consumer/.spec/adr/0001-first.md"] = new("# First\n"),
            ["consumer/.spec/adr/0002-second.md"] = new("# Second\n"),
        });
        var before = Snapshot(files);
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new MintCommand(
            new TestConsole(),
            EngineServices.Reader(files),
            new Minter(new FixedRandomSource(0), new HeldIds(new SpechtDiscovery(files, new UnexpectedChildProcess())), new FixedClock(FixedClock.CheckTime)),
            errors);

        // When
        await command.ExecuteAsync(
            null!,
            new MintCommand.Settings { Type = "adr", Root = files.Path.GetFullPath("consumer") },
            TestContext.Current.CancellationToken);

        // Then
        Snapshot(files).Should().BeEquivalentTo(before, errors.ToString());

        static Dictionary<string, (string? Text, DateTimeOffset Written)> Snapshot(MockFileSystem files) =>
            files.AllPaths.ToDictionary(
                static path => path,
                path => files.GetFile(path) is { IsDirectory: false } file ? (file.TextContents, file.LastWriteTime) : default);
    }
}
