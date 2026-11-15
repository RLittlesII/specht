using System.Text.RegularExpressions;
using Specht.Discovery;
using Specht.Manifest;

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
    /// <implements epic="0001" feature="F6" claim="B-013" />
    /// <implements epic="0001" feature="F16" claim="B-018" />
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

        features.AddRange(NativeFeatures(structure, listing, texts));

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

        var claimSets = ClaimSets(structure, structure.ClaimInput?.Files ?? [], listing.ClaimFiles, texts);
        return new SpechtModel(features, items, epics, schemas, tests, claimSets)
        {
            OrphanCompanions = listing.OrphanCompanions,
            ClaimFaults = listing.ClaimFaults.Concat(claimSets.SelectMany(static claims => claims.Faults)).Distinct().ToArray(),
        };
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

    /// <summary>
    /// Reads each claim file set's files with the shared Gherkin claim reader, under the manifest's claim tag form, the
    /// tree's claim grammar, the manifest's lifecycle tag lists and its Feature-level scope form; a file whose tags cannot be
    /// read declares nothing, and neither does a file two of <paramref name="sets"/> list. Each set records, as a fault, each
    /// file the reader refused, its glob when it lists no file, each file another set lists, and each Feature-level tag
    /// naming another Feature than its own. The check reads every set of the manifest, and the collision guard the sets of
    /// one Feature (<c>0001-F19</c> B-078).
    /// </summary>
    /// <param name="structure">The effective manifest.</param>
    /// <param name="sets">The claim file sets to read.</param>
    /// <param name="files">The claim files discovery listed for each of <paramref name="sets"/>, in the same order.</param>
    /// <param name="texts">The text of every listed file, by root-relative path.</param>
    /// <returns>The claims of each set, in the order given; empty unless <c>claim.source</c> is <c>gherkin</c>.</returns>
    /// <implements epic="0001" feature="F16" claim="B-001" />
    /// <implements epic="0001" feature="F16" claim="B-004" />
    /// <implements epic="0001" feature="F16" claim="B-005" />
    /// <implements epic="0001" feature="F16" claim="B-012" />
    /// <implements epic="0001" feature="F16" claim="B-013" />
    /// <implements epic="0001" feature="F16" claim="B-014" />
    /// <implements epic="0001" feature="F16" claim="B-015" />
    /// <implements epic="0001" feature="F16" claim="B-016" />
    /// <implements epic="0001" feature="F16" claim="B-018" />
    /// <implements epic="0001" feature="F16" claim="B-019" />
    /// <implements epic="0001" feature="F19" claim="B-078" />
    internal static List<SpechtClaims> ClaimSets(
        SpechtStructure structure,
        IReadOnlyList<SpechtClaimFileSet> sets,
        IReadOnlyList<IReadOnlyList<string>> files,
        IReadOnlyDictionary<string, string> texts)
    {
        if (structure.ClaimInput is not { Source: SpechtClaimSource.Gherkin } input)
        {
            return [];
        }

        var grammar = structure.ClaimGrammar(null);
        var pattern = grammar.LongerScenarioHead ? "(?!" + Regex.Escape(grammar.ScenarioHead!) + ")(?:" + grammar.Pattern + ")" : grammar.Pattern;
        var reader = new GherkinClaimTextReader(new GherkinClaimTags(
            new GherkinClaimTagForm(structure.ClaimTagForm!, "claim", structure.ClaimTagIgnoreCase, pattern),
            FeatureScope(input.FeatureScopeTag),
            null,
            input.PlannedTags,
            input.WithdrawnTags,
            input.SupersededTags));
        var shared = files.SelectMany(static paths => paths.Distinct(StringComparer.Ordinal))
            .GroupBy(static path => path, StringComparer.Ordinal)
            .Where(static group => group.Skip(1).Any())
            .Select(static group => group.Key)
            .ToHashSet(StringComparer.Ordinal);
        var claims = new List<SpechtClaims>();
        for (var index = 0; index < sets.Count; index++)
        {
            var set = sets[index];
            var read = new List<GherkinClaimText>();
            var faults = new List<SpechtClaimFault>();
            if (files[index].Count == 0)
            {
                faults.Add(new SpechtClaimFault(set.Glob, 0, SpechtClaimFaultKind.EmptySet, set.Feature));
            }

            foreach (var path in files[index])
            {
                try
                {
                    var text = reader.Read(path, texts[path]);
                    read.Add(text);
                    faults.AddRange(text.FeatureTagValues.Zip(text.FeatureTagLines ?? [])
                        .Where(tag => !string.Equals(tag.First, set.Feature, StringComparison.Ordinal))
                        .Select(tag => new SpechtClaimFault(path, tag.Second, SpechtClaimFaultKind.FeatureTag, tag.First)));
                }
                catch (GherkinClaimTextException exception)
                {
                    faults.Add(new SpechtClaimFault(path, exception.Line, SpechtClaimFaultKind.UnreadableFile, null));
                }

                if (shared.Contains(path))
                {
                    faults.Add(new SpechtClaimFault(path, 0, SpechtClaimFaultKind.SharedFile, null));
                }
            }

            claims.Add(new SpechtClaims(set, read, structure, shared) { Faults = faults });
        }

        return claims;
    }

    /// <summary>
    /// The reader's Feature-level scope form for <c>claim.scopes.featureTag</c>, or none when the manifest sets none or its
    /// form holds other than exactly one <c>{feature}</c> (<c>0001-F16</c> OQ-20).
    /// </summary>
    /// <param name="form">The form the manifest writes, or null.</param>
    /// <returns>The scope form, or null.</returns>
    /// <implements epic="0001" feature="F16" claim="B-019" />
    private static GherkinClaimTagForm? FeatureScope(string? form) =>
        form?.Split(FeaturePlaceholder).Length == 2 ? new GherkinClaimTagForm(form, "feature", false, null) : null;

    /// <summary>
    /// Makes one Feature of each claim file set that has a location: its document the loaded record, or empty text with no
    /// record; no companion; the grammar of a Feature with no claim prefix whatever the record declares; its identity the
    /// set's; and its lifecycle the record's text at the manifest's lifecycle key, or <c>current</c> with no record.
    /// </summary>
    /// <param name="structure">The effective manifest.</param>
    /// <param name="listing">What discovery listed.</param>
    /// <param name="texts">The text of every listed file and loaded record, by root-relative path.</param>
    /// <returns>The native Features, in the manifest's order of their sets; none unless <c>claim.source</c> is <c>gherkin</c>.</returns>
    /// <implements epic="0001" feature="F16" claim="B-003" />
    /// <implements epic="0001" feature="F16" claim="B-026" />
    /// <implements epic="0001" feature="F16" claim="B-032" />
    /// <implements epic="0001" feature="F16" claim="B-036" />
    /// <implements epic="0001" feature="F16" claim="B-037" />
    private IEnumerable<FeatureSpec> NativeFeatures(SpechtStructure structure, SpechtListing listing, IReadOnlyDictionary<string, string> texts)
    {
        if (structure.ClaimInput is not { Source: SpechtClaimSource.Gherkin } input)
        {
            yield break;
        }

        for (var index = 0; index < input.Files.Count; index++)
        {
            if (listing.ClaimLocations[index] is not { } location)
            {
                continue;
            }

            var record = listing.ClaimRecords[index];
            var document = documents.Parse(record is null ? string.Empty : texts[record], location.RelativePath);
            yield return new FeatureSpec(location, document, [], structure.FeatureKeys, structure.Forms, structure.ClaimGrammar(null))
            {
                ClaimFileSet = input.Files[index],
                Record = record,
                Lifecycle = record is null ? "current" : document.Frontmatter.Text(input.LifecycleKey),
            };
        }
    }

    private const string StatusTitle = "Status";

    private const string FeaturePlaceholder = "{feature}";
}
