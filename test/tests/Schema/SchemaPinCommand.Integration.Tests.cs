using System.Text;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Schema;

/// <summary>Pinning through the shipped host, including exact-byte preservation and recovery.</summary>
[Trait("Tier", "Integration")]
public sealed class SchemaPinCommandIntegrationTests
{
    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-16")]
    [InlineData("utf-16BE")]
    [InlineData("utf-32")]
    [InlineData("utf-32BE")]
    public void SupportedManifestEncoding_WhenListingAndPinning_ShouldMatchTheCheckAndPreserveBytes(string name)
    {
        // Given
        using var tree = new SpechtTree();
        var encoding = Encoding.GetEncoding(name);
        const string text = "{\r\n\"$comment\":\"😀\",\r\n\"schemaVersion\":\"0.1.0\"\r\n}";
        var path = Path.Combine(tree.Root, SpechtTree.ManifestPath);
        File.WriteAllBytes(path, [.. encoding.GetPreamble(), .. encoding.GetBytes(text)]);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root);
        var list = AcceptanceTool.Launch(tree.Root, "schema", "list");
        var pin = AcceptanceTool.Launch(tree.Root, "schema", "pin", "0.2.0");

        // Then
        ExitCode.Should().Be(0, Stderr);
        list.ExitCode.Should().Be(0, list.Stderr);
        list.Stdout.Should().Contain("0.1.0 (pinned)");
        pin.ExitCode.Should().Be(0, pin.Stderr);
        File.ReadAllBytes(path).Should().Equal([.. encoding.GetPreamble(), .. encoding.GetBytes(text.Replace("0.1.0", "0.2.0"))]);
    }

    [Fact]
    public void NewerSettings_WhenDowngrading_ShouldSurviveEvenWhenTheSubsequentCheckRejectsThem()
    {
        // Given
        using var tree = new SpechtTree();
        var path = Path.Combine(tree.Root, SpechtTree.ManifestPath);
        const string before = "{\"schemaVersion\":\"0.2.0\",\"claim\":{\"tag\":\"@req:{claim}\",\"tagIgnoreCase\":false}}";
        File.WriteAllText(path, before);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "schema", "pin", "0.1.0");
        var check = AcceptanceTool.Launch(tree.Root);

        // Then
        ExitCode.Should().Be(0, Stderr);
        File.ReadAllText(path).Should().Be(before.Replace("0.2.0", "0.1.0", StringComparison.Ordinal));
        check.ExitCode.Should().Be(3);
        check.Stderr.Should().Contain("claim");
    }

    [Theory]
    [InlineData("0.1.0", "0.2.0", "SPEC031")]
    [InlineData("0.2.0", "0.1.0", "SPEC031")]
    [InlineData("7.0.0", "0.2.0", "SPEC4004")]
    [InlineData("wrong", "0.2.0", "SPEC4004")]
    public void ExistingPin_WhenPinning_ShouldChangeOnlyItsValue(string before, string target, string rule)
    {
        // Given
        using var tree = new SpechtTree();
        var text = "{\r\n  \"$comment\": \"schemaVersion in a value is untouched 😀\",\r\n  \"schemaVersion\" : \"" + before
            + "\",\r\n  \"rules\": {\"" + rule + "\": \"warning\"}\r\n}\r\n";
        var path = Path.Combine(tree.Root, SpechtTree.ManifestPath);
        File.WriteAllBytes(path, [0xEF, 0xBB, 0xBF, .. Encoding.UTF8.GetBytes(text)]);
        var snapshot = Directory.GetFiles(tree.Root, "*", SearchOption.AllDirectories).ToDictionary(
            file => Path.GetRelativePath(tree.Root, file).Replace('\\', '/'), File.ReadAllBytes);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "schema", "pin", target);

        // Then
        ExitCode.Should().Be(0, Stderr);
        Stdout.Should().Contain(target).And.Contain(SpechtTree.ManifestPath);
        File.ReadAllBytes(path).Should().Equal([0xEF, 0xBB, 0xBF, .. Encoding.UTF8.GetBytes(text.Replace('"' + before + '"', '"' + target + '"'))]);
        Directory.GetFiles(tree.Root, "*", SearchOption.AllDirectories).Select(file => Path.GetRelativePath(tree.Root, file).Replace('\\', '/'))
            .Should().BeEquivalentTo(snapshot.Keys);
        foreach (var entry in snapshot.Where(static entry => entry.Key != SpechtTree.ManifestPath))
        {
            File.ReadAllBytes(Path.Combine(tree.Root, entry.Key)).Should().Equal(entry.Value);
        }
    }

    [Theory]
    [InlineData("7.0.0", 4)]
    [InlineData("0.1", 4)]
    public void InvalidTarget_WhenPinning_ShouldWriteNothing(string target, int code)
    {
        // Given
        using var tree = new SpechtTree();
        var path = Path.Combine(tree.Root, SpechtTree.ManifestPath);
        var before = File.ReadAllBytes(path);

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "schema", "pin", target);

        // Then
        ExitCode.Should().Be(code);
        Stdout.Should().BeEmpty();
        Stderr.Should().Contain(target).And.Contain("0.2.0").And.NotContain(tree.Root);
        File.ReadAllBytes(path).Should().Equal(before);
    }

    [Theory]
    [InlineData(null, 2)]
    [InlineData("{", 3)]
    [InlineData("null", 3)]
    [InlineData("{\"rules\":{\"SPEC031\":\"invalid\"}}", 3)]
    [InlineData("{\"unknown\":true}", 3)]
    public void InvalidManifest_WhenNamingAnInvalidTarget_ShouldReportTheManifestFirst(string? text, int code)
    {
        // Given
        using var tree = new SpechtTree();
        var path = Path.Combine(tree.Root, SpechtTree.ManifestPath);
        if (text is null)
        {
            File.Delete(path);
        }
        else
        {
            File.WriteAllText(path, text);
        }

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "schema", "pin", "7.0.0");

        // Then
        ExitCode.Should().Be(code);
        Stdout.Should().BeEmpty();
        Stderr.Should().Contain(SpechtTree.ManifestPath).And.NotContain(tree.Root);
        if (text is null)
        {
            File.Exists(path).Should().BeFalse();
        }
        else
        {
            File.ReadAllText(path).Should().Be(text);
        }
    }
}
