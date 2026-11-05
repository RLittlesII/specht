using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Tests.Mcp;

/// <summary>
/// Holds the pointers in the host's messages against the page they point to (0132-F3 B-031, C-12): the messages are the ones
/// <c>SpechtTool</c> composes over a scripted set of programs, and the page is <c>docs/mcp.md</c>, copied beside the tests at its
/// repository-relative path, so a pointer and an anchor cannot drift apart.
/// </summary>
[Trait("Tier", "Integration")]
public sealed partial class McpPageIntegrationTests
{
    [Fact]
    public async Task TheMessagesThatPointToThePage_WhenTheirPointersAreReadAgainstIt_ShouldEachResolveToExactlyOneHeading()
    {
        // Given
        var page = Path.Combine(AppContext.BaseDirectory, "docs", "mcp.md");
        File.Exists(page).Should().BeTrue("docs/mcp.md is the one MCP page of this repository (C-8)");
        var anchors = Headings(File.ReadAllText(page).ReplaceLineEndings("\n")).Select(Slug).ToList();

        // When
        var messages = new List<string>();
        foreach (var process in new[]
                 {
                     new ScriptedProcessRun { ListedPin = ScriptedProcessRun.Pin },
                     new ScriptedProcessRun { GlobalReports = "96.54.32" },
                     ScriptedProcessRun.Tool(pinned: true, "0.0.1"),
                 })
        {
            SpechtTool tool = new SpechtToolFixture().WithProcess(process);
            messages.Add(
                await tool.RunAsync(["--json"], CancellationToken.None) switch
                {
                    ToolOutcome.Refused refused => refused.Message,
                    ToolOutcome.Ran { UnpinnedReport: { } report } => report,
                    _ => string.Empty,
                });
        }

        // Then
        var pointers = messages.SelectMany(static message => Pointer().Matches(message).Select(static match => match.Groups[1].Value)).Distinct().ToList();
        pointers.Should().BeEquivalentTo(["restore-the-tool", "pin-the-tool", "update-the-tool"], "the three messages point to the three entries (C-12)");
        pointers.Where(pointer => anchors.Count(anchor => anchor == pointer) != 1)
            .Should()
            .BeEmpty("each pointer in a message resolves to exactly one heading of docs/mcp.md (B-031, C-12)");
    }

    private static List<string> Headings(string page)
    {
        var headings = new List<string>();
        var fenced = false;
        foreach (var line in page.Split('\n'))
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                fenced = !fenced;
            }
            else if (!fenced && Heading().Match(line) is { Success: true } heading)
            {
                headings.Add(heading.Groups[1].Value);
            }
        }

        return headings;
    }

    private static string Slug(string heading) =>
        NotInAnchor().Replace(heading.Replace("`", string.Empty).Replace("*", string.Empty).ToLowerInvariant(), string.Empty).Replace(' ', '-');

    [GeneratedRegex(@"docs/mcp\.md#([a-z][a-z-]*)")]
    private static partial Regex Pointer();

    [GeneratedRegex(@"^#{1,6} +(.+?) *$")]
    private static partial Regex Heading();

    [GeneratedRegex(@"[^\p{L}\p{N}\s_-]")]
    private static partial Regex NotInAnchor();
}
