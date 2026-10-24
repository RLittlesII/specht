using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Specht.Analysis.Policy;

namespace Specht.Analysis.Bindings.Gherkin;

/// <summary>Reports the findings of a native claim catalogue at the supplied claim files' lines.</summary>
/// <implements epic="0201" feature="F7" claim="B-012" />
#pragma warning disable RS1004 // 0201-F6 decision 0003 limits this capability to CSharp declarations.
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GherkinClaimAnalyzer : DiagnosticAnalyzer
{
    public GherkinClaimAnalyzer() => _descriptors = new GherkinDiagnostics().Descriptors;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _descriptors;

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
#pragma warning disable RSA1007 // Roslyn registration is a method taking an Action, not a delegate invocation.
        context.RegisterCompilationAction(Analyze);
#pragma warning restore RSA1007
    }

    private static Location Locate(AdditionalText? supplied, int line, CompilationAnalysisContext context)
    {
        var text = supplied?.GetText(context.CancellationToken);
        if (supplied is null || text is null || line < 1 || line > text.Lines.Count)
        {
            return Location.None;
        }

        var span = text.Lines[line - 1].Span;
        return Location.Create(supplied.Path, span, text.Lines.GetLinePositionSpan(span));
    }

    private void Analyze(CompilationAnalysisContext context)
    {
        var snapshot = new AnalysisInputAdapter().Read(context.Options.AdditionalFiles, context.Options.AnalyzerConfigOptionsProvider,
            context.Compilation.SyntaxTrees, context.CancellationToken);
        if (!snapshot.Available)
        {
            return;
        }

        foreach (var finding in snapshot.Catalog!.Findings)
        {
            var descriptor = _descriptors.First(candidate => candidate.Id == finding.DiagnosticId);
            var supplied = context.Options.AdditionalFiles.FirstOrDefault(file =>
                context.Options.AnalyzerConfigOptionsProvider.GetOptions(file)
                    .TryGetValue("build_metadata.AdditionalFiles.SpechtRelativePath", out var path) && path == finding.RelativePath);
            var properties = ImmutableDictionary<string, string?>.Empty.Add("SpechtRelativePath", finding.RelativePath)
                .Add("SpechtLine", finding.Line.ToString(CultureInfo.InvariantCulture));
            context.ReportDiagnostic(Diagnostic.Create(descriptor, Locate(supplied, finding.Line, context), properties, finding.Text));
        }
    }

    private readonly ImmutableArray<DiagnosticDescriptor> _descriptors;
}
