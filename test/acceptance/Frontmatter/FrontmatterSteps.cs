using System.Security.Cryptography;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Frontmatter;

/// <summary>Runs every standalone frontmatter scenario through the built tool's file/stdin boundary.</summary>
[Binding]
[Scope(Feature = "The frontmatter command")]
public sealed class FrontmatterSteps
{
    [Given("a repository root holding a manifest that pins schema version 0.1.0")]
    public void GivenAPinnedRoot()
    {
        _tree = new SpecTree();
        Tree.WriteRaw(SpecTree.ManifestPath, "{\"schemaVersion\":\"0.1.0\"}");
    }

    [Given("a task document outside the tree whose frontmatter lacks a required key")]
    public void GivenADocumentOutsideTheTree()
    {
        MissingKey("task");
        _path = "../outside.md";
        Tree.WriteRaw(_path, _document);
    }

    [Given("a {word} document whose frontmatter lacks a required key")]
    public void MissingKey(string kind = "task") => _document = Document(kind).Replace("priority: med\n", string.Empty, StringComparison.Ordinal);

    [Given("a task document on standard input whose frontmatter lacks a required key")]
    public void GivenAMissingKeyOnStdin() => MissingKey();

    [Given("a task document whose creation date on its fifth line is not a date")]
    public void GivenAnInvalidDate() => _document = TaskDocument.Replace("created: '2026-10-01'", "created: not-a-date", StringComparison.Ordinal);

    [Given("a document with no frontmatter")]
    public void GivenNoFrontmatter() => _document = "# A document\n";

    [Given("a task document the embedded schema accepts and a stricter on-disk schema rejects")]
    public void GivenAStricterSchema()
    {
        _document = TaskDocument;
        Tree.WriteRaw(SpecTree.ItemSchemaPath, """{"type":"object","required":["additionalKey"]}""");
    }

    [Given("the root's manifest selects that on-disk schema under version 0.2.0")]
    public void GivenTheSelectedDiskSchema() =>
        Tree.WriteRaw(SpecTree.ManifestPath, """{"schemaVersion":"0.2.0","schemaSource":"disk","identifiers":{"epic":"^[0-9]{4}$"}}""");

    [Given("a task document whose valid frontmatter depends on a file that does not exist")]
    public void GivenAnUnresolvedDependency() =>
        _document = TaskDocument.Replace("github_issue: null", "github_issue: null\ndepends_on: ['missing.md']", StringComparison.Ordinal);

    [Given("a feature document with valid frontmatter and none of the twelve sections")]
    public void GivenAFeatureWithoutSections() => _document = Document("feature").Split("\n# ", 2)[0];

    [Given("the root holds a specification whose frontmatter fails its schema")]
    public void GivenAnInvalidRootSpecification() =>
        Tree.WriteFeature("0001", "F1", new Dictionary<string, string>
        {
            ["priority"] = "wrong",
            ["github_issue"] = "1",
            ["synced_at"] = "\"2026-10-01T12:00:00Z\"",
        });

    [Given("a task document with valid frontmatter")]
    public void GivenAValidTask() => _document = TaskDocument;

    [Given("the root holds a specification whose frontmatter fails its schema on two keys")]
    public void GivenTwoInvalidFeatureKeys()
    {
        var path = Tree.WriteFeature("0001", "F1",
            new Dictionary<string, string>
            {
                ["priority"] = "wrong",
                ["created"] = "not-a-date",
                ["github_issue"] = "1",
                ["synced_at"] = "\"2026-10-01T12:00:00Z\"",
            });
        _path = Path.GetRelativePath(Tree.Root, path).Replace('\\', '/');
        _document = File.ReadAllText(path);
        _expected = [.. Tree.Run().Violations.Where(v => v.File == _path && v.RuleId == "SPEC002").Select(static v => v.ToString())];
    }

    [Given(@"^a task document at drafts/issue\.md whose frontmatter lacks a required key$")]
    public void GivenANamedTask() => MissingKey();

    [Given("a task document whose frontmatter produces two schema failures")]
    public void GivenTwoSchemaFailures() => GivenAnInvalidDate();

    [Given("a root path that is not a directory")]
    public void GivenAMissingRoot() => _root = "absent";

    [Given("a root with no manifest")]
    public void GivenNoManifest() => File.Delete(Path.Combine(Tree.Root, SpecTree.ManifestPath));

    [Given("a root whose manifest is not well-formed")]
    public void GivenAMalformedManifest() => Tree.WriteRaw(SpecTree.ManifestPath, "{");

    [Given("a root whose manifest pins a version the tool does not ship")]
    public void GivenAnUnshippedPin() => Tree.WriteRaw(SpecTree.ManifestPath, "{\"schemaVersion\":\"99.0.0\"}");

    [Given(@"^no file at drafts/missing\.md$")]
    public void GivenNoDocument() => _path = "drafts/missing.md";

