using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Reqnroll;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Specht.Analysis.Documentation;
using Specht.Codefixes.Documentation;

namespace Specht.Acceptance.Documentation;

public sealed partial class DocumentationSteps
{
    [Given("a claim-only member with one unambiguous type default and an invoked repair")]
    public void GivenMaterialization()
    {
        _repairSource = "/// <spec feature=\"F1\" />\npublic class Example {\n"
            + "    /// <summary>Keep &amp; preserve.</summary><custom />\n"
            + "    /// <verifies claim=\"B-001\" />\n    public void Run() {}\n}";
        _repairKey = "Specht.MaterializeDefault";
        _repairExpected = _repairSource.Replace("<verifies claim=\"B-001\" />", "<verifies feature=\"F1\" claim=\"B-001\" />");
    }

    [Given("{string} default targets")]
    public void GivenUnsafeRepair(string problem)
    {
        _repairAmbiguous = problem == "ambiguous";
        var defaults = problem == "missing" ? string.Empty : "/// <spec feature=\"F1\" />"
            + (problem == "conflicting" ? "<spec feature=\"F2\" />" : string.Empty) + "\n";
        _repairSource = defaults + "public class Example {\n/// <verifies claim=\"B-001\" />\npublic void Run() {} }";
    }

    [Given("a selected test method with no binding documentation")]
    public void GivenMissingBinding()
    {
        _repairSource = "public class Example {\n    public void Run() {}\n}";
    }

    [Given("a declaration with unrelated XML documentation including another custom tag")]
    public void GivenOtherXml() => GivenMaterialization();

    [Given("duplicate valid relationship elements on one code symbol")]
    public void GivenRepairDuplicates()
    {
        _repairSource = "public class Example {\n/// <verifies feature=\"F1\" claim=\"B-001\" />\n"
            + "/// <custom /><verifies feature=\"F1\" claim=\"B-001\" />\npublic void Run() {}\n}";
        _repairKey = "Specht.RemoveDuplicates";
        _repairExpected = _repairSource.Replace("<custom /><verifies feature=\"F1\" claim=\"B-001\" />", "<custom />");
    }

    [Given("a binding repair that needs a target choice or replacement")]
    public void GivenRepairChoice()
    {
        _repairSource = "public class Example {\n/// <verifies feature=\"F1\" claim=\"B-999\" />\npublic void Run() {} }";
        _repairExpected = _repairSource.Replace("B-999", "B-001");
    }

    [Given("{string}")]
    public void GivenSelection(string selection) => _repairKey = selection == "an existing record is selected" ? "Specht.Select:" : null;

    [Given("{string} in a binding element")]
    public void GivenMisspelled(string defect)
    {
        _repairSource = "public class Example {\n/// <verifies "
            + (defect == "a misspelled ID" ? "feature=\"F1\" claim=\"B-999\"" : "feature=\"F1\" cliam=\"B-001\"")
            + " />\npublic void Run() {} }";
    }

    [Given("a reserved element with {string}")]
    public void GivenDestructive(string material)
    {
        _repairSource = "public class Example {\n/// <verifies feature=\"F1\" claim=\"B-001\""
            + (material == "unknown attributes" ? " extra=\"keep\" />" : ">keep</verifies>") + "\npublic void Run() {} }";
    }

    [Given("multiple bindings needing {string} with no target choice")]
    public void GivenBulk(string repair)
    {
        if (repair == "unambiguous default materialization")
        {
            GivenMaterialization();
        }
        else
        {
            GivenRepairDuplicates();
        }

        _repairSource = _repairSource!.Replace("public void Run() {}", "public void Run() {}\n"
            + (_repairKey == "Specht.MaterializeDefault" ? "/// <implements claim=\"B-001\" />" :
                "/// <implements feature=\"F1\" claim=\"B-001\" /><implements feature=\"F1\" claim=\"B-001\" />")
            + "\npublic void Other() {}");
        _repairExpected = _repairExpected!.Replace("public void Run() {}", "public void Run() {}\n"
            + "/// <implements feature=\"F1\" claim=\"B-001\" />\npublic void Other() {}");
    }

    [Given("a target selected for one declaration's binding")]
    public void GivenSelectedForOne()
    {
        GivenRepairChoice();
        _repairKey = "Specht.Select:verifies:";
    }

