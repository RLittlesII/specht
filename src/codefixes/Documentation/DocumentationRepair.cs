using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Specht.Codefixes.Documentation;

internal sealed class DocumentationRepair
{
    public DocumentationRepair(string key, string title, IReadOnlyList<(DocumentId Document, TextChange Change)> changes)
    {
        Key = key;
        Title = title;
        Changes = changes;
    }

    public string Key { get; }
    public string Title { get; }
    public IReadOnlyList<(DocumentId Document, TextChange Change)> Changes { get; }
}
