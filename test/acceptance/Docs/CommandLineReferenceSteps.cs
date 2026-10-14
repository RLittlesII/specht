using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Docs;

/// <summary>
/// Steps for <c>docs/.spec/docs-as-first-class-citizen.feature</c> (0002-F1): item 0181 binds B-003 and B-006 to B-011, and
/// the file's other scenarios stay pending for their items. The command-line surface (A-6) is read from the launched
/// tool's own help, so a command the host registers is seen without a test naming it. The pages are this repository's,
/// copied beside the tests at their repository-relative paths. How a section, an entry and a name (A-7) are recognised is
/// § 8 of the specification.
/// </summary>
[Binding]
[Scope(Feature = "Docs as first-class citizen")]
public sealed partial class CommandLineReferenceSteps
{
    [Given("the repository's documentation")]
    public void GivenTheRepositorysDocumentation() =>
        Directory.Exists(Copied("docs")).Should().BeTrue("docs/ holds the repository's documentation");

    [Given("the commands the tool's help lists")]
    [Given("the options a command's help lists")]
    public void GivenWhatTheToolsHelpLists() => Surface.Should().ContainKey(DefaultCommand);

    [Given("the command-line reference")]
    public void GivenTheCommandLineReference() => _page = Reference;

    [Given("a command a specification describes and the tool's help does not list")]
    public void GivenACommandASpecificationDescribesAndTheToolsHelpDoesNotList()
    {
        _unbuilt = [.. SpecifiedCommands.Except(Surface.Keys)];
        _unbuilt.Should().NotBeEmpty("0002-F1 § 5 row 6 names commands that are specified and not built");
    }

    [Given("the repository's front page")]
    public void GivenTheRepositorysFrontPage() => _page = "README.md";

    [Given("the readme the package carries")]
    public void GivenTheReadmeThePackageCarries() => _page = "src/tool/README.md";

    [When("the command-line reference is compared with them")]
    [When("the command-line reference is compared with the tool's help")]
    [When("it is compared with the tool's help")]
    public void WhenItIsComparedWithTheToolsHelp() => Surface.Should().NotBeEmpty();

    [Then("the command-line reference is one page of it")]
    public void ThenTheCommandLineReferenceIsOnePageOfIt() =>
        File.Exists(Copied(Reference)).Should().BeTrue($"the command-line reference is the page {Reference} (B-003)");

    [Then("it is not a command's specification")]
    public void ThenItIsNotACommandsSpecification() =>
        Read(Reference).Split('\n').Should().NotContain("type: feature", $"{Reference} is a usage page, never a specification (A-4)");

    [Then("each command has a section of its own in the reference")]
    public void ThenEachCommandHasASectionOfItsOwnInTheReference()
    {
        var sections = Sections();
        Surface.Keys.Where(command => sections.Count(section => section.Command == command) != 1)
            .Should()
            .BeEmpty($"each command the help lists has one section in {Reference} (B-006)");
    }

    [Then("each option has an entry in that command's section")]
    public void ThenEachOptionHasAnEntryInThatCommandsSection()
    {
        var sections = Sections();
        Surface.SelectMany(command => command.Value
                .Select(static option => option[^1])
                .Except(sections.Where(section => section.Command == command.Key).SelectMany(static section => section.Options))
                .Select(option => $"{command.Key} {option}"))
            .Should()
            .BeEmpty($"each option a command's help lists has an entry in that command's section of {Reference} (B-007)");
    }

    [Then("every command with a section is one the help lists")]
    public void ThenEveryCommandWithASectionIsOneTheHelpLists() =>
        Sections().Select(static section => section.Command).Except(Surface.Keys)
            .Should()
            .BeEmpty($"{Reference} has a section only for a command the help lists (B-008)");

    [Then("every option with an entry is one its command's help lists")]
    public void ThenEveryOptionWithAnEntryIsOneItsCommandsHelpLists() =>
        Sections().Where(section => Surface.ContainsKey(section.Command))
            .SelectMany(section => section.Options.Except(Surface[section.Command].SelectMany(static option => option))
                .Select(option => $"{section.Command} {option}"))
            .Should()
            .BeEmpty($"{Reference} has an entry only for an option its command's help lists (B-008)");

    [Then("the reference has no section for that command")]
    public void ThenTheReferenceHasNoSectionForThatCommand() =>
        Sections().Select(static section => section.Command).Intersect(_unbuilt)
            .Should()
            .BeEmpty($"{Reference} has no section for a command the help does not list (B-008, C-8)");

    [Then("every command and option it names is one the help lists")]
    public void ThenEveryCommandAndOptionItNamesIsOneTheHelpLists() =>
        string.Join(", ", Unlisted(Page).Distinct()).Should().BeEmpty($"{Page} names only what the tool's help lists (A-6, A-7)");

    [Then("it links to the command-line reference")]
    public void ThenItLinksToTheCommandLineReference() =>
        ReferenceLink().IsMatch(Read(Page)).Should().BeTrue($"{Page} links to {Reference} (B-011)");

    private Dictionary<string, string[][]> Surface => _surface ??= ReadSurface();

    private string Page => _page ?? throw new InvalidOperationException("No page was named.");

    private static Dictionary<string, string[][]> ReadSurface()
    {
        var surface = new Dictionary<string, string[][]>(StringComparer.Ordinal);
        var pending = new Queue<string[]>();
        pending.Enqueue([]);
        while (pending.TryDequeue(out var command))
        {
            var help = Help(command);
            surface[string.Join(" ", new[] { DefaultCommand }.Concat(command))] = Options(help);
            foreach (var listed in Block(help, "COMMANDS:").Select(static line => CommandEntry().Match(line)).Where(static match => match.Success))
            {
                pending.Enqueue([.. command, listed.Groups[1].Value]);
            }
        }

        return surface;
    }