    [Given("unrelated declarations needing their own target choices")]
    public void GivenUnrelatedChoices() => _repairSource = _repairSource!.Replace("public void Run() {}",
        "public void Run() {}\n/// <implements claim=\"B-999\" />\npublic void Other() {}");

    [When("{string} is requested")]
    public async Task RequestRepair(string operation)
    {
        if (operation == "binding analysis")
        {
            _repairKey = null;
        }

        await AnalyzeRepair();
    }

    [When("the duplicate-removal fix is invoked")]
    [When("the repair is invoked")]
    [When("available repairs are requested")]
    public Task InvokeRepair() => AnalyzeRepair();

    [When("Fix All is invoked for that supported repair")]
    public async Task InvokeFixAll()
    {
        _repairBulk = true;
        await AnalyzeRepair();
    }

    [When("bulk repairs are requested")]
    public async Task InvokeChoiceFixAll()
    {
        await AnalyzeRepair();
        var provider = new DocumentationCodeFixProvider();
        var selected = _repairActions.Single(action => action.EquivalenceKey!.StartsWith(_repairKey!, StringComparison.Ordinal));
        var context = new FixAllContext(_repairProject!.Documents.Single(), provider, FixAllScope.Project, selected.EquivalenceKey,
            provider.FixableDiagnosticIds, new RepairDiagnostics(), CancellationToken.None);
        (await provider.GetFixAllProvider().GetFixAsync(context)).Should().BeNull();
    }

    [Then("the repair persists its complete target identity while preserving other XML documentation")]
    [Then("redundant copies are removed while one complete binding is preserved")]
    [Then("the deterministic repairs are applied")]
    public void ThenRepaired() => _repairFixed.Should().Be(_repairExpected);

    [Then("analysis reports the missing binding and an invoked fix offers existing claim choices without inventing the target")]
    public async Task ThenClaimChoices()
    {
        _repairDiagnostics.Should().ContainSingle().Which.Id.Should().Be("SPCT0007");
        var action = _repairActions.Should().ContainSingle().Subject;
        action.Title.Should().Contain("F1").And.Contain("B-001");
        _repairFixed.Should().Be(_repairSource);
        var operations = await action.GetOperationsAsync(CancellationToken.None);
        var project = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution.GetProject(_repairProject!.Id)!;
        (await RepairFindings(project)).Should().BeEmpty();
        (await project.Documents.Single().GetTextAsync()).ToString().Should().Contain("<verifies feature=\"F1\" claim=\"B-001\" />");
    }

    [Then("the unrelated documentation is preserved")]
    public void ThenPreserved() => _repairFixed.Should().Be(_repairKey is null ? _repairSource : _repairExpected);

    [Then("{string}")]
    public void ThenSelection(string outcome) => _repairFixed.Should().Be(outcome == "no target edit is made" ? _repairSource : _repairExpected);

    [Then("no replacement is guessed from spelling or similarity")]
    public void ThenNoGuess() => _repairActions.Should().OnlyContain(static action =>
        action.EquivalenceKey!.StartsWith("Specht.Select:", StringComparison.Ordinal));

    [Then("no repair deletes that material merely to make the element valid")]
    public void ThenNoDeletion()
    {
        _repairActions.Should().BeEmpty();
        _repairFixed.Should().Be(_repairSource);
    }

    [Then("that selected target is not propagated to those declarations")]
    public void ThenNoPropagation() => _repairFixed.Should().Contain("<implements claim=\"B-999\" />");

