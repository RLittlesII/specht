using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

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
            var options = (CSharpParseOptions?)document.Project.ParseOptions;
            var path = document.FilePath ?? document.Name;
            var root = await CSharpSyntaxTree.ParseText(updated, options, path, cancellationToken).GetRootAsync(cancellationToken).ConfigureAwait(false);
            var order = new DocumentationOrder().Read(updated, root, Written(changes));
            if (order.Count > 0)
            {
                root = await CSharpSyntaxTree.ParseText(updated.WithChanges(order), options, path, cancellationToken)
                    .GetRootAsync(cancellationToken).ConfigureAwait(false);
            }

            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            editor.ReplaceNode(editor.OriginalRoot, root);
            solution = editor.GetChangedDocument().Project.Solution;
        }

        return solution;
    }

    private static IReadOnlyList<TextSpan> Written(IReadOnlyList<TextChange> changes)
    {
        var written = new List<TextSpan>();
        var delta = 0;
        foreach (var change in changes)
        {
            var length = change.NewText?.Length ?? 0;
            if (length > 0)
            {
                written.Add(new TextSpan(change.Span.Start + delta, length));
            }

            delta += length - change.Span.Length;
        }

        return written;
    }
}
