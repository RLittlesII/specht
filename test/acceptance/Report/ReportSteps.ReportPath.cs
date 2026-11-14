using AwesomeAssertions;
using Reqnroll;
using Specht.Evaluation;
using Specht.Model;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Report;

/// <summary>
/// Steps for the report path (<c>0001-F3</c> B-003, B-004, B-022 to B-025, B-044 to B-046; decision 0002; item
/// <c>0036</c>): the built tool is launched with a relative <c>--report</c> path and a <c>--root</c> relative to its
/// working directory, which is the root itself unless B-046's scenario moves it outside, and the document the path must
/// hold is the engine's check of the same root, mapped and serialized before the launch. A root with no manifest or with
/// one that does not parse is a run that checks nothing (B-044, B-045): the engine is not run over it, and the earlier
/// run's document is another tree's.
/// </summary>
public sealed partial class ReportSteps
{
    [Given("the root holds specifications")]
    public void GivenTheRootHoldsSpecifications()
    {
        Tree.WriteFeature("0001", "F1");
        Tree.WriteFeature("0001", "F2");
    }

    [Given("the root holds a specification with one error-severity violation")]
    public void GivenTheRootHoldsASpecificationWithOneErrorSeverityViolation()
    {
        Tree.WriteFeature("0001", "F1", featureFile: null);
        Tree.Run().Violations.Should().ContainSingle().Which.Severity.Should().Be(SpechtSeverity.Error);
    }

    [Given("the root has no manifest")]
    public void GivenTheRootHasNoManifest()
    {
        File.Delete(Path.Combine(Tree.Root, SpechtTree.ManifestPath));
        _checksNothing = true;
    }

    [Given("the root has a manifest that is not well-formed JSON")]
    public void GivenTheRootHasAManifestThatIsNotWellFormedJson()
    {
        Tree.WriteRaw(SpechtTree.ManifestPath, "{ \"sections\": ");
        _checksNothing = true;
    }

    [Given("the working directory is outside the root")]
    public void GivenTheWorkingDirectoryIsOutsideTheRoot()
    {
        _workingDirectory = Directory.CreateTempSubdirectory("specht-working-").FullName;
        Path.GetRelativePath(Tree.Root, _workingDirectory).Should().StartWith("..");
    }

    [Given("a file already exists at the report path from an earlier run")]
    public void GivenAFileAlreadyExistsAtTheReportPathFromAnEarlierRun()
    {
        _reportPath = BareReportPath;
        if (_checksNothing)
        {
            using var other = new SpechtTree();
            other.WriteFeature("0002", "F1", featureFile: null);
            File.WriteAllText(ReportFile, Serialized(other.Run()));
            return;
        }

        Tree.WriteFeature("0002", "F1", featureFile: null);
        var earlier = Serialized(Tree.Run());
        Directory.Delete(Path.Combine(Tree.Root, "epics", "0002-epic"), recursive: true);
        earlier.Length.Should().BeGreaterThan(Serialized(Tree.Run()).Length, "the earlier run's tree held one more violation");
        File.WriteAllText(ReportFile, earlier);
    }

    [When("the check runs with a report path")]
    [When("the check runs with that report path")]
    public void WhenTheCheckRunsWithAReportPath() => RunWithReportPath(_reportPath ?? BareReportPath);

    [When("the check runs with a report path in a directory that does not yet exist")]
    public void WhenTheCheckRunsWithAReportPathInADirectoryThatDoesNotYetExist()
    {
        Directory.Exists(Path.Combine(Tree.Root, NewDirectory)).Should().BeFalse();
        RunWithReportPath(NewDirectory + "/new/specht-report.json");
    }

    [When("the check runs with a report path and without JSON output")]
    public void WhenTheCheckRunsWithAReportPathAndWithoutJsonOutput()
    {
        (_stdoutWithoutReportPath, _, _) = AcceptanceTool.Launch(Tree.Root, "--root", ".");
        RunWithReportPath(BareReportPath);
    }

    [When("the check runs with JSON output and a report path")]
    public void WhenTheCheckRunsWithJsonOutputAndAReportPath() => RunWithReportPath(BareReportPath, "--json");

    [When("the check runs with a relative report path")]
    public void WhenTheCheckRunsWithARelativeReportPath() => RunWithReportPath(BareReportPath);

