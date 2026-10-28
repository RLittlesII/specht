using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.RuleSetting;

public sealed partial class RuleSettingSteps
{
    [Given("a snapshot of every file under the root")]
    public void GivenSnapshot() => _filesBefore = Snapshot();

    [Given("a root with no manifest")]
    public void GivenNoManifest() => File.Delete(ManifestPath);

    [Given("a root path that is not a directory")]
    public void GivenMissingRoot() => _root = "missing-root";

    [Given("a root whose manifest (.*)")]
    public void GivenRejected(string reason)
    {
        var text = reason switch
        {
            "is not well-formed" => "{",
            "carries a key the tool does not know" => "{\"unknown\":true}",
            "pins a version the tool does not ship" => "{\"schemaVersion\":\"9.0.0\"}",
            _ => throw new ArgumentException("Unknown rejection case.", nameof(reason)),
        };
        Tree.WriteRaw(SpecTree.ManifestPath, text);
    }

    [When("the out-of-order-section rule is set to {word} against that root")]
    public void WhenAgainstRoot(string level) => Run("SPEC010", level);

    [When("the out-of-order-section rule is set to a level named {word}")]
    [When("the out-of-order-section rule is set to a level named {word} against that root")]
    public void WhenInvalidLevel(string level) => Run("SPEC010", level);

    [When("a rule that version 0.1.0 does not hold is set to {word}")]
    [When("a rule that version 0.1.0 does not hold is set to {word} against that root")]
    public void WhenUnknownRule(string level) => Run("SPEC035", level);

    [When("the out-of-order-section rule is set to warning against that root given as an absolute path")]
    public void WhenAbsoluteRoot()
    {
        _root = Tree.Root;
        Run("SPEC010", "warning");
    }

    [Then("the exit code is {int}")]
    public void ThenExitCode(int code) => _result.ExitCode.Should().Be(code, _result.Stderr);

    [Then("the standard error names that rule")]
    public void ThenNamesRule()
    {
        _result.Stdout.Should().BeEmpty();
        _result.Stderr.Should().Contain("SPEC035");
    }

    [Then("the standard error names loud and the three levels")]
    public void ThenNamesLevels()
    {
        _result.Stdout.Should().BeEmpty();
        _result.Stderr.Should().Contain("loud").And.Contain("error").And.Contain("warning").And.Contain("off");
    }

    [Then("no file under the root has changed")]
    public void ThenFilesUnchanged() => Snapshot().Should().BeEquivalentTo(_filesBefore);

    [Then("no file under the root other than the manifest was created, modified or deleted")]
    public void ThenOtherFilesUnchanged()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        var after = Snapshot();
        after.Remove(SpecTree.ManifestPath);
        _filesBefore.Remove(SpecTree.ManifestPath);
        after.Should().BeEquivalentTo(_filesBefore);
    }

    [Then("the manifest still pins schema version {word}")]
    public void ThenPin(string pin) => ReadManifest()["schemaVersion"]!.GetValue<string>().Should().Be(pin);

    [Then("nothing is written")]
    public void ThenNothingWritten() => Directory.Exists(Path.Combine(Tree.Root, _root)).Should().BeFalse();

    [Then("no manifest is written")]
    public void ThenNoManifest() => File.Exists(ManifestPath).Should().BeFalse();

    [Then("every path the output names other than those typed is relative to the root and uses forward slashes")]
    public void ThenRelativePaths()
    {
        _result.ExitCode.Should().Be(2, _result.Stderr);
        _result.Stdout.Should().BeEmpty();
        _result.Stderr.Should().Contain(SpecTree.ManifestPath).And.NotContain(Tree.Root).And.NotContain("\\");
    }

    private void Run(string id, string level) => _result = AcceptanceTool.Launch(Tree.Root, "rules", "set", id, level, "--root", _root);

    private Dictionary<string, string> Snapshot() => Directory.GetFiles(Tree.Root, "*", SearchOption.AllDirectories)
        .ToDictionary(path => Path.GetRelativePath(Tree.Root, path).Replace('\\', '/'), static path => Convert.ToBase64String(File.ReadAllBytes(path)));

    private string _root = ".";
    private Dictionary<string, string> _filesBefore = [];
    private (string Stdout, string Stderr, int ExitCode) _result;
}
