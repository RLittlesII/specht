using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BindingSpike;

public static class FrameworkEvidence
{
    public static async Task<object> Run(Fixture fixture, Action<bool, string> check)
    {
        const string folder = "epics/0201-spec-code-analysis/spikes/0236/frameworks";
        var binary = Directory.GetFiles(folder + "/bin/Debug", "BindingSpike.FrameworkTests.dll", SearchOption.AllDirectories).Single();
        var references = Fixture.References.Concat(Directory.GetFiles(Path.GetDirectoryName(binary)!, "*.dll").Select(path => MetadataReference.CreateFromFile(path)))
            .DistinctBy(reference => Path.GetFileName(reference.Display), StringComparer.OrdinalIgnoreCase).ToArray();
        var source = File.ReadAllText(folder + "/Tests.cs");
        var tree = CSharpSyntaxTree.ParseText("global using System; global using System.Threading.Tasks;\n" + source,
            new CSharpParseOptions(documentationMode: DocumentationMode.Diagnose), path: folder + "/Tests.cs");
        CSharpCompilation Compile(SyntaxTree input) => CSharpCompilation.Create("RealFrameworkTrial", [input], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var compilation = Compile(tree);
        var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        check(errors.Length == 0, "Real framework source must compile: " + string.Join(";", errors.Select(error => error.GetMessage())));
        check(!(await compilation.WithAnalyzers([new BindingAnalyzer(Candidate.C, requireExplicit: true)], new AnalyzerOptions(fixture.Files)).GetAnalyzerDiagnosticsAsync()).Any(), "Explicit real-framework methods must analyze cleanly.");
        var root = await tree.GetRootAsync();
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToArray();
        check(methods.Length == 8, "Eight real test methods expected.");
        foreach (var name in new[] { "Fact_WhenIncremented_ShouldMatch", "InlineTheory_WhenIncremented_ShouldMatch", "Theory_WhenContentRendered_ShouldMatch" })
        {
            var method = methods.Single(method => method.Identifier.ValueText == name);
            var replacement = method.WithLeadingTrivia(SyntaxFactory.EndOfLine("\n"));
            var mutated = tree.WithRootAndOptions(root.ReplaceNode(method, replacement), tree.Options);
            var diagnostics = await Compile(mutated).WithAnalyzers([new BindingAnalyzer(Candidate.C, requireExplicit: true)], new AnalyzerOptions(fixture.Files)).GetAnalyzerDiagnosticsAsync();
            check(diagnostics.Length == 1 && diagnostics[0].Id == "SPK001", name + ": missing binding must produce SPK001, got " + string.Join(',', diagnostics.Select(diagnostic => diagnostic.Id)));
            check(diagnostics[0].Location.SourceSpan == (await mutated.GetRootAsync()).DescendantNodes().OfType<MethodDeclarationSyntax>().Single(method => method.Identifier.ValueText == name).Identifier.Span,
                "Missing binding diagnostic must point at the real method.");
        }
        foreach (var (tag, expected) in new[] { ("<verifies claim=\"B-002\" />", "SPK009"), ("<verifies feature=\"F1\" epic=\"0001\" claim=\"B-999\" />", "SPK004") })
        {
            var method = methods.Single(method => method.Identifier.ValueText == "InlineTheory_WhenIncremented_ShouldMatch");
            var replaced = method.WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia("\n/// " + tag + "\n"));
            var mutated = tree.WithRootAndOptions(root.ReplaceNode(method, replaced), tree.Options);
            var diagnostics = await Compile(mutated).WithAnalyzers([new BindingAnalyzer(Candidate.C, requireExplicit: true)], new AnalyzerOptions(fixture.Files)).GetAnalyzerDiagnosticsAsync();
            check(diagnostics.Length == 1 && diagnostics[0].Id == expected, "Real theory mutation must produce " + expected);
        }
        var model = compilation.GetSemanticModel(tree);
        var bindings = methods.Select(method => new
        {
            Class = model.GetDeclaredSymbol(method)!.ContainingType.ToDisplayString(),
            Method = method.Identifier.ValueText,
            Targets = BindingReader.Read(method, model, fixture.Catalog, Candidate.C).Targets.Select(target => target.Key).ToArray(),
        }).ToArray();
        check(bindings.All(binding => binding.Targets.Length == 1), "Every real test must resolve exactly one target.");
        return new { Case = "real-framework-bindings", Methods = bindings, DeclaredTargets = fixture.Catalog.AllTargets().Select(target => target.Key).ToArray(), MissingBindingMutations = 3, InvalidTheoryMutations = 2, ReqnrollRequired = false };
    }
}
