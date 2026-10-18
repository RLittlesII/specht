using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Specht.Codefixes.Documentation;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DocumentationCodeFixProvider))]
[Shared]
public sealed class DocumentationCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("SPCT0002", "SPCT0003", "SPCT0004", "SPCT0005", "SPCT0006", "SPCT0007", "SPCT0009");

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var version = await document.Project.GetDependentVersionAsync(context.CancellationToken).ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            var plans = await new DocumentationRepairs().Read(document, diagnostic, context.CancellationToken).ConfigureAwait(false);
            foreach (var plan in plans)
            {
                context.RegisterCodeFix(CodeAction.Create(plan.Title, async cancellationToken =>
                {
                    var current = document.Project.Solution.Workspace.CurrentSolution;
                    var live = current.GetDocument(document.Id);
                    if (live is null || await live.Project.GetDependentVersionAsync(cancellationToken).ConfigureAwait(false) != version)
                    {
                        return current;
                    }

                    var refreshed = await new DocumentationRepairs().Read(live, diagnostic, cancellationToken).ConfigureAwait(false);
                    var selected = refreshed.FirstOrDefault(repair => repair.Key == plan.Key);
                    return selected is null ? current
                        : await new DocumentationEdits().Apply(current, new[] { selected }, cancellationToken).ConfigureAwait(false);
                }, plan.Key), diagnostic);
            }
        }
    }

    public override FixAllProvider GetFixAllProvider() => new DocumentationFixAllProvider();
}
