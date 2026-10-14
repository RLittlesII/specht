using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;
using Specht.Tool.Features.Schema.List;
using Specht.Versioning;
using Spectre.Console.Testing;

namespace Specht.Acceptance.Versioning;

public sealed partial class VersioningSteps
{
    [When("the shipped versions are listed")]
    public async Task WhenTheShippedVersionsAreListed()
    {
        if (_shipped is null)
        {
            (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Tree.Root, "schema", "list");
            return;
        }
        var versions = new SchemaVersions(_shipped.Values);
        var console = new TestConsole();
        using var error = new StringWriter();
        var command = new SchemaListCommand(console, EngineServices.Commands(versions), versions, error);
        _exitCode = await command.ExecuteAsync(null!, new SchemaListCommand.Settings { Root = Tree.Root }, CancellationToken.None);
        _stdout = console.Output;
        _stderr = error.ToString();
    }

    [When("schema versions are listed against a path that is not a directory")]
    public void WhenSchemaVersionsAreListedAgainstAMissingRoot() =>
        (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Tree.Root, "schema", "list", "--root", "no-such-root");

    [Then("the standard output is 0.1.0 and 0.2.0, one to a line")]
    public void ThenEachShippedVersionIsPrintedOnItsOwnLine() =>
        _stdout.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Should().Equal(Shipped.Keys.Order().Select(static version => version.ToString()));

    [Then("0.9.0 is printed before 0.10.0")]
    public void ThenVersionsArePrintedInSemverOrder() =>
        _stdout.IndexOf("0.9.0", StringComparison.Ordinal).Should().BeLessThan(_stdout.IndexOf("0.10.0", StringComparison.Ordinal));

    [Then("version {word} is marked as the pinned one")]
    public void ThenVersionIsMarkedAsPinned(string version) => _stdout.Should().Contain(version + " (pinned)");

    [Then("version {word} is not marked")]
    public void ThenVersionIsNotMarked(string version) => _stdout.Should().NotContain(version + " (pinned)");

    [Then("no version is marked")]
    public void ThenNoVersionIsMarked() => _stdout.Should().NotContain("(pinned)");

    [Then("the standard error suggests pinning a shipped version")]
    public void ThenTheErrorSuggestsPinning() => _stderr.Should().Contain("specht schema pin <version>");
}
