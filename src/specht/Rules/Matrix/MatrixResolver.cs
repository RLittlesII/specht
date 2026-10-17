using System.Text.RegularExpressions;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules.Matrix;

/// <summary>Resolves matrix test names once from the parsed document and declared file texts, without I/O or diagnostics.</summary>
public sealed class MatrixResolver
{
    /// <summary>Reads the configured matrix columns by position and resolves their inline-code test names.</summary>
    /// <param name="document">The specification already parsed by the model.</param>
    /// <param name="structure">The selected manifest's roles and test settings.</param>
    /// <param name="files">The declared file texts read once by the discovery store and shared by all consumers.</param>
    /// <returns>The matrix rows, in document order, with root-relative resolution evidence.</returns>
    public IReadOnlyList<MatrixRowResolution> Resolve(
        SpecDocument document, SpecStructure structure, IReadOnlyList<SpecTestFile> files)
    {
        var section = document.Section(structure.Roles["matrix"]);
        if (section is null)
        {
            return [];
        }

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

            rows.Add(new MatrixRowResolution(
                section.RowLines[row], cells.ElementAtOrDefault(0) ?? string.Empty, cells.ElementAtOrDefault(1) ?? string.Empty,
                cells.ElementAtOrDefault(2) ?? string.Empty, cells.ElementAtOrDefault(3) ?? string.Empty, tests.ToArray()));
        }

        return rows.ToArray();
    }

    private static MatrixTestResolution ResolveTest(string name, SpecTestSettings? settings, IReadOnlyList<SpecTestFile> files)
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

    private static Regex Declaration(string form, string name)
    {
        var halves = form.Split("{test}", StringSplitOptions.None);
        var before = halves[0].Length == 0 ? @"(?<!\w)" : Regex.Escape(halves[0]);
        var after = halves[1].Length == 0 ? @"(?!\w)" : Regex.Escape(halves[1]);
        return new Regex(before + "(?<test>" + Regex.Escape(name) + ")" + after);
    }

    private static string Word(string name) => @"(?<!\w)" + Regex.Escape(name) + @"(?!\w)";
}
