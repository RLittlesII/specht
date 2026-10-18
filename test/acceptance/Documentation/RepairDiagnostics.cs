using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Specht.Analysis.Documentation;

namespace Specht.Acceptance.Documentation;

internal sealed class RepairDiagnostics : FixAllContext.DiagnosticProvider
{
    public override async Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
    {
        var compilation = await document.Project.GetCompilationAsync(cancellationToken);
        var tree = await document.GetSyntaxTreeAsync(cancellationToken);
        var diagnostics = await compilation!.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new DocumentationBindingAnalyzer()),
            document.Project.AnalyzerOptions).GetAnalyzerDiagnosticsAsync(cancellationToken);
        return System.Linq.Enumerable.Where(diagnostics, diagnostic => diagnostic.Location.SourceTree == tree);
    }

    public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
        Task.FromResult<IEnumerable<Diagnostic>>(ImmutableArray<Diagnostic>.Empty);

    public override async Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken);
        return await compilation!.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new DocumentationBindingAnalyzer()), project.AnalyzerOptions)
            .GetAnalyzerDiagnosticsAsync(cancellationToken);
    }
}
