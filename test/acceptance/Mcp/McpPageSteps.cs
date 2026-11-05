using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Mcp;

/// <summary>
/// Steps for the three scenarios of <c>docs/mcp.md</c> (0132-F3 B-015, B-016, B-031; item 0332). The page is this repository's, copied beside
/// the tests at its repository-relative path. The tools the host offers are read over the protocol from the built program, so a tool the host
/// registers is seen without a test naming it, and the option a configuration entry names is read from the host's own help. How an entry, a
/// section and an anchor are recognised is § 8 of the specification.
/// </summary>
[Binding]
[Scope(Feature = "The MCP host")]
public sealed partial class McpPageSteps
{
    [Given("the tool's MCP documentation page")]
    public void GivenTheMcpPage() => File.Exists(Copied(PagePath)).Should().BeTrue($"{PagePath} is the one MCP page of this repository (C-8)");

    [Given("the tools the server offers")]
    public void GivenTheToolsTheServerOffers()
    {
        using var session = AcceptanceTool.McpSession.Start(Path.GetTempPath());
        var listing = session.Exchange(AcceptanceTool.McpRequest(1, "tools/list", AcceptanceTool.McpRevision));
        _offered =
        [
            .. (JsonNode.Parse(listing ?? "null")?["result"]?["tools"] as JsonArray
                ?? throw new Xunit.Sdk.XunitException($"The host did not list its tools; tools/list answered: {listing ?? "nothing"}"))
                .Select(static tool => tool!["name"]!.GetValue<string>()),
        ];
        _offered.Should().NotBeEmpty("the host offers tools");
    }

    [When("the tool's MCP documentation page is compared with them")]
    public void WhenThePageIsComparedWithThem() => File.Exists(Copied(PagePath)).Should().BeTrue($"{PagePath} is the one MCP page of this repository (C-8)");

    [Then("it gives a client configuration entry that runs the MCP through the local tool manifest, naming the repository root")]
    public void ThenItGivesTheLocalEntry() =>
        HelpListed(Entry(static (tokens, at) => tokens[at] == "dotnet" && At(tokens, at + 1) == "specht-mcp", skip: 1), "dotnet specht-mcp");

    [Then("it gives one that runs the MCP from a global install, naming the repository root")]
    public void ThenItGivesTheGlobalEntry() =>
        HelpListed(Entry(static (tokens, at) => tokens[at] == "specht-mcp" && At(tokens, at - 1) != "dotnet", skip: 0), "specht-mcp");

    [Then("each tool has exactly one entry on the page")]
    public void ThenEachToolHasExactlyOneEntry()
    {
        var entries = Entries(Read(PagePath));
        _offered.Where(tool => entries.Count(entry => entry == tool) != 1)
            .Should()
            .BeEmpty($"each tool the host offers has one entry in the tools section of {PagePath} (B-016)");
    }

    [Then("every tool with an entry is one the server offers")]
    public void ThenEveryEntryIsOffered()
    {
        var page = Read(PagePath);
        Entries(page).Except(_offered)
            .Should()
            .BeEmpty($"{PagePath} has an entry only for a tool the host offers (B-016, C-8)");
        ToolsSection(page).Where(static line => Planned().IsMatch(line))
            .Should()
            .BeEmpty($"{PagePath} has no planned marker in its tools section (C-8)");
    }

    [Then("it has one entry on restoring the tool")]
    public void ThenItHasAnEntryOnRestoring() => OneEntryAt("restore-the-tool");

    [Then("it has one entry on pinning the tool")]
    public void ThenItHasAnEntryOnPinning() => OneEntryAt("pin-the-tool");

    [Then("it has one entry on updating the tool")]
    public void ThenItHasAnEntryOnUpdating() => OneEntryAt("update-the-tool");

    private static void OneEntryAt(string anchor) =>
        Headings(Read(PagePath)).Count(heading => Slug(heading.Text) == anchor)
            .Should()
            .Be(1, $"{PagePath} has exactly one heading whose anchor is #{anchor} (B-031, C-12)");

    private static string[] Entry(Func<string[], int, bool> starts, int skip)
    {
        var blocks = Blocks(Read(PagePath));
        var entry = blocks
            .Select(static block => Tokens(block))
            .FirstOrDefault(tokens => Enumerable.Range(0, tokens.Length).Any(at => starts(tokens, at) && NamesRoot(tokens, at + skip)));
        entry.Should()
            .NotBeNull(
                $"{PagePath} has a fenced client configuration entry that runs the MCP and names the root with --root; "
                + $"its code blocks are {string.Join(" | ", blocks)}");
        return entry!;
    }

