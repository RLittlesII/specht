using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Specht.Analysis.Tests.Documentation;

internal sealed class RepairDiagnostics : FixAllContext.DiagnosticProvider
{
    public override async Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
    {
        var diagnostics = await new RepairWorkspace().Diagnostics(document.Project);
        var tree = await document.GetSyntaxTreeAsync(cancellationToken);
        return diagnostics.Where(diagnostic => diagnostic.Location.SourceTree == tree);
    }

    public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
        Task.FromResult<IEnumerable<Diagnostic>>([]);

    public override async Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
        await new RepairWorkspace().Diagnostics(project);
}
