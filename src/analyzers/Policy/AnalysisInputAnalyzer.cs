using System.Collections.Immutable;
using System.Linq;
using System.Resources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Specht.Analysis.Policy;

/// <summary>Reports unavailable consumer analysis independently of binding diagnostics.</summary>
#pragma warning disable RS1004 // 0201-F2 decision 0003 limits this capability to CSharp declarations.
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AnalysisInputAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InputFailure);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
#pragma warning disable RSA1007 // Roslyn registration is a method taking an Action, not a delegate invocation.
        context.RegisterCompilationAction(Analyze);
#pragma warning restore RSA1007
    }

    private static void Analyze(CompilationAnalysisContext context)
    {
        var snapshot = new AnalysisInputAdapter().Read(context.Options.AdditionalFiles, context.Options.AnalyzerConfigOptionsProvider,
            context.Compilation.SyntaxTrees, context.CancellationToken);
        foreach (var failure in snapshot.Failures)
        {
            var supplied = context.Options.AdditionalFiles.FirstOrDefault(file =>
                context.Options.AnalyzerConfigOptionsProvider.GetOptions(file)
                    .TryGetValue("build_metadata.AdditionalFiles.SpechtRelativePath", out var path) && path == failure.RelativePath);
            var location = supplied is null ? Location.None
                : Location.Create(supplied.Path, new TextSpan(0, 0), new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 0)));
            var properties = ImmutableDictionary<string, string?>.Empty.Add("SpechtRelativePath", failure.RelativePath)
                .Add("SpechtAnalysisAvailable", "false");
            context.ReportDiagnostic(Diagnostic.Create(InputFailure, location, properties, failure.Message));
        }
    }

    private static readonly ResourceManager Resources = new("Specht.Analysis.Policy.AnalysisResources", typeof(AnalysisInputAnalyzer).Assembly);
#pragma warning disable RS2008 // The pre-release SPCT registry is 0201-F2 § 7; package release tracking belongs to 0243.
    private static readonly DiagnosticDescriptor InputFailure = new(
        "SPCT0001", new LocalizableResourceString("InputTitle", Resources, typeof(AnalysisInputAnalyzer)),
        new LocalizableResourceString("InputMessage", Resources, typeof(AnalysisInputAnalyzer)), "Traceability", DiagnosticSeverity.Error, true,
        description: new LocalizableResourceString("InputDescription", Resources, typeof(AnalysisInputAnalyzer)),
        helpLinkUri: "https://github.com/RLittlesII/specht/blob/main/src/codeanalysis/docs/SPCT0001.md",
        customTags: new[] { WellKnownDiagnosticTags.CompilationEnd });
}
