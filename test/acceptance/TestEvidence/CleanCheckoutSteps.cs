using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Reqnroll;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Specht.Acceptance.Documentation;
using Specht.Analysis.Documentation;
using Specht.Tool.Features.Evidence;

using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace Specht.Acceptance.TestEvidence;

/// <summary>Provisional recognition, reconciliation messages and the Release boundary on a clean checkout (0201-F3 B-017 to B-019, B-022 to B-024).</summary>
[Binding]
public sealed class CleanCheckoutSteps : IDisposable
{
    [Given("a clean checkout with no evidence artifact and no evidence cache")]
    public void GivenCleanCheckout() => _artifact = null;

    [Given("a selected xUnit v2 project run by VSTest whose test method carries an authored verifies relationship")]
    public void GivenAuthoredVerifies()
    {
        _source = Source("/// <verifies feature=\"F1\" claim=\"B-001\" />\n    ");
        _tree.Declare("a", null);
    }

    [Given("mandatory enforcement requires a verifies relationship on every test method")]
    public async Task GivenMandatoryEnforcement()
    {
        var current = await AcceptanceTestEvidence.Current(Symbol, [(SourcePath, Source(""))], Inputs());
        var diagnostics = await Analyze(Source(""), current);
        diagnostics.Select(static diagnostic => diagnostic.Id).Should().Equal("SPCT0007");
    }

    [Given("warnings are treated as errors")]
    public void GivenWarningsAsErrors() => _strict = true;

    [Given("an evidence artifact produced before a test source file changed")]
    public async Task GivenStaleArtifact()
    {
        _tree.Declare("a", null);
        var cases = new[] { _tree.Case("a", CaseId, "LocalCases", "Fact") };
        _tree.Discover("a", cases);
        _tree.Run("a", null, []);
        _source = Source("");
        _artifact = await AcceptanceTestEvidence.Current(Symbol, [(SourcePath, _source)], Inputs());
    }

    [Given("a global package reference present in every project")]
    public void GivenGlobalPackageReference() => _source = "public class Library\n{\n    public static int Value => 1;\n}";

    [Given("a Release configuration that excludes the test sources and test packages")]
    public void GivenReleaseExcludesTests() => _configuration = "Release";

    [Given("an evidence artifact left by an earlier Debug build")]
    public async Task GivenDebugArtifact()
    {
        _source = Source("");
        _artifact = await AcceptanceTestEvidence.Current(Symbol, [(SourcePath, _source)], Inputs());
    }

    [When("the projects are built before any evidence is collected")]
    public async Task WhenBuiltBeforeEvidence() => _diagnostics = await Analyze(_source!, _artifact);

    [When("reconciliation is requested before any evidence is collected")]
    public void WhenReconciledBeforeEvidence() => _result = _tree.ReconcileWithoutEvidence();

    [When("reconciliation is requested")]
    public void WhenReconciled()
    {
        _tree.EditSource();
        _result = _tree.Reconcile();
    }

    [When("the Release configuration is built")]
    public async Task WhenReleaseBuilt()
    {
        _configuration = "Release";
        _diagnostics = await Analyze(_source!, _artifact);
    }

    [When("the documented build, discovery, run and analysis sequence is followed, with the produced artifact supplied to the build")]
    public async Task WhenSequenceFollowed()
    {
        _tree.Declare("a", null);
        var cases = new[] { _tree.Case("a", CaseId, "LocalCases", "Fact"), _tree.Case("a", TheoryId, "LocalCases", "Theory") };
        _tree.Discover("a", cases);
        _tree.Run("a", null, cases.Select(static testCase => new VsTestResult(testCase) { Outcome = TestOutcome.Passed }));
        _result = _tree.Reconcile();
        _diagnostics = await Analyze(_source!, _tree.Artifact("a").ToJsonString());
    }

    [Then("the build succeeds without a diagnostic suppression")]
    public void ThenBuildSucceeds() => _diagnostics.Where(diagnostic => _strict || diagnostic.Severity == DiagnosticSeverity.Error)
        .Select(static diagnostic => diagnostic.Id).Should().BeEmpty();

    [Then("a verifies relationship with a malformed claim or an unresolved claim is still reported")]
    public async Task ThenMalformedOrUnresolvedStillReported()
    {
        var malformed = await Analyze(Source("/// <verifies feature=\"F1\" claim=\"B-001,B-002\" />\n    "), null);
        var unresolved = await Analyze(Source("/// <verifies feature=\"F1\" claim=\"B-009\" />\n    "), null);
        malformed.Select(static diagnostic => diagnostic.Id).Should().Equal("SPCT0003");
        unresolved.Select(static diagnostic => diagnostic.Id).Should().Equal("SPCT0004");
    }

    [Then("reconciliation is incomplete")]
    public void ThenIncomplete()
    {
        _result!.ExitCode.Should().Be(2);
        _result.Report["inputState"]!["state"]!.GetValue<string>().Should().Be("incomplete");
    }

    [Then("its diagnosis names the build, discovery and run steps that produce the evidence")]
    public void ThenDiagnosisNamesSteps() => Messages("missingCompilation").Should().NotBeEmpty().And.OnlyContain(static message => NamesSteps(message));

    [Then("the evidence is reported as stale with a diagnosis naming the rebuild, discovery and run steps that regenerate it")]
    public void ThenStaleDiagnosis() => Messages("staleEvidence").Should().NotBeEmpty()
        .And.OnlyContain(static message => message.Contains("rebuild", StringComparison.OrdinalIgnoreCase) && NamesSteps(message));