    [Then("the file at that path is the report document")]
    [Then("the file at that path is this run's report document and nothing else")]
    public void ThenTheFileAtThatPathIsTheReportDocument()
    {
        File.Exists(ReportFile).Should().BeTrue("the report path {0} holds the document; stdout: {1}; stderr: {2}", _reportPath, _stdout, _stderr);
        File.ReadAllText(ReportFile).Should().Be(new SpechtReportSerializer().Serialize(Document), _stderr);
    }

    [Then("no file exists at the report path")]
    public void ThenNoFileExistsAtTheReportPath()
    {
        _exit.Should().BeOneOf([2, 3], "the run checked nothing; stderr: {0}", _stderr);
        File.Exists(ReportFile).Should().BeFalse("a run that exits {0} leaves nothing at {1}; stderr: {2}", _exit, _reportPath, _stderr);
    }

    [Then("that directory does not exist")]
    public void ThenThatDirectoryDoesNotExist()
    {
        _exit.Should().BeOneOf([2, 3], "the run checked nothing; stderr: {0}", _stderr);
        Directory.Exists(Path.Combine(Tree.Root, NewDirectory)).Should().BeFalse("a run that exits {0} creates no directory; stderr: {1}", _exit, _stderr);
    }

    [Then("the file at that path under the working directory is the report document")]
    public void ThenTheFileAtThatPathUnderTheWorkingDirectoryIsTheReportDocument()
    {
        var reportPath = _reportPath ?? throw new InvalidOperationException("No report path was named.");
        File.Exists(Path.Combine(Tree.Root, reportPath)).Should().BeFalse("the report path {0} is not read from the root; stderr: {1}", reportPath, _stderr);
        File.Exists(ReportFile).Should().BeTrue("the report path {0} is read from the working directory; stderr: {1}", reportPath, _stderr);
        File.ReadAllText(ReportFile).Should().Be(new SpechtReportSerializer().Serialize(Document), _stderr);
    }

    [Then("the directory exists")]
    public void ThenTheDirectoryExists() =>
        Directory.Exists(Path.GetDirectoryName(ReportFile)).Should().BeTrue("the report path {0}'s directories are created; stderr: {1}", _reportPath, _stderr);

    [Then("the standard output carries the same diagnostic lines and summary as a run without a report path")]
    public void ThenTheStandardOutputCarriesTheSameDiagnosticLinesAndSummaryAsARunWithoutAReportPath()
    {
        _stdoutWithoutReportPath.Should().Contain(Report.Violations.Should().ContainSingle().Which.ToString());
        _stdout.Should().Be(_stdoutWithoutReportPath, _stderr);
    }

    [Then("the document on the standard output equals the document at the path")]
    public void ThenTheDocumentOnTheStandardOutputEqualsTheDocumentAtThePath()
    {
        File.Exists(ReportFile).Should().BeTrue("the report path {0} holds the document; stderr: {1}", _reportPath, _stderr);
        File.ReadAllText(ReportFile).Should().Be(Json, _stderr);
    }

    private string WorkingDirectory => _workingDirectory ?? Tree.Root;

    private string ReportFile => Path.Combine(WorkingDirectory, _reportPath ?? throw new InvalidOperationException("No report path was named."));

    private static string Serialized(SpechtReport report) => new SpechtReportSerializer().Serialize(new SpechtReportDocumentMapper().Map(report));

    private void RunWithReportPath(string path, params string[] options)
    {
        _reportPath = path;
        _root = Tree.Root;
        if (!_checksNothing)
        {
            _report = EngineServices.Check().Run(Tree.Root, null);
            Document = new SpechtReportDocumentMapper().Map(_report);
        }

        (_stdout, _stderr, _exit) = AcceptanceTool.Launch(
            WorkingDirectory,
            ["--root", Path.GetRelativePath(WorkingDirectory, Tree.Root), .. options, "--report", path]);
        Json = _stdout.EndsWith(Environment.NewLine, StringComparison.Ordinal) ? _stdout[..^Environment.NewLine.Length] : _stdout;
    }

    private const string BareReportPath = "specht-report.json";

    private const string NewDirectory = "reports";

    private string? _reportPath;
    private string? _workingDirectory;
    private bool _checksNothing;
    private string _stdoutWithoutReportPath = string.Empty;
}
