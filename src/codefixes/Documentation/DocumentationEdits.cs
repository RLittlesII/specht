using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;

namespace Specht.Codefixes.Documentation;

internal sealed class DocumentationEdits
{
    public async Task<Solution> Apply(Solution solution, IEnumerable<DocumentationRepair> repairs, CancellationToken cancellationToken)
    {
        var original = solution;
        foreach (var group in repairs.SelectMany(static repair => repair.Changes).GroupBy(static change => change.Document))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var document = solution.GetDocument(group.Key)!;
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var changes = group.Select(static edit => edit.Change).Distinct().OrderBy(static change => change.Span.Start).ToArray();
            if (changes.Zip(changes.Skip(1), static (left, right) => left.Span.End > right.Span.Start).Any(static overlaps => overlaps))
            {
                return original;
            }

            var updated = text.WithChanges(changes);
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var root = await CSharpSyntaxTree.ParseText(updated, (CSharpParseOptions?)document.Project.ParseOptions,
                document.FilePath ?? document.Name, cancellationToken).GetRootAsync(cancellationToken).ConfigureAwait(false);
            editor.ReplaceNode(editor.OriginalRoot, root);
            solution = editor.GetChangedDocument().Project.Solution;
        }

        return solution;
    }
}