    [Given("a snapshot of every file under the root")]
    public void GivenASnapshot() => _snapshot = Snapshot();

    [Given("a root with a manifest")]
    [Given(@"^a root with a manifest and no file at drafts/missing\.md$")]
    public void GivenAManifest() => File.Exists(Path.Combine(Tree.Root, SpecTree.ManifestPath)).Should().BeTrue();

    [Given(@"^a root path that is not a directory and no file at drafts/missing\.md$")]
    public void GivenSeveralMissingInputs() => GivenAMissingRoot();

    [When("its frontmatter is checked as a {word}")]
    [When("the task document's frontmatter is checked as a {word}")]
    public void WhenChecked(string kind) => Launch("--kind", kind, "--stdin");

    [When("its frontmatter is checked as a task by its path")]
    [When(@"^its frontmatter is checked as a task by the path drafts/issue\.md$")]
    public void WhenCheckedByPath()
    {
        Tree.WriteRaw(_path, _document);
        Launch("--kind", "task", _path);
    }

    [When("its frontmatter is checked as a task from standard input")]
    public void WhenCheckedOnStdin() => WhenChecked("task");

    [When("that specification's frontmatter is checked as a feature")]
    public void WhenTheSpecificationIsChecked() => Launch("--kind", "feature", _path);

    [When("a task document's frontmatter is checked against that root")]
    public void WhenCheckedAgainstTheRoot() => WhenChecked("task");

    [When("its frontmatter is checked as a kind named story")]
    public void WhenAnUnknownKindIsChecked() => WhenChecked("story");

    [When(@"^the frontmatter at drafts/missing\.md is checked as a task$")]
    public void WhenAMissingDocumentIsChecked() => Launch("--kind", "task", "drafts/missing.md");

    [When("a task document's frontmatter is checked against that root given as an absolute path")]
    public void WhenAnAbsoluteRootIsUsed()
    {
        _root = Tree.Root;
        WhenChecked("task");
    }

    [When("its frontmatter is checked as a task twice")]
    public void WhenCheckedTwice()
    {
        WhenChecked("task");
        _first = _result;
        WhenChecked("task");
    }

    [When("the command is run (.*)")]
    public void WhenTheCommandIsRun(string how)
    {
        string[] args = how switch
        {
            "with no kind" => ["--stdin"],
            "with a kind and neither a path nor the pipe" => ["--kind", "task"],
            "with a kind and both a path and the pipe" => ["--kind", "task", "drafts/issue.md", "--stdin"],
            "with a kind named story and neither a path nor the pipe" => ["--kind", "story"],
            "with a kind named story and the path drafts/missing.md" => ["--kind", "story", "drafts/missing.md"],
            "with a task kind and the path drafts/missing.md" => ["--kind", "task", "drafts/missing.md"],
            "with a kind named story and a path naming a file" => ["--kind", "story", "drafts/issue.md"],
            _ => throw new InvalidOperationException(how),
        };
        Launch(args);
    }

    [Then("one violation is reported for the missing key")]
    public void ThenAMissingKeyIsReported() => Diagnostics().Should().ContainSingle().Which.Should().Contain("priority");

    [Then("the violation carries rule id {word}")]
    public void ThenTheRuleIsReported(string rule) => Diagnostics().Should().ContainSingle().Which.Should().Contain($": error {rule}:");

    [Then("the violation is reported at line {int}")]
    public void ThenTheLineIsReported(int line) => Diagnostics().Should().ContainSingle(static l => l.EndsWith("[created]", StringComparison.Ordinal))
        .Which.Should().Contain($"({line}):");

    [Then("the violation carries rule id SPEC001 at line 1")]
    public void ThenNoFrontmatterIsReported() => Diagnostics().Should().ContainSingle().Which.Should().Contain("(1): error SPEC001:");

    [Then("a violation is reported")]
    public void ThenAViolationIsReported() => Diagnostics().Should().NotBeEmpty();

    [Then("no violation is reported")]
    public void ThenNoViolationIsReported() => Diagnostics().Should().BeEmpty();

    [Then("the violations are the frontmatter violations the check reports for that file")]
    public void ThenTheVerdictsMatch() => Diagnostics().Should().Equal(_expected);

    [Then("the standard output carries one line naming the document, the line, the severity, the rule id and the message in the build's diagnostic form")]
    public void ThenADiagnosticLineIsPrinted() => Regex.IsMatch(Diagnostics().Should().ContainSingle().Which,
        @"^<stdin>\(\d+\): error SPEC003: .+priority.+$").Should().BeTrue();

    [Then(@"^the violation line names drafts/issue\.md$")]
    public void ThenTheTypedPathIsPrinted() => Diagnostics().Should().ContainSingle().Which.Should().StartWith("drafts/issue.md(");

    [Then("the violation line names <stdin>")]
    public void ThenStdinIsPrinted() => Diagnostics().Should().ContainSingle().Which.Should().StartWith("<stdin>(");

