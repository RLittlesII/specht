using System.Text.RegularExpressions;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules.Matrix;

/// <summary>Resolves matrix test names and Scenario cell titles once from parsed text and declared files, without I/O or diagnostics.</summary>
/// <implements epic="0001" feature="F12" claim="B-001" />
public sealed class MatrixResolver
{
    /// <summary>Joins declared claims to their resolved matrix rows and already-parsed companion tags.</summary>
    /// <param name="feature">The specification and its parsed companion snapshot.</param>
    /// <param name="structure">The selected roles and tag-case setting.</param>
    /// <param name="claims">The model's already-derived claim rows.</param>
    /// <returns>Claim resolutions in declaration order.</returns>
    /// <implements epic="0001" feature="F12" claim="B-001" />
    public IReadOnlyList<MatrixClaimResolution> ResolveClaims(FeatureSpec feature, SpechtStructure structure, SpechtClaims claims)
    {
        var rows = Resolve(feature, structure, []).ToLookup(static row => row.Claim.Trim(), StringComparer.Ordinal);
        var comparer = structure.ClaimTagIgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var tags = feature.FeatureFiles.SelectMany(static file => file.Tags).ToLookup(static tag => tag.Id, comparer);
        return claims.Rows.Select((claim, index) => new MatrixClaimResolution(
            claim, claims.Section!.Rows[index].ElementAtOrDefault(3)?.Trim() ?? string.Empty,
            rows[claim.Id].ToArray(), tags[claim.Id].ToArray())).ToArray();
    }

    /// <summary>Reads the matrix columns by position and resolves their test names and their Scenario cell titles.</summary>
    /// <param name="feature">The specification and its parsed companion snapshot.</param>
    /// <param name="structure">The selected manifest's roles, tag-case, title and test settings.</param>
    /// <param name="files">The declared file texts read once by the discovery store and shared by all consumers.</param>
    /// <returns>The matrix rows, in document order, with root-relative resolution evidence.</returns>
    /// <implements epic="0001" feature="F12" claim="B-008" />
    public IReadOnlyList<MatrixRowResolution> Resolve(FeatureSpec feature, SpechtStructure structure, IReadOnlyList<SpechtTestFile> files) =>
        Resolve(feature.Document, structure, files, feature.FeatureFiles);

    /// <summary>Reads the matrix columns by position and resolves their test names, with no companion to hold a title.</summary>
    /// <param name="document">The specification already parsed by the model.</param>
    /// <param name="structure">The selected manifest's roles and test settings.</param>
    /// <param name="files">The declared file texts read once by the discovery store and shared by all consumers.</param>
    /// <returns>The matrix rows, in document order, with root-relative resolution evidence.</returns>
    /// <implements epic="0001" feature="F12" claim="B-001" />
    public IReadOnlyList<MatrixRowResolution> Resolve(
        SpechtDocument document, SpechtStructure structure, IReadOnlyList<SpechtTestFile> files) =>
        Resolve(document, structure, files, []);

    /// <implements epic="0001" feature="F12" claim="B-001" />
    private static MatrixRowResolution[] Resolve(
        SpechtDocument document, SpechtStructure structure, IReadOnlyList<SpechtTestFile> files, IReadOnlyList<FeatureFile> companions)
    {
        var section = document.Section(structure.Roles["matrix"]);
        if (section is null)
        {
            return [];
        }

        var comparer = structure.ClaimTagIgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var named = companions
            .SelectMany(static file => file.Scenarios, static (file, scenario) => (
                Evidence: new MatrixScenarioResolution(scenario.Title, MatrixScenarioStatus.Resolved, file.RelativePath, scenario.Line),
                scenario.Tags))
            .ToLookup(static entry => entry.Evidence.Title, StringComparer.Ordinal);
        var ordered = files.OrderBy(static file => file.RelativePath, StringComparer.Ordinal).ToArray();
        var cache = new Dictionary<string, MatrixTestResolution>(StringComparer.Ordinal);
        var rows = new List<MatrixRowResolution>();
        for (var row = 0; row < section.Rows.Count; row++)
        {
            var cells = section.Rows[row];
            var spans = section.RowCodeSpans.ElementAtOrDefault(row)?.ElementAtOrDefault(2) ?? [];
            var tests = new List<MatrixTestResolution>();
            foreach (var name in spans.SelectMany(static span =>
                span.Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)))
            {
                if (!cache.TryGetValue(name, out var resolution))
                {
                    resolution = ResolveTest(name, structure.Tests, ordered);
                    cache.Add(name, resolution);
                }

                tests.Add(resolution);
            }

            var claim = cells.ElementAtOrDefault(0) ?? string.Empty;
            var scenario = cells.ElementAtOrDefault(1) ?? string.Empty;
            var titles = scenario.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(title => ResolveTitle(title, claim.Trim(), structure.MatrixScenarioTitles, named, comparer))
                .ToArray();
            rows.Add(new MatrixRowResolution(
                section.RowLines[row], claim, scenario,
                cells.ElementAtOrDefault(2) ?? string.Empty, cells.ElementAtOrDefault(3) ?? string.Empty, tests.ToArray(), titles));
        }

