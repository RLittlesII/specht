using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Rules;

public sealed partial class RuleEntriesSteps
{
    [Given("a root path that is not a directory")]
    public void GivenMissingRoot() => _rootArgument = "missing-root";

    [Given("a root with no manifest")]
    public void GivenNoManifest() => File.Delete(Path.Combine(Tree.Root, SpecTree.ManifestPath));

    [Given("a root whose manifest (.*)")]
    public void GivenRejectedManifest(string reason)
    {
        var path = Path.Combine(Tree.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        switch (reason)
        {
            case "is not well-formed":
                File.WriteAllText(path, "{");
                return;
            case "sets a rule the pinned version does not hold":
                manifest["rules"] = new JsonObject { ["SPEC999"] = "off" };
                break;
            case "pins a version the tool does not ship":
                manifest["schemaVersion"] = "7.0.0";
                break;
            case "pins a malformed version":
                manifest["schemaVersion"] = "0.1";
                break;
            case "records an upstream schema source and explicitly selects the embedded source":
                manifest["schemaVersion"] = "0.2.0";
                manifest["schemaSource"] = "embedded";
                manifest["spechtSchema"] = JsonNode.Parse("""
                    {
                      "location":"https://schemas.example.test/", "version":"fixture",
                      "hashes": {
                        "feature":"sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                        "task":"sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",
                        "epic":"sha256:cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc"
                      }
                    }
                    """);
                break;
            default:
                throw new ArgumentException("Unrecognised rejection example.", nameof(reason));
        }
        File.WriteAllText(path, manifest.ToJsonString());
    }

    [Given("a snapshot of every file under the root")]
    public void GivenFileSnapshot() => _filesBefore = Snapshot();

    [When("the rules are listed against that root")]
    public void WhenRulesAreListedAgainstRoot() =>
        _result = AcceptanceTool.Launch(Tree.Root, "rules", "--root", _rootArgument);

    [When("the rules are listed against that root given as an absolute path")]
    public void WhenRootIsAbsolute() => _result = AcceptanceTool.Launch(Tree.Root, "rules", "--root", Tree.Root);

    [When("the rules are listed twice")]
    public void WhenRulesAreListedTwice()
    {
        WhenRulesAreListed();
        _secondResult = AcceptanceTool.Launch(Tree.Root, "rules");
    }

    [Then("the listing is the standard output")]
    [Then("the standard output carries only the listing")]
    public void ThenStdoutIsListing()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        _result.Stderr.Should().BeEmpty();
        Entries.Should().HaveCount(_version!.RuleIds.Count);
        Entries.Should().OnlyContain(static fields => fields.Length == 4);
        Entries.Select(static fields => fields[0]).Should().BeEquivalentTo(_version.RuleIds);
    }

    [Then("the exit code is {int}")]
    public void ThenExitCode(int expected) => _result.ExitCode.Should().Be(expected, _result.Stderr);

    [Then("the standard output is empty")]
    public void ThenStdoutEmpty() => _result.Stdout.Should().BeEmpty();

    [Then("the standard error names the root as it was given")]
    public void ThenStderrNamesRoot() => _result.Stderr.Should().Contain(_rootArgument);

    [Then("the standard error names the manifest path")]
    public void ThenStderrNamesManifest() => _result.Stderr.Should().Contain(SpecTree.ManifestPath);

    [Then("the standard error names what the check names for that manifest")]
    public void ThenStderrMatchesCheck()
    {
        var check = AcceptanceTool.Launch(Tree.Root, "--root", _rootArgument);
        check.ExitCode.Should().Be(3, check.Stderr);
        _result.Stderr.Should().Be(check.Stderr);
    }

    [Then("no file under the root was created, modified or deleted")]
    public void ThenFilesUnchanged()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        Snapshot().Should().BeEquivalentTo(_filesBefore);
    }

    [Then("the manifest is byte-for-byte as it was")]
    public void ThenManifestUnchanged()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        File.ReadAllBytes(Path.Combine(Tree.Root, SpecTree.ManifestPath)).Should().Equal(_manifestBefore);
    }

    [Then("every path the output names other than those typed is relative to the root and uses forward slashes")]
    public void ThenPathsAreRelative()
    {
        _result.ExitCode.Should().Be(2, _result.Stderr);
        _result.Stdout.Should().BeEmpty();
        _result.Stderr.Should().Contain(SpecTree.ManifestPath).And.NotContain(Tree.Root).And.NotContain("\\");
    }

    [Then("both runs print the same standard output and exit with the same code")]
    public void ThenRunsMatch()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        _secondResult.Stdout.Should().Be(_result.Stdout);
        _secondResult.ExitCode.Should().Be(_result.ExitCode);
    }

    [Then("the out-of-order-section rule is on one line holding its rule id, family, default severity and effective level in that order")]
    public void ThenTextFieldsAreOrdered() =>
        _result.Stdout.Split('\n').Select(static line => line.TrimEnd('\r')).Should()
            .ContainSingle(static line => line == "SPEC010\tStructure\terror\twarning");

    [Then("the default severity and the effective level are in lower case")]
    public void ThenLevelsAreLowerCase()
    {
        Entries.Should().NotBeEmpty();
        Entries.Should().OnlyContain(static fields => fields.Length == 4);
        foreach (var fields in Entries)
        {
            fields[2].Should().BeOneOf("error", "warning", "off");
            fields[3].Should().BeOneOf("error", "warning", "off");
        }
    }

    private Dictionary<string, string> Snapshot() => Directory.GetFiles(Tree.Root, "*", SearchOption.AllDirectories)
        .ToDictionary(path => Path.GetRelativePath(Tree.Root, path).Replace('\\', '/'), static path => Convert.ToBase64String(File.ReadAllBytes(path)));

    private string _rootArgument = ".";
    private Dictionary<string, string> _filesBefore = [];
    private (string Stdout, string Stderr, int ExitCode) _secondResult;
}