    [Then("the standard output ends with an error count of {int} and a warning count of {int}")]
    public void ThenTheCountsArePrinted(int errors, int warnings) => _result.Stdout.TrimEnd().Should().EndWith($"errors: {errors}, warnings: {warnings}");

    [Then("the exit code is {int}")]
    public void ThenTheCodeIsReturned(int code) => _result.ExitCode.Should().Be(code, _result.Stdout + _result.Stderr);

    [Then("the standard error names the root as it was given")]
    public void ThenTheTypedRootIsPrinted() => _result.Stderr.Should().Contain(_root);

    [Then("the standard error names the manifest path")]
    [Then("the standard error names the manifest path relative to the root")]
    public void ThenTheManifestIsPrinted() => _result.Stderr.Should().Contain(SpecTree.ManifestPath).And.NotContain(Tree.Root);

    [Then("the standard output is empty")]
    public void ThenStdoutIsEmpty() => _result.Stdout.Should().BeEmpty();

    [Then("the standard error names the pinned version and the versions the tool ships")]
    public void ThenVersionsArePrinted() => _result.Stderr.Should().Contain("99.0.0").And.Contain("0.1.0").And.Contain("0.2.0");

    [Then(@"^the standard error names (the usage|story and the three kinds|drafts/missing\.md)$")]
    public void ThenTheErrorNames(string what)
    {
        if (what == "the usage")
        {
            _result.Stderr.Should().Contain("specht frontmatter --kind");
        }
        else if (what == "story and the three kinds")
        {
            _result.Stderr.Should().Contain("story").And.Contain("feature").And.Contain("task").And.Contain("epic");
        }
        else
        {
            _result.Stderr.Should().Contain(what);
        }
    }

    [Then("the standard output carries only the violation line and the summary")]
    public void ThenOnlyTheVerdictIsPrinted()
    {
        _result.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries).Should().HaveCount(2);
        _result.Stderr.Should().BeEmpty();
    }

    [Then("no file under the root was created, modified or deleted")]
    public void ThenTheTreeIsUnchanged() => Snapshot().Should().BeEquivalentTo(_snapshot);

    [Then("every path the output names other than those typed is relative to the root and uses forward slashes")]
    public void ThenPathsAreRelative() =>
        _result.Stderr.Should().Contain(SpecTree.ManifestPath).And.NotContain(Tree.Root).And.NotContain("\\");

    [Then("both runs print the same standard output and exit with the same code")]
    public void ThenRunsAreDeterministic() => _result.Should().Be(_first);

    [AfterScenario]
    public void DisposeTree() => _tree?.Dispose();

    private SpecTree Tree => _tree ?? throw new InvalidOperationException("No root was prepared.");

    private string Document(string kind) => kind switch
    {
        "task" => TaskDocument,
        "feature" => File.ReadAllText(Tree.WriteFeature("0001", "F1", new Dictionary<string, string>
        {
            ["github_issue"] = "1",
            ["synced_at"] = "\"2026-10-01T12:00:00Z\"",
        })),
        "epic" => "---\nid: '0001'\ntype: epic\nstatus: ready\npriority: med\nmilestone: null\nchildren: []\n"
            + "created: '2026-10-01'\nupdated: '2026-10-01'\ngithub_issue: null\n---\n",
        _ => throw new InvalidOperationException(kind),
    };

    private void Launch(params string[] args)
    {
        string[] command = ["frontmatter", .. args, "--root", _root];
        _result = args.Contains("--stdin", StringComparer.Ordinal)
            ? AcceptanceTool.LaunchWithInput(Tree.Root, _document, command)
            : AcceptanceTool.Launch(Tree.Root, command);
    }

    private string[] Diagnostics() => [.. _result.Stdout.ReplaceLineEndings("\n").Split('\n')
        .Where(static line => line.Contains(": error SPEC", StringComparison.Ordinal) || line.Contains(": warning SPEC", StringComparison.Ordinal))];

    private Dictionary<string, string> Snapshot() => Directory.EnumerateFiles(Tree.Root, "*", SearchOption.AllDirectories)
        .ToDictionary(path => Path.GetRelativePath(Tree.Root, path).Replace('\\', '/'),
            static path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))));

    private const string TaskDocument = "---\nid: '0001-01'\nparent: '0001-F1'\ntype: task\ncreated: '2026-10-01'\n"
        + "status: ready\npriority: med\nupdated: '2026-10-01'\ngithub_issue: null\n---\n";

    private SpecTree? _tree;
    private string _document = TaskDocument;
    private string _path = "drafts/issue.md";
    private string _root = ".";
    private string[] _expected = [];
    private Dictionary<string, string>? _snapshot;
    private (string Stdout, string Stderr, int ExitCode) _result;
    private (string Stdout, string Stderr, int ExitCode) _first;
}
