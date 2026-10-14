using System;
using System.IO.Abstractions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;
using Spectre.Console.Testing;

namespace Specht.Acceptance.Versioning;

public sealed partial class VersioningSteps
{
    [Given("an interactive terminal")]
    public void GivenAnInteractiveTerminal() => _interactive = true;

    [Given("there is no interactive terminal")]
    public void GivenThereIsNoInteractiveTerminal() => _interactive = false;

    [When("specht schema pin runs without a version")]
    public void WhenPinRunsWithoutAVersion()
    {
        if (_interactive)
        {
            WhenPinIsConfirmed("0.2.0");
            return;
        }
        _filesBefore ??= SnapshotFiles();
        (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Tree.Root, "schema", "pin");
    }

    [When("specht schema pin runs without a version and {word} is confirmed with Enter")]
    public void WhenPinIsConfirmed(string version)
    {
        _filesBefore ??= SnapshotFiles();
        var console = new TestConsole().Width(100);
        console.Profile.Capabilities.Interactive = _interactive;
        foreach (var choice in EngineServices.Versions().Versions)
        {
            if (choice.Number.ToString() == version)
            {
                break;
            }
            console.Input.PushKey(ConsoleKey.DownArrow);
        }
        console.Input.PushKey(ConsoleKey.Enter);
        var result = EngineServices.CommandApp(new FileSystem(), console).Run("schema", "pin", "--root", Tree.Root);
        (_stdout, _stderr, _exitCode) = (result.Output, string.Empty, result.ExitCode);
    }

    [Then("versions {word} and {word} are offered as a single-choice list navigated with the arrow keys")]
    public void ThenTheVersionsAreOffered(string first, string second) =>
        _stdout.Should().Contain("Select schema version").And.Contain(first).And.Contain(second);

    [Then("the standard error suggests supplying a version")]
    public void ThenTheErrorSuggestsSupplyingAVersion() => _stderr.Should().Contain("specht schema pin <version>");

    private bool _interactive;
}
