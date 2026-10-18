using System.IO.Compression;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Check;

public sealed partial class CheckSteps
{
    [Given("a checkout whose working schema the published consumer pin cannot read")]
    public void GivenAWorkingCheckout()
    {
        GivenThisRepositorysOwnTree();
        _consumerManifest = File.ReadAllBytes(Path.Combine(Source, ".config/dotnet-tools.json"));
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(Source, ".specht/manifest.json")))!.AsObject();
        manifest.Should().ContainKey("tests").And.ContainKey("owners");
    }

    [Given("the checkout's checker source cannot compile")]
    public void GivenTheCheckoutCannotCompile() =>
        File.WriteAllText(Path.Combine(Source, "src/tool/Program.cs"), "#error self-check test compiler failure\n");

    [Given("its self-check has already installed the working package")]
    public void GivenAnEarlierWorkingPackage()
    {
        WhenTheBuildsSpechtTargetRuns();
        ThenTheWorkingPackageIsInstalled();
        _firstVersion = InstalledVersion();
    }

    [When("the checker source changes without a version change and the self-check runs again")]
    public void WhenWorkingSourceChanges()
    {
        File.WriteAllText(Path.Combine(Source, "src/tool/Program.cs"), "System.Console.WriteLine(\"working-checker-edited\");\nreturn 1;\n");
        WhenTheBuildsSpechtTargetRuns();
    }

    [Then("it invokes the package built from that checkout through an isolated local manifest")]
    public void ThenTheWorkingPackageIsInstalled()
    {
        File.Exists(Path.Combine(Source, ".artifacts/self-check/.config/dotnet-tools.json"))
            .Should().BeTrue("the self-check must install the working package, " + Target.Stdout + Target.Stderr);
        var package = Directory.GetFiles(Path.Combine(Source, ".artifacts/self-check/feed"), "*.nupkg").Should().ContainSingle().Subject;
        using var zip = ZipFile.OpenRead(package);
        using var metadata = zip.Entries.Single(static entry => entry.FullName.EndsWith(".nuspec", StringComparison.Ordinal)).Open();
        var version = XDocument.Load(metadata).Descendants().Single(static element => element.Name.LocalName == "version").Value;
        InstalledVersion().Should().Be(version);
        LaunchInstalled();
        _exitCode.Should().Be(0, _stdout + _stderr);
        Carried(Target.Stdout).Should().Equal(Printed(_stdout), Target.Stdout + Target.Stderr);
        Target.ExitCode.Should().Be(_exitCode, Target.Stdout + Target.Stderr);
    }

    [Then("the committed consumer tool manifest is unchanged")]
    public void ThenTheConsumerPinIsUnchanged() =>
        File.ReadAllBytes(Path.Combine(Source, ".config/dotnet-tools.json")).Should().Equal(_consumerManifest!);

    [Then("preparation fails before any checker runs")]
    public void ThenPreparationFails()
    {
        Target.ExitCode.Should().NotBe(0);
        (Target.Stdout + Target.Stderr).Should().Contain("self-check test compiler failure");
        Carried(Target.Stdout).Should().NotContain(static line => line.StartsWith("specifications:", StringComparison.Ordinal));
        File.Exists(Path.Combine(Source, ".artifacts/self-check/.config/dotnet-tools.json")).Should().BeFalse();
    }

    [Then("the changed checker is the one that runs")]
    public void ThenTheChangedCheckerRuns()
    {
        InstalledVersion().Should().Be(_firstVersion, "the source edit does not change the computed package version");
        Carried(Target.Stdout).Should().Equal("working-checker-edited");
        Target.ExitCode.Should().Be(1);
    }

    private string InstalledVersion() => JsonNode.Parse(File.ReadAllText(
        Path.Combine(Source, ".artifacts/self-check/.config/dotnet-tools.json")))!["tools"]!["specht.tool"]!["version"]!.GetValue<string>();

    private byte[]? _consumerManifest;
    private string? _firstVersion;
}
