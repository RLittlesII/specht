using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Specht.Acceptance.Adoption;

internal sealed class AdoptionFile(string path, string? text, string kind) : AdditionalText
{
    public override string Path => path;
    public string Kind => kind;
    public override SourceText? GetText(CancellationToken cancellationToken = default) =>
        text is null ? null : SourceText.From(text, Encoding.UTF8);
}
