using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Check;

public sealed partial class CheckSteps
{
    [Given("the tool has been published as a package")]
    public void GivenTheToolHasBeenPublishedAsAPackage()
    {
        if (OperatingSystem.IsWindows())
        {
            runtime.TestIgnore("The build target is reached through build.sh, which does not run on Windows.");
        }

        CopyFromRepository(Listed("--cached", "--others", "--exclude-standard"));
        _source = Succeed(Tree.Root, "pwd", "-P").TrimEnd('\n');
        File.WriteAllText(Path.Combine(Source, "nuget.config"), """
            <configuration><packageSources><clear /></packageSources><auditSources><clear /></auditSources></configuration>
            """);
        Succeed(Source, "dotnet", "tool", "restore");
        _installed = RunProcess(Source, "dotnet", "tool", "run", "specht", "--", "--root", ".");
        _installed.Value.Stdout.Should().Contain("specifications:", _installed.Value.Stderr);
    }

    [Given("this repository's local tool manifest names that package")]
    public void GivenThisRepositorysLocalToolManifestNamesThatPackage()
    {
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(Source, ".config/dotnet-tools.json")))!;
        var tool = manifest["tools"]!["specht.tool"]!;
        tool["version"]!.GetValue<string>().Should().NotBeNullOrEmpty();
        tool["commands"]!.AsArray().Select(static command => command!.GetValue<string>()).Should().Equal("specht");
    }

    [Given("this repository's tool source cannot be built")]
    public void GivenThisRepositorysToolSourceCannotBeBuilt() =>
        File.WriteAllText(Path.Combine(Source, "src/tool/Program.cs"), "#error The self-check must use the installed package.\n");

    [Then("it reaches the tool through the local tool manifest")]
    public void ThenItReachesTheToolThroughTheLocalToolManifest()
    {
        var installed = _installed ?? throw new InvalidOperationException("The installed tool did not run.");
        Carried(Target.Stdout).Should().Equal(Printed(installed.Stdout), Target.Stdout + Target.Stderr);
        Target.ExitCode.Should().Be(installed.ExitCode, Target.Stdout + Target.Stderr);
    }

    [Then("it does not build the tool from this repository's source")]
    public void ThenItDoesNotBuildTheToolFromThisRepositorysSource() =>
        Directory.Exists(Path.Combine(Source, "src/tool/bin")).Should().BeFalse(Target.Stdout + Target.Stderr);

    private (string Stdout, string Stderr, int ExitCode)? _installed;
}