    private static void HelpListed(string[] entry, string runs)
    {
        var (stdout, stderr, exitCode) = AcceptanceTool.LaunchMcp(Path.GetTempPath(), [], "--help");
        exitCode.Should().Be(0, stdout + stderr);
        var help = stdout + stderr;
        entry.Where(static token => token.StartsWith("--", StringComparison.Ordinal)).Select(static token => token.Split('=')[0])
            .Except(OptionName().Matches(help).Select(static match => match.Value))
            .Should()
            .BeEmpty($"the entry that runs {runs} names only options the host's help lists (C-8)");
    }

    private static bool NamesRoot(string[] tokens, int from)
    {
        for (var at = from + 1; at < tokens.Length; at++)
        {
            if (tokens[at].StartsWith("--root=", StringComparison.Ordinal) && tokens[at].Length > "--root=".Length)
            {
                return true;
            }

            if (tokens[at] == "--root" && At(tokens, at + 1) is { Length: > 0 } value && !value.StartsWith('-'))
            {
                return true;
            }
        }

        return false;
    }

    private static string At(string[] tokens, int at) => at >= 0 && at < tokens.Length ? tokens[at] : string.Empty;

    private static string[] Tokens(string block) =>
        [.. Separator().Split(JsonKey().Replace(block, " ")).Where(static token => token.Length > 0)];

    private static IEnumerable<string> Blocks(string page)
    {
        var block = new List<string>();
        var fenced = false;
        foreach (var line in page.Split('\n'))
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                if (fenced)
                {
                    yield return string.Join('\n', block);
                    block.Clear();
                }

                fenced = !fenced;
            }
            else if (fenced)
            {
                block.Add(line);
            }
        }
    }

    private static List<(int Level, string Text)> Headings(string page)
    {
        var headings = new List<(int Level, string Text)>();
        var fenced = false;
        foreach (var line in page.Split('\n'))
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                fenced = !fenced;
            }
            else if (!fenced && Heading().Match(line) is { Success: true } heading)
            {
                headings.Add((heading.Groups[1].Length, heading.Groups[2].Value));
            }
        }

        return headings;
    }

    private static IEnumerable<string> ToolsSection(string page)
    {
        var headings = Headings(page);
        headings.Count(static heading => string.Equals(heading.Text, "Tools", StringComparison.OrdinalIgnoreCase))
            .Should()
            .Be(1, $"{PagePath} has one tools section, a heading `Tools`");
        var level = headings.Single(static heading => string.Equals(heading.Text, "Tools", StringComparison.OrdinalIgnoreCase)).Level;
        return page.Split('\n')
            .SkipWhile(line => !(Heading().Match(line) is { Success: true } start
                && start.Groups[1].Length == level
                && start.Groups[2].Value.Equals("Tools", StringComparison.OrdinalIgnoreCase)))
            .Skip(1)
            .TakeWhile(line => !(Heading().Match(line) is { Success: true } next && next.Groups[1].Length <= level));
    }

    private static List<string> Entries(string page)
    {
        var section = ToolsSection(page).ToList();
        var fenced = false;
        var entries = new List<string>();
        foreach (var line in section)
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                fenced = !fenced;
            }
            else if (!fenced && Heading().Match(line) is { Success: true } heading)
            {
                entries.Add(ToolName().Match(heading.Groups[2].Value) is { Success: true } name ? name.Groups[1].Value : $"<{heading.Groups[2].Value}>");
            }
        }

        return entries;
    }

    private static string Slug(string heading) =>
        NotInAnchor().Replace(heading.Replace("`", string.Empty).Replace("*", string.Empty).ToLowerInvariant(), string.Empty).Replace(' ', '-');

    private static string Read(string page)
    {
        File.Exists(Copied(page)).Should().BeTrue($"{page} is the one MCP page of this repository (C-8)");
        return File.ReadAllText(Copied(page)).ReplaceLineEndings("\n");
    }

    private static string Copied(string path) => Path.Combine([AppContext.BaseDirectory, .. path.Split('/')]);

    [GeneratedRegex(@"^(#{1,6}) +(.+?) *$")]
    private static partial Regex Heading();

    [GeneratedRegex(@"^`([^`\s]+)`$")]
    private static partial Regex ToolName();

    [GeneratedRegex(@"""[^""]*""\s*:")]
    private static partial Regex JsonKey();

    [GeneratedRegex(@"[\s""',\[\]{}]+")]
    private static partial Regex Separator();

    [GeneratedRegex(@"(?<![\w-])--[A-Za-z][\w-]*")]
    private static partial Regex OptionName();

    [GeneratedRegex(@"[^\p{L}\p{N}\s_-]")]
    private static partial Regex NotInAnchor();

    [GeneratedRegex("planned", RegexOptions.IgnoreCase)]
    private static partial Regex Planned();

    private const string PagePath = "docs/mcp.md";
    private IReadOnlyList<string> _offered = [];
}
