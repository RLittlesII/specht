using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Specht.Codefixes.Documentation;

internal sealed class DocumentationFixAllProvider : FixAllProvider
{
    public override IEnumerable<FixAllScope> GetSupportedFixAllScopes() =>
        new[] { FixAllScope.Document, FixAllScope.Project, FixAllScope.Solution };

    public override async Task<CodeAction?> GetFixAsync(FixAllContext context)
    {
        var key = context.CodeActionEquivalenceKey;
        if (key is not ("Specht.MaterializeDefault" or "Specht.RemoveDuplicates"))
        {
            return null;
        }

        var projects = context.Scope == FixAllScope.Solution ? context.Solution.Projects : new[] { context.Project };
        var versions = new Dictionary<ProjectId, VersionStamp>();
        var diagnostics = new List<(DocumentId Document, Diagnostic Diagnostic)>();
        foreach (var project in projects)
        {
            if (project.Language != LanguageNames.CSharp)
            {
                continue;
            }

            versions.Add(project.Id, await project.GetDependentVersionAsync(context.CancellationToken).ConfigureAwait(false));
            var findings = context.Scope == FixAllScope.Document && context.Document is not null
                ? await context.GetDocumentDiagnosticsAsync(context.Document).ConfigureAwait(false)
                : await context.GetAllDiagnosticsAsync(project).ConfigureAwait(false);
            foreach (var diagnostic in findings.Where(diagnostic => context.DiagnosticIds.Contains(diagnostic.Id)))
            {
                var document = diagnostic.Location.SourceTree is null ? null : project.GetDocument(diagnostic.Location.SourceTree);
                if (document is not null)
                {
                    diagnostics.Add((document.Id, diagnostic));
                }
            }
        }

        return CodeAction.Create("Fix deterministic documentation bindings", async cancellationToken =>
        {
            var solution = context.Solution.Workspace.CurrentSolution;
            foreach (var entry in versions)
            {
                var project = solution.GetProject(entry.Key);
                if (project is null || await project.GetDependentVersionAsync(cancellationToken).ConfigureAwait(false) != entry.Value)
                {
                    return solution;
                }
            }

            var repairs = new List<DocumentationRepair>();
            foreach (var finding in diagnostics)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var document = solution.GetDocument(finding.Document)!;
                var plans = await new DocumentationRepairs().Read(document, finding.Diagnostic, cancellationToken).ConfigureAwait(false);
                repairs.AddRange(plans.Where(plan => plan.Key == key));
            }

            return await new DocumentationEdits().Apply(solution, repairs, cancellationToken).ConfigureAwait(false);
        }, key);
    }
}