    [Then("the build treats the artifact as provisional and still succeeds")]
    public async Task ThenProvisionalBuildSucceeds()
    {
        (await Analyze(_source!, _artifact)).Select(static diagnostic => diagnostic.Id).Should().Equal("SPCT0007");
        (await Analyze(_source! + "\n", _artifact)).Select(static diagnostic => diagnostic.Id).Should().BeEmpty();
    }

    [Then("the analysis holds current passing evidence for the authored verifies relationship")]
    public void ThenCurrentPassingEvidence()
    {
        _result!.Report["findings"]!.ToJsonString().Should().Be("[]");
        _result.ExitCode.Should().Be(0);
        _result.Report["inputState"]!["state"]!.GetValue<string>().Should().Be("complete");
    }

    [Then("the rebuild with the artifact supplied succeeds")]
    public void ThenRebuildSucceeds() => _diagnostics.Select(static diagnostic => diagnostic.Id).Should().BeEmpty();

    [Then("no hand-written evidence or diagnostic suppression was needed")]
    public void ThenNothingHandWritten()
    {
        var artifact = _tree.Artifact("a");
        artifact["adapter"]!["id"]!.GetValue<string>().Should().Be("vstest");
        artifact["results"]!["state"]!.GetValue<string>().Should().Be("available");
        artifact["results"]!["runId"]!.GetValue<string>().Should().NotBeNullOrWhiteSpace();
    }

    [Then("the Debug evidence artifact is not used as Release evidence")]
    public async Task ThenDebugEvidenceNotUsed()
    {
        _configuration = "Debug";
        (await Analyze(_source!, _artifact)).Select(static diagnostic => diagnostic.Id).Should().Equal("SPCT0007");
        _diagnostics.Select(static diagnostic => diagnostic.Id).Should().BeEmpty();
    }

    public void Dispose() => _tree.Dispose();

    private static string SourcePath => Path.Combine(AcceptanceTestEvidence.Root, "test", "Example.cs");

    private static bool NamesSteps(string message) =>
        message.Contains("build", StringComparison.OrdinalIgnoreCase)
        && message.Contains("dotnet test --list-tests", StringComparison.Ordinal)
        && System.Text.RegularExpressions.Regex.IsMatch(message, "dotnet test(?! --list-tests)");

    private static List<(string Kind, string Path, string Text)> Inputs() =>
        [
            ("configuration", "config/consumer.json", Manifest()),
            ("feature", "src/read/.spec/README.md", "---\nid: F1\nspec_status: draft\n---\n## 3. Acceptance Criteria\n"
                + "| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n| B-001 | Synthetic | agreement | Active |\n"),
            ("schema:feature", "contracts/feature.json", "{\"type\":\"object\"}"),
        ];

    private static string Manifest() => Enforced;

    private static string Source(string documentation) =>
        "public class Example\n{\n    " + documentation + "public void Run() { }\n}";

    private string[] Messages(string kind) =>
        _result!.Report["findings"]!.AsArray().Where(finding => finding!["kind"]!.GetValue<string>() == kind)
            .Select(static finding => finding!["message"]!.GetValue<string>()).ToArray();

    private async Task<IReadOnlyList<Diagnostic>> Analyze(string source, string? artifact)
    {
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DocumentationBindingAnalyzer>().WithDiagnosticSeverity(null)
            .AddSource(SourcePath, source)
            .AddGlobalOption("build_property.SpechtRoot", AcceptanceTestEvidence.Root)
            .AddGlobalOption("build_property.SpechtProjectPath", "test/tests.csproj")
            .AddGlobalOption("build_property.TargetFramework", "net10.0")
            .AddGlobalOption("build_property.Configuration", _configuration);
        var inputs = Inputs();
        if (artifact is not null)
        {
            inputs.Add(("test-evidence", "obj/specht-test-evidence/tests-net10.0.json", artifact));
        }

        foreach (var (kind, path, text) in inputs)
        {
            builder = builder.AddAdditionalText(path, text)
                .AddOption(path, "build_metadata.AdditionalFiles.SpechtInputKind", kind)
                .AddOption(path, "build_metadata.AdditionalFiles.SpechtRelativePath", path);
        }

        var result = await builder.GenerateAsync(CancellationToken.None);
        result.InputDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        return result.AnalyzerResults[typeof(DocumentationBindingAnalyzer)].Diagnostics;
    }

    private const string Enforced = """
        {"schemaVersion":"0.1.0","identifiers":{"feature":"^F[0-9]+$","claim":"^B-[0-9]{3}$"},
         "claimsSection":"3. Acceptance Criteria","markers":{},"codeAnalysis":{
          "mappings":{"/schemaVersion":"/schemaVersion","/identifiers":"/identifiers","/claimsSection":"/claimsSection",
            "/policy":"/codeAnalysis","/markers":"/markers"},
          "projects":[{"path":"test/tests.csproj","targetFrameworks":["net10.0"]}],"features":[{"feature":"F1"}],
          "enforcement":[{"project":"test/tests.csproj","include":["test/**/*.cs"],"exclude":[],"role":"verifies","declarationKinds":["testMethod"]}]}}
        """;

    private const string Symbol = "M:Example.Run";
    private const string CaseId = "11111111-1111-4111-8111-111111111111";
    private const string TheoryId = "22222222-2222-4222-8222-222222222222";
    private readonly VsTestEvidenceTree _tree = new();
    private IReadOnlyList<Diagnostic> _diagnostics = [];
    private EvidenceResult? _result;
    private string? _source;
    private string? _artifact;
    private string _configuration = "Debug";
    private bool _strict;
}
