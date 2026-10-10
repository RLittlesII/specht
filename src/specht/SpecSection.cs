namespace Specht;

/// <summary>A <c>## </c> section of a specification, with the table it opens with, if any.</summary>
/// <param name="Title">The heading text, trimmed.</param>
/// <param name="Line">One-based line of the heading.</param>
/// <param name="Headers">The first table's header cells, or empty when the section has no table.</param>
/// <param name="Rows">The first table's body rows, each a list of cell strings.</param>
/// <param name="RowLines">One-based line of each body row, parallel to <see cref="Rows"/>.</param>
public sealed record SpecSection(
    string Title,
    int Line,
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    IReadOnlyList<int> RowLines);
