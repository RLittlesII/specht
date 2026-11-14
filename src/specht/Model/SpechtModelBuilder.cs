using Specht.Discovery;

namespace Specht.Model;

/// <summary>Builds one immutable <see cref="SpechtModel"/> from the text of what discovery listed; it touches no file system.</summary>
/// <param name="documents">The specification document parser.</param>
/// <param name="frontmatter">The frontmatter parser, for items and epics.</param>
/// <param name="tags">The companion-tag parser.</param>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpechtModelBuilder(SpechtDocumentParser documents, FrontmatterReader frontmatter, FeatureTagParser tags)
{
    /// <summary>
    /// Builds the model of <paramref name="listing"/>, listing Features, items, epics and test files in the listing's
    /// order, each file's text taken from <paramref name="texts"/> by its root-relative path.
    /// </summary>
    /// <param name="schemas">The compiled schemas and the manifest they were selected by.</param>
    /// <param name="listing">What discovery listed.</param>
    /// <param name="texts">The text of every listed file, by root-relative path.</param>
    /// <returns>The model.</returns>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-006" />
    /// <implements epic="0001" feature="F5" claim="B-079" />
    /// <implements epic="0001" feature="F5" claim="B-080" />
    public SpechtModel Build(SpechtSchemas schemas, SpechtListing listing, IReadOnlyDictionary<string, string> texts)
    {
        var structure = schemas.Structure;
        var features = new List<FeatureSpec>();

        for (var index = 0; index < listing.Specifications.Count; index++)
        {
            var location = listing.Specifications[index];
            var document = documents.Parse(texts[location.RelativePath], location.RelativePath);
            var grammar = structure.ClaimGrammar(
                structure.FeatureKeys.ClaimPrefix is { } key ? document.Frontmatter.Text(key) : null);
            features.Add(new FeatureSpec(
                location,
                document,
                listing.Companions[index]
                    .Select(path => tags.Read(path, texts[path], structure, grammar))
                    .ToList(),
                structure.FeatureKeys,
                structure.Forms,
                grammar));
        }

        var items = new List<ChildItem>();
        var shapes = structure.Forms is null ? null : new SpechtTaskShapes(structure.Discovery.TaskFiles, structure.Identifiers["task"]);

        foreach (var path in listing.Items)
        {
            var slash = path.LastIndexOf('/');
            var fileName = path[(slash + 1)..];
            items.Add(new ChildItem(
                path,
                fileName,
                frontmatter.Parse(texts[path]),
                slash < 0 ? string.Empty : path[..slash],
                structure.TaskKeys,
                shapes?.Task(fileName)));
        }

        var epics = listing.Epics.Select(path => new EpicFile(path, frontmatter.Parse(texts[path]))).ToList();
        var tests = listing.TestFiles.Select(path => new SpechtTestFile(path, texts[path])).ToList();

        return new SpechtModel(features, items, epics, schemas, tests);
    }

    /// <summary>
    /// Builds the record at <paramref name="relativePath"/> from its text: its frontmatter, read as an item's is, and the
    /// first line under its <c>## Status</c> heading, whatever its kind (<c>0132-F2</c> B-011).
    /// </summary>
    /// <param name="relativePath">The record's root-relative path.</param>
    /// <param name="kind">The kind its folder gives.</param>
    /// <param name="text">The record's complete text.</param>
    /// <returns>The record.</returns>
    /// <implements epic="0132" feature="F2" claim="B-011" />
    public RecordFile Record(string relativePath, string kind, string text) =>
        new(relativePath, kind, frontmatter.Parse(text), documents.FirstLine(text, StatusTitle));

    private const string StatusTitle = "Status";
}