        return rows.ToArray();
    }

    /// <implements epic="0001" feature="F12" claim="B-009" />
    private static MatrixScenarioResolution ResolveTitle(
        string title, string claim, bool matching,
        ILookup<string, (MatrixScenarioResolution Evidence, IReadOnlyList<FeatureTag> Tags)> named, StringComparer comparer)
    {
        if (!matching)
        {
            return new MatrixScenarioResolution(title, MatrixScenarioStatus.Unconfigured);
        }

        var scenarios = named[title].ToArray();
        if (scenarios.Length == 0)
        {
            return new MatrixScenarioResolution(title, MatrixScenarioStatus.NoSuchTitle);
        }

        var tagged = scenarios.Where(scenario => scenario.Tags.Any(tag => comparer.Equals(tag.Id, claim))).Select(static scenario => scenario.Evidence);
        return tagged.FirstOrDefault() ?? scenarios[0].Evidence with { Status = MatrixScenarioStatus.Untagged };
    }

    /// <implements epic="0001" feature="F12" claim="B-001" />
    private static MatrixTestResolution ResolveTest(string name, SpechtTestSettings? settings, IReadOnlyList<SpechtTestFile> files)
    {
        if (settings is null)
        {
            return new MatrixTestResolution(name, MatrixTestStatus.Unconfigured);
        }

        var parts = name.Split('.');
        if (parts.Any(static part => part.Length == 0))
        {
            return new MatrixTestResolution(name, MatrixTestStatus.Unresolved);
        }

        var qualifiers = parts[..^1].Select(static part => new Regex(Word(part))).ToArray();
        var forms = settings.Declarations.Count == 0 ? ["{test}"] : settings.Declarations;
        var declarations = forms.Select(form => Declaration(form, parts[^1])).ToArray();
        foreach (var file in files)
        {
            if (qualifiers.Any(qualifier => !qualifier.IsMatch(file.Text)))
            {
                continue;
            }

            var found = declarations.Select(declaration => declaration.Match(file.Text))
                .Where(static match => match.Success)
                .Select(static match => match.Groups["test"])
                .OrderBy(static group => group.Index)
                .FirstOrDefault();
            if (found is not null)
            {
                var line = 1;
                for (var index = 0; index < found.Index; index++)
                {
                    if (file.Text[index] == '\n' || file.Text[index] == '\r' && (index + 1 == found.Index || file.Text[index + 1] != '\n'))
                    {
                        line++;
                    }
                }

                return new MatrixTestResolution(name, MatrixTestStatus.Resolved, file.RelativePath, line);
            }
        }

        return new MatrixTestResolution(name, MatrixTestStatus.Unresolved);
    }

    /// <implements epic="0001" feature="F12" claim="B-001" />
    private static Regex Declaration(string form, string name)
    {
        var halves = form.Split("{test}", StringSplitOptions.None);
        var before = halves[0].Length == 0 ? @"(?<!\w)" : Regex.Escape(halves[0]);
        var after = halves[1].Length == 0 ? @"(?!\w)" : Regex.Escape(halves[1]);
        return new Regex(before + "(?<test>" + Regex.Escape(name) + ")" + after);
    }

    /// <implements epic="0001" feature="F12" claim="B-001" />
    private static string Word(string name) => @"(?<!\w)" + Regex.Escape(name) + @"(?!\w)";
}
