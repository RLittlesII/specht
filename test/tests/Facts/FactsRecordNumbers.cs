namespace Specht.Tests.Facts;

/// <summary>
/// A record's root-relative path and the number OQ-3's reading of B-011 takes from its file name (<c>0132-F2</c> B-011):
/// the file name's first word, up to its first character that is neither a letter nor a digit, as written, a Nano ID's
/// included, never a word of a folder above it, and <see langword="null"/> when the name begins with no such word.
/// </summary>
public sealed class FactsRecordNumbers : TheoryData<string, string?>
{
    public FactsRecordNumbers()
    {
        Add("src/area/.spec/decisions/0001-the-title.md", "0001");
        Add("src/area/.spec/decisions/k3m9xq2a-a-record-named-by-a-nano-id.md", "k3m9xq2a");
        Add("src/area/.spec/decisions/0010_an_underscore.md", "0010");
        Add("src/0002-area/.spec/decisions/notes.md", "notes");
        Add("src/area/.spec/decisions/-a-leading-dash.md", null);
        Add("src/area/.spec/decisions/_a-leading-underscore.md", null);
    }
}
