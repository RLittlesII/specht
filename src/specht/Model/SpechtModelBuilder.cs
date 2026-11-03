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
    public SpechtModel Build(SpechtSchemas schemas, SpechtListing listing, IReadOnlyDictionary<string, string> texts)
    {
        var structure = schemas.Structure;
        var features = new List<FeatureSpec>();

        for (var index = 0; index < listing.Specifications.Count; index++)
        {
            var location = listing.Specifications[index];
            features.Add(new FeatureSpec(
                location,
                documents.Parse(texts[location.RelativePath], location.RelativePath),
                listing.Companions[index]
                    .Select(path => new FeatureFile(path, tags.Parse(texts[path], structure)))
                    .ToList(),
                structure.FeatureKeys,
                structure.Forms));
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
}
