using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Specht.Analysis.Documentation;
using Specht.Analysis.Policy;
using Specht.Analysis.Tests.Policy;
using Specht.Codefixes.Documentation;

namespace Specht.Analysis.Tests.Documentation;

internal sealed class RepairWorkspace
{
    public List<AnalysisInput> Inputs { get; } = new AnalysisInputs().Inputs;

    public async Task<Project> Create(params (string Path, string Source)[] sources)
    {
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DocumentationBindingAnalyzer>()
            .WithDiagnosticSeverity(null)
            .AddGlobalOption("build_property.SpechtProjectPath", "test/tests.csproj")
            .AddGlobalOption("build_property.TargetFramework", "net10.0");
        foreach (var source in sources)
        {
            builder = builder.AddSource(source.Path, source.Source);
        }

        foreach (var input in Inputs.Where(static input => input.Kind != "source"))
        {
            builder = builder.AddAdditionalText(input.RelativePath, input.Text!)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtInputKind", input.Kind)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtRelativePath", input.RelativePath);
        }

        builder = builder.AddAdditionalText("test/evidence.json", """
            {"format":"specht.test-evidence","formatVersion":1,"discovery":{"methods":[{"symbolId":"M:Example.Run"}]}}
            """).AddOption("test/evidence.json", "build_metadata.AdditionalFiles.SpechtInputKind", "test-evidence")
            .AddOption("test/evidence.json", "build_metadata.AdditionalFiles.SpechtRelativePath", "test/evidence.json");
        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);
        result.InputDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        var project = result.ProjectInformation.SourceProject;
        foreach (var document in project.Documents)
        {
            var source = sources.Single(source => Path.GetFileName(source.Path) == Path.GetFileName(document.Name));
            project = project.Solution.WithDocumentFilePath(document.Id, source.Path).GetProject(project.Id)!;
        }

        var config = "is_global = true\nbuild_property.SpechtProjectPath = test/tests.csproj\nbuild_property.TargetFramework = net10.0\n";
        foreach (var input in Inputs.Where(static input => input.Kind != "source"))
        {
            project = project.AddAdditionalDocument(input.RelativePath, SourceText.From(input.Text!), filePath: Path.GetFullPath(input.RelativePath)).Project;
            config += "\n[" + Path.GetFullPath(input.RelativePath) + "]\nbuild_metadata.AdditionalFiles.SpechtInputKind = " + input.Kind
                + "\nbuild_metadata.AdditionalFiles.SpechtRelativePath = " + input.RelativePath + "\n";
        }

        project = project.AddAdditionalDocument("evidence.json", SourceText.From("""
            {"format":"specht.test-evidence","formatVersion":1,"discovery":{"methods":[{"symbolId":"M:Example.Run"}]}}
            """), filePath: Path.GetFullPath("test/evidence.json")).Project;
        config += "\n[" + Path.GetFullPath("test/evidence.json") + "]\nbuild_metadata.AdditionalFiles.SpechtInputKind = test-evidence\n"
            + "build_metadata.AdditionalFiles.SpechtRelativePath = test/evidence.json\n";
        project = project.AddAnalyzerConfigDocument(".globalconfig", SourceText.From(config), filePath: Path.GetFullPath(".globalconfig")).Project;
        project.Solution.Workspace.TryApplyChanges(project.Solution).Should().BeTrue();
        return project.Solution.Workspace.CurrentSolution.GetProject(project.Id)!;
    }

    public async Task<ImmutableArray<Diagnostic>> Diagnostics(Project project)
    {
        var compilation = await project.GetCompilationAsync(TestContext.Current.CancellationToken);
        compilation!.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        return await compilation.WithAnalyzers([new DocumentationBindingAnalyzer()], project.AnalyzerOptions)
            .GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);
    }

    public async Task<List<CodeAction>> Actions(Project project, Diagnostic diagnostic)
    {
        var document = project.GetDocument(diagnostic.Location.SourceTree!)!;
        var actions = new List<CodeAction>();
        await new DocumentationCodeFixProvider().RegisterCodeFixesAsync(new CodeFixContext(document, diagnostic,
            (action, _) => actions.Add(action), TestContext.Current.CancellationToken));
        return actions;
    }

    public async Task<Solution> Apply(CodeAction action)
    {
        var operations = await action.GetOperationsAsync(TestContext.Current.CancellationToken);
        return operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
    }
}
