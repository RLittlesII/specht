using System.IO.Abstractions;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;
using Spectre.Console.Testing;

namespace Specht.Tests.Schema;

/// <summary>The real Spectre selection prompt and its redirected-input boundary (0001-F7 B-057, B-058, B-113).</summary>
[Trait("Tier", "Integration")]
public sealed class SchemaPinSelectorIntegrationTests
{
    [Theory]
    [InlineData(0, "0.1.0")]
    [InlineData(1, "0.2.0")]
    public void InteractiveTerminal_WhenChoosingWithArrowKeysAndEnter_ShouldOfferEveryVersionAndPinTheChoice(int moves, string target)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteRaw(SpecTree.ManifestPath, "{\"schemaVersion\":\"wrong\"}");
        var console = new TestConsole().Width(100);
        console.Profile.Capabilities.Interactive = true;
        for (var move = 0; move < moves; move++)
        {
            console.Input.PushKey(ConsoleKey.DownArrow);
        }
        console.Input.PushKey(ConsoleKey.Enter);
        var app = EngineServices.CommandApp(new FileSystem(), console);

        // When
        var result = app.Run("schema", "pin", "--root", tree.Root);

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        foreach (var version in EngineServices.Versions().Versions)
        {
            result.Output.Should().Contain(version.Number.ToString());
        }
        File.ReadAllText(Path.Combine(tree.Root, SpecTree.ManifestPath)).Should().Be("{\"schemaVersion\":\"" + target + "\"}");
        result.Output.Should().Contain("pinned schemaVersion " + target);
    }

    [Fact]
    public void RedirectedInput_WhenNoVersionIsSupplied_ShouldSuggestAnExplicitVersionAndWriteNothing()
    {
        // Given
        using var tree = new SpecTree();
        var path = Path.Combine(tree.Root, SpecTree.ManifestPath);
        var before = File.ReadAllBytes(path);

        // When
        var result = AcceptanceTool.Launch(tree.Root, "schema", "pin");

        // Then
        result.ExitCode.Should().Be(2);
        result.Stdout.Should().BeEmpty();
        result.Stderr.Should().Contain("specht schema pin <version>").And.NotContain(tree.Root);
        File.ReadAllBytes(path).Should().Equal(before);
    }
}
