using AwesomeAssertions;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.RepresentationModel;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class SettingsChecklistIntegrationTests
{
    [Fact]
    public void B001_Checklist_WhenRead_ShouldRequireAPullRequestBeforeAnyChangeReachesMain()
    {
        // Given
        var settings = Settings();

        // When
        var pullRequest = settings.GetValueOrDefault("ruleset.pull_request");

        // Then
        pullRequest.Should().Be("true");
    }

    [Fact]
    public void B002_ChecklistAndIntegrationWorkflow_WhenCompared_ShouldRequireExactlyTheChecksTheWorkflowProduces()
    {
        // Given
        var produced = ChecksTheIntegrationWorkflowProduces();

        // When
        var required = RequiredChecks()
            .Where(static row => row.Source == "GitHub Actions")
            .Select(static row => row.Check)
            .ToArray();

        // Then
        required.Should().OnlyHaveUniqueItems();
        required.Should().BeEquivalentTo(produced);
    }

    [Fact]
    public void B003_Checklist_WhenRead_ShouldRequireNoCoverageStatus()
    {
        // Given
        var checks = RequiredChecks().Select(static row => row.Check).ToArray();

        // When
        var coverage = checks.Where(static check => check is "codecov/patch" or "codecov/project");

        // Then
        coverage.Should().BeEmpty();
    }

    [Fact]
    public void B004_Checklist_WhenRead_ShouldEnableSquashMergingWithTheCommitMessagesAsTheBody()
    {
        // Given
        var settings = Settings();

        // When
        var squash = (settings.GetValueOrDefault("allow_squash_merge"), settings.GetValueOrDefault("squash_merge_commit_message"));

        // Then
        squash.Should().Be(("true", "COMMIT_MESSAGES"));
    }

    [Fact]
    public void B005_Checklist_WhenRead_ShouldEnableRebaseMergingAndDisableMergeCommits()
    {
        // Given
        var settings = Settings();

        // When
        var merging = (settings.GetValueOrDefault("allow_rebase_merge"), settings.GetValueOrDefault("allow_merge_commit"));

        // Then
        merging.Should().Be(("true", "false"));
    }

    [Fact]
    public void B006_Checklist_WhenRead_ShouldEnableAutoMerge()
    {
        // Given
        var settings = Settings();

        // When
        var autoMerge = settings.GetValueOrDefault("allow_auto_merge");

        // Then
        autoMerge.Should().Be("true");
    }

    [Fact]
    public void B011_Checklist_WhenRead_ShouldRequireTheChecksToPassAndNoApprovingReview()
    {
        // Given
        var settings = Settings();

        // When
        var gate = (
            settings.GetValueOrDefault("ruleset.required_status_checks"),
            settings.GetValueOrDefault("ruleset.pull_request.required_approving_review_count"));

        // Then
        gate.Should().Be(("true", "0"));
    }

    private static Dictionary<string, string> Settings()
    {
        var rows = Tables(SettingsHeader).SelectMany(static table => table).ToArray();
        rows.Select(static row => row[1]).Should().OnlyHaveUniqueItems("a Field appears once in the checklist");
        return rows.ToDictionary(static row => row[1], static row => row[2]);
    }

    private static (string Check, string Source, string IntegrationId)[] RequiredChecks()
    {
        var tables = Tables(RequiredChecksHeader);
        tables.Should().ContainSingle("the checklist holds one required-checks table");
        return tables[0].Select(static row => (row[0], row[1], row[2])).ToArray();
    }

    private static List<string[][]> Tables(string[] header)
    {
        var path = Path.Combine(FindRepositoryRoot(), ".github", "settings", "checklist.md");
        File.Exists(path).Should().BeTrue("the settings checklist is .github/settings/checklist.md");
        var pipeline = new MarkdownPipelineBuilder().UseYamlFrontMatter().UsePipeTables().Build();
        var document = Markdown.Parse(File.ReadAllText(path), pipeline);

        return document.Descendants<Table>()
            .Select(static table => table.OfType<TableRow>().Select(static row => row.OfType<TableCell>().Select(Cell).ToArray()).ToArray())
            .Where(rows => rows.Length > 0 && rows[0].SequenceEqual(header))
            .Select(static rows => rows[1..])
            .ToList();
    }

    private static string Cell(TableCell cell)
    {
        var code = cell.Descendants<CodeInline>().FirstOrDefault();
        if (code is not null)
        {
            return code.Content;
        }

        return string.Concat(cell.Descendants<LiteralInline>().Select(static literal => literal.Content.ToString())).Trim();
    }

    private static string[] ChecksTheIntegrationWorkflowProduces()
    {
        var path = Path.Combine(FindRepositoryRoot(), ".github", "workflows", "ci.yml");
        var stream = new YamlStream();
        using var input = File.OpenText(path);
        stream.Load(input);
        var jobs = (YamlMappingNode)((YamlMappingNode)stream.Documents[0].RootNode)["jobs"];

        return jobs.Children.Select(static job => Check(job.Key.ToString(), (YamlMappingNode)job.Value)).ToArray();
    }

    private static string Check(string key, YamlMappingNode job)
    {
        if (job.Children.TryGetValue("strategy", out var strategy))
        {
            ((YamlMappingNode)strategy).Children.Keys.Select(static axis => axis.ToString())
                .Should().NotContain("matrix", $"job '{key}' has a matrix, and the file alone does not settle its check names");
        }

        var name = job.Children.TryGetValue("name", out var named) ? named.ToString() : key;
        name.Should().NotContain("${{", $"job '{key}' names its check with an expression, and the file alone does not settle it");
        return name;
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "specht.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    private static readonly string[] SettingsHeader = ["Setting", "Field", "Value"];

    private static readonly string[] RequiredChecksHeader = ["Check", "Source", "Integration id"];
}