    private async Task AnalyzeRepair()
    {
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DocumentationBindingAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("test/Example.cs", _repairSource!);
        var result = await builder.GenerateAsync(CancellationToken.None);
        result.InputDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        var project = result.ProjectInformation.SourceProject;
        project = project.Solution.WithDocumentFilePath(project.DocumentIds.Single(), "test/Example.cs").GetProject(project.Id)!;
        var inputs = new[]
        {
            ("configuration", "config/consumer.json", """
             {"schemaVersion":"0.1.0","identifiers":{"feature":"^F[0-9]+$","claim":"^B-[0-9]{3}$"},
              "claimsSection":"3. Acceptance Criteria","markers":{},"codeAnalysis":{
               "mappings":{"/schemaVersion":"/schemaVersion","/identifiers":"/identifiers","/claimsSection":"/claimsSection",
                 "/policy":"/codeAnalysis","/markers":"/markers"},
               "projects":[{"path":"test/tests.csproj","targetFrameworks":["net10.0"]}],"features":[{"feature":"F1"}],
               "enforcement":[{"project":"test/tests.csproj","include":["test/**/*.cs"],"exclude":[],"role":"verifies","declarationKinds":["testMethod"]}]}}
             """),
            ("feature", "src/read/.spec/README.md", "---\nid: F1\nspec_status: draft\n---\n## 3. Acceptance Criteria\n"
                + "| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n| B-001 | Synthetic | agreement | Active |\n"),
            ("schema:feature", "contracts/feature.json", "{\"type\":\"object\"}"),
            ("test-evidence", "test/evidence.json", """
             {"format":"specht.test-evidence","formatVersion":1,"discovery":{"methods":[{"symbolId":"M:Example.Run"}]}}
             """),
        };
        if (_repairAmbiguous)
        {
            inputs = inputs.Concat(new[] { ("feature", "src/other/.spec/README.md", inputs[1].Item3) }).ToArray();
        }

        var config = "is_global = true\nbuild_property.SpechtProjectPath = test/tests.csproj\nbuild_property.TargetFramework = net10.0\n";
        foreach (var (kind, path, text) in inputs)
        {
            project = project.AddAdditionalDocument(path, SourceText.From(text), filePath: Path.GetFullPath(path)).Project;
            config += "\n[" + Path.GetFullPath(path) + "]\nbuild_metadata.AdditionalFiles.SpechtInputKind = " + kind
                + "\nbuild_metadata.AdditionalFiles.SpechtRelativePath = " + path + "\n";
        }

        project = project.AddAnalyzerConfigDocument(".globalconfig", SourceText.From(config), filePath: Path.GetFullPath(".globalconfig")).Project;
        project.Solution.Workspace.TryApplyChanges(project.Solution).Should().BeTrue();
        _repairProject = project = project.Solution.Workspace.CurrentSolution.GetProject(project.Id)!;
        _repairDiagnostics = await RepairFindings(project);
        _repairDiagnostics.Should().NotBeEmpty();
        var provider = new DocumentationCodeFixProvider();
        _repairActions.Clear();
        foreach (var diagnostic in _repairDiagnostics.Where(diagnostic => provider.FixableDiagnosticIds.Contains(diagnostic.Id)))
        {
            await provider.RegisterCodeFixesAsync(new CodeFixContext(project.GetDocument(diagnostic.Location.SourceTree!)!, diagnostic,
                (action, _) => _repairActions.Add(action), CancellationToken.None));
        }

        var action = _repairBulk
            ? await provider.GetFixAllProvider().GetFixAsync(new FixAllContext(project.Documents.Single(), provider, FixAllScope.Project,
                _repairKey, provider.FixableDiagnosticIds, new RepairDiagnostics(), CancellationToken.None))
            : _repairKey is null ? null : _repairActions.FirstOrDefault(action => action.EquivalenceKey!.StartsWith(_repairKey, StringComparison.Ordinal));
        if (_repairKey is not null)
        {
            action.Should().NotBeNull();
        }

        var fixedProject = action is null ? project
            : (await action.GetOperationsAsync(CancellationToken.None)).OfType<ApplyChangesOperation>().Single().ChangedSolution.GetProject(project.Id)!;
        _repairFixed = (await fixedProject.Documents.Single().GetTextAsync()).ToString();
        if (action is not null && !_repairKey!.StartsWith("Specht.Select:", StringComparison.Ordinal))
        {
            (await RepairFindings(fixedProject)).Should().BeEmpty();
        }
    }

    private async Task<ImmutableArray<Diagnostic>> RepairFindings(Project project)
    {
        var compilation = await project.GetCompilationAsync();
        compilation!.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        return await compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new DocumentationBindingAnalyzer()), project.AnalyzerOptions)
            .GetAnalyzerDiagnosticsAsync();
    }

    private readonly List<CodeAction> _repairActions = new();
    private string? _repairSource;
    private string? _repairExpected;
    private string? _repairFixed;
    private string? _repairKey;
    private bool _repairBulk;
    private bool _repairAmbiguous;
    private Project? _repairProject;
    private ImmutableArray<Diagnostic> _repairDiagnostics;
}