    private static string Help(params string[] command)
    {
        var (stdout, stderr, exitCode) = AcceptanceTool.Launch(Path.GetTempPath(), [.. command, "--help"]);
        exitCode.Should().Be(0, stdout + stderr);
        var help = EscapeSequence().Replace(stdout, string.Empty);
        Block(help, "OPTIONS:").Should().NotBeEmpty($"the help of every command lists its options, but read {help}");
        return help;
    }

    private static IEnumerable<string> Block(string help, string heading) =>
        help.ReplaceLineEndings("\n")
            .Split('\n')
            .Select(static line => line.TrimEnd())
            .SkipWhile(line => line != heading)
            .Skip(1)
            .TakeWhile(static line => line.Length > 0);

    private static string[][] Options(string help) =>
        [
            .. Block(help, "OPTIONS:")
                .Select(static line => OptionDeclaration().Match(line))
                .Where(static match => match.Success)
                .Select(static match => Names(match.Groups[1].Value)),
        ];

    private static string[] Names(string text) => [.. OptionName().Matches(text).Select(static match => match.Value)];

    private static List<(string Command, List<string> Options)> Sections()
    {
        var sections = new List<(string Command, List<string> Options)>();
        foreach (var line in Read(Reference).Split('\n'))
        {
            if (SectionHeading().Match(line) is { Success: true } heading)
            {
                sections.Add((heading.Groups[1].Value, []));
            }
            else if (sections.Count > 0 && OptionEntry().Match(line) is { Success: true } entry)
            {
                sections[^1].Options.AddRange(Names(entry.Groups[1].Value));
            }
        }

        return sections;
    }

    private static IEnumerable<string> Code(string page)
    {
        var fenced = false;
        foreach (var line in page.Split('\n'))
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                fenced = !fenced;
            }
            else if (fenced)
            {
                yield return line.Trim();
            }
            else
            {
                foreach (Match span in CodeSpan().Matches(line))
                {
                    yield return span.Groups[1].Value.Trim();
                }
            }
        }
    }

    private static string Read(string page)
    {
        File.Exists(Copied(page)).Should().BeTrue($"{page} is a page of this repository");
        return File.ReadAllText(Copied(page)).ReplaceLineEndings("\n");
    }

    private static string Copied(string path) => Path.Combine([AppContext.BaseDirectory, .. path.Split('/')]);

    [GeneratedRegex(@"\x1b\[[0-9;]*m")]
    private static partial Regex EscapeSequence();

    [GeneratedRegex(@"^ {4}(\S+)")]
    private static partial Regex CommandEntry();

    [GeneratedRegex(@"^\s+(-.*?)(?:\s{2,}|$)")]
    private static partial Regex OptionDeclaration();

    [GeneratedRegex(@"(?<![\w-])--?[A-Za-z][\w-]*")]
    private static partial Regex OptionName();

    [GeneratedRegex(@"^#{1,6} +`(specht(?: [^`]*)?)` *$")]
    private static partial Regex SectionHeading();

    [GeneratedRegex(@"^\s*(?:\||[-*])\s*`(-[^`]*)`")]
    private static partial Regex OptionEntry();

    [GeneratedRegex(@"^(?:dotnet (?:tool run )?)?specht(?:\s+(.*))?$")]
    private static partial Regex CommandLine();

    [GeneratedRegex(@"^[a-z][a-z-]*$")]
    private static partial Regex CommandName();

    [GeneratedRegex(@"`([^`]+)`")]
    private static partial Regex CodeSpan();

    [GeneratedRegex(@"\]\(docs/cli\.md(?:#[^)]*)?\)")]
    private static partial Regex ReferenceLink();

    private IEnumerable<string> Unlisted(string page)
    {
        var anywhere = Surface.Values.SelectMany(static options => options.SelectMany(static option => option)).ToHashSet(StringComparer.Ordinal);
        foreach (var code in Code(Read(page)))
        {
            if (CommandLine().Match(code) is { Success: true } line)
            {
                var typed = line.Groups[1].Value;
                var first = typed.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                var command = Surface.Keys
                    .Where(candidate => candidate == DefaultCommand || typed == candidate[(DefaultCommand.Length + 1)..]
                        || typed.StartsWith(candidate[(DefaultCommand.Length + 1)..] + " ", StringComparison.Ordinal))
                    .OrderByDescending(static candidate => candidate.Length)
                    .First();
                if (command == DefaultCommand && first is not null && CommandName().IsMatch(first))
                {
                    command = $"{DefaultCommand} {first}";
                }
                if (!Surface.TryGetValue(command, out var options))
                {
                    yield return command;
                    continue;
                }

                foreach (var name in Names(typed).Except(options.SelectMany(static option => option)))
                {
                    yield return $"{command} {name}";
                }
            }
            else if (code.StartsWith('-'))
            {
                foreach (var name in Names(code).Where(name => !anywhere.Contains(name)))
                {
                    yield return name;
                }
            }
        }
    }

    private const string Reference = "docs/cli.md";
    private const string DefaultCommand = "specht";

    private static readonly string[] SpecifiedCommands = ["specht upgrade", "specht manifest", "specht rules"];

    private Dictionary<string, string[][]>? _surface;
    private string? _page;
    private string[] _unbuilt = [];
}
