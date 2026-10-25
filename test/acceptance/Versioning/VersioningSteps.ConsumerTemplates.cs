using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Versioning;

public sealed partial class VersioningSteps
{
    [Given("the report the check gives while the manifest names no template as the consumer's")]
    public void GivenTheReportTheCheckGivesWhileTheManifestNamesNoTemplate()
    {
        WhenTheCheckRuns();
        _baselineStdout = _stdout;
        _baselineExitCode = _exitCode;
    }

    [When("the manifest names the feature template as the consumer's and the check runs again")]
    public void WhenTheManifestNamesTheFeatureTemplateAndTheCheckRunsAgain()
    {
        EditManifest(static manifest => manifest["consumerTemplates"] = new JsonArray("feature.md"));
        WhenTheCheckRuns();
    }

    [Then("the report is the same")]
    public void ThenTheReportIsTheSame()
    {
        _exitCode.Should().Be(_baselineExitCode, _stderr);
        _stdout.Should().Be(_baselineStdout);
    }

    private string _baselineStdout = string.Empty;
    private int _baselineExitCode = -1;
}
