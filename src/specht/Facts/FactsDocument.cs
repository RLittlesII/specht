using System.Globalization;
using System.Text.Json;

using Specht.Model;

namespace Specht.Facts;

/// <summary>
/// The facts document (<c>0132-F2</c>): the shape <c>docs/schema/facts.schema.json</c> publishes, projected from one
/// <see cref="SpechtModel"/> and the records beside its Features. No rule runs, so it carries no violation, count or
/// verdict (B-014), and it is a function of what it is given alone, so nothing in it reads the clock, the machine or the
/// environment (B-022).
/// </summary>
/// <param name="Specifications">Every discovered Feature specification, in discovery order.</param>
/// <implements epic="0132" feature="F2" claim="B-001" />
/// <implements epic="0132" feature="F2" claim="B-014" />
public sealed record FactsDocument(IReadOnlyList<FeatureFacts> Specifications)
{
    /// <summary>
    /// Makes the document for <paramref name="model"/> and the <paramref name="records"/> beside its Features: every claim,
    /// constraint, out-of-scope, open-question and sign-off row and every lessons-and-deltas entry is given, none judged
    /// (B-013), each section read alone so a missing one or one with no table empties only its own member (B-015), the
    /// claims and sign-off sections and the sign-off headers read under the names the manifest gives (B-016), every record
    /// is given whatever its file holds (B-011), and its epic files, traceability matrices and test files are not read
    /// (B-025, B-026).
    /// </summary>
    /// <param name="model">The model the check's own read gives.</param>
    /// <param name="records">The records beside each Feature; a Feature the map lacks has none.</param>
    /// <returns>The document.</returns>
    /// <implements epic="0132" feature="F2" claim="B-001" />
    /// <implements epic="0132" feature="F2" claim="B-003" />
    /// <implements epic="0132" feature="F2" claim="B-004" />
    /// <implements epic="0132" feature="F2" claim="B-005" />
    /// <implements epic="0132" feature="F2" claim="B-006" />
    /// <implements epic="0132" feature="F2" claim="B-007" />
    /// <implements epic="0132" feature="F2" claim="B-008" />
    /// <implements epic="0132" feature="F2" claim="B-009" />
    /// <implements epic="0132" feature="F2" claim="B-010" />
    /// <implements epic="0132" feature="F2" claim="B-011" />
    /// <implements epic="0132" feature="F2" claim="B-012" />
    /// <implements epic="0132" feature="F2" claim="B-013" />
    /// <implements epic="0132" feature="F2" claim="B-015" />
    /// <implements epic="0132" feature="F2" claim="B-016" />
    /// <implements epic="0132" feature="F2" claim="B-017" />
    /// <implements epic="0132" feature="F2" claim="B-025" />
    /// <implements epic="0132" feature="F2" claim="B-026" />
    public static FactsDocument From(SpechtModel model, IReadOnlyDictionary<FeatureSpec, IReadOnlyList<RecordFile>> records)
    {
        var identified = model.FeaturesWithIdentity.ToHashSet();
        var structure = model.Schemas.Structure;
        var listed = structure.Sections;
        var signOffHeaders = structure.Tables.TryGetValue(SignOffRole, out var named) ? named : EngineSignOffHeaders;
        return new(model.Features
            .Where(static feature => feature.ClaimFileSet is null)
            .Select(feature => new FeatureFacts(
                feature.RelativePath,
                identified.Contains(feature) ? feature.Identity : null,
                feature.SpecStatus,
                feature.Status,
                feature.Strings(feature.Keys.DependsOn),
                feature.Strings(feature.Keys.Blocks),
                feature.Strings(feature.Keys.Children),
                Claims(model.Claims[feature]),
                Constraints(Listed(listed, feature, ConstraintsTitle)),
                OutOfScope(Listed(listed, feature, OutOfScopeTitle)),
                OpenQuestions(Listed(listed, feature, OpenQuestionsTitle)),
                records.TryGetValue(feature, out var beside) ? Records(beside) : [],
                Deltas(Listed(listed, feature, DeltasTitle)),
                SignOff(feature.Document.Section(structure.Roles[SignOffRole]), signOffHeaders)))
            .ToList());
    }

    /// <summary>Serializes the document as the JSON the published facts schema describes, every member written.</summary>
    /// <returns>The JSON text.</returns>
    /// <implements epic="0132" feature="F2" claim="B-002" />
    /// <implements epic="0132" feature="F2" claim="B-022" />
    public string ToJson() => JsonSerializer.Serialize(this, Options);

    /// <summary>
    /// Gives the document holding the specifications <paramref name="path"/> selects, in this document's order: the one whose
    /// path is the text, or every one under the directory it names by whole path segments, every trailing <c>/</c> removed
    /// first. Any other text selects none.
    /// </summary>
    /// <param name="path">A root-relative, <c>/</c>-separated path, compared ordinally.</param>
    /// <returns>The selected document, with no specification when the path selects none.</returns>
    /// <implements epic="0132" feature="F2" claim="B-018" />
    /// <implements epic="0132" feature="F2" claim="B-019" />
    /// <implements epic="0132" feature="F2" claim="B-020" />
    public FactsDocument Select(string path)
    {
        var trimmed = path.TrimEnd('/');
        var directory = trimmed + "/";
        return new(Specifications
            .Where(specification => string.Equals(specification.Path, trimmed, StringComparison.Ordinal)
                || specification.Path.StartsWith(directory, StringComparison.Ordinal))
            .ToList());
    }

    /// <summary>
    /// Gives one line per claim, constraint, out-of-scope row and open question, specification by specification in this
    /// document's order and within one in that order of kinds, each five tab-separated fields: path, kind, id, line and
    /// text (B-024, OQ-5's default). A missing text is an empty field, and a tab, line feed or carriage return inside a
    /// field is written as one space.
    /// </summary>
    /// <returns>The lines, with no line terminator.</returns>
    /// <implements epic="0132" feature="F2" claim="B-024" />
    public IReadOnlyList<string> Lines() =>
        Specifications
            .SelectMany(static specification => specification.Claims
                .Select(claim => Line(specification.Path, ClaimKind, claim.Id, claim.Line, claim.Text))
                .Concat(specification.Constraints
                    .Select(constraint => Line(specification.Path, ConstraintKind, constraint.Id, constraint.Line, constraint.Text)))
                .Concat(specification.OutOfScope
                    .Select(row => Line(specification.Path, OutOfScopeKind, row.Number, row.Line, row.Item)))
                .Concat(specification.OpenQuestions
                    .Select(question => Line(specification.Path, OpenQuestionKind, question.Id, question.Line, question.Question))))
            .ToList();

    /// <implements epic="0132" feature="F2" claim="B-005" />
    /// <implements epic="0132" feature="F2" claim="B-012" />
    /// <implements epic="0132" feature="F2" claim="B-013" />
    private static List<ClaimFacts> Claims(SpechtClaims claims) =>
        claims.Section is not { } section
            ? []
            : claims.Rows
                .Select((claim, row) => new ClaimFacts(claim.Id, Cell(section, row, "Claim"), Cell(section, row, "Status"), claim.Line))
                .ToList();

    /// <implements epic="0132" feature="F2" claim="B-006" />
    private static List<ConstraintFacts> Constraints(SpechtSection? section) =>
        section is null
            ? []
            : section.Rows
                .Select((cells, row) => new ConstraintFacts(
                    FirstCell(cells),
                    Cell(section, row, "Constraint"),
                    Cell(section, row, "Rules Out"),
                    section.RowLines[row]))
                .ToList();

    /// <implements epic="0132" feature="F2" claim="B-007" />
    /// <implements epic="0132" feature="F2" claim="B-015" />
    private static List<OutOfScopeFacts> OutOfScope(SpechtSection? section) =>
        section is null
            ? []
            : section.Rows
                .Select((cells, row) => new OutOfScopeFacts(
                    FirstCell(cells),
                    Cell(section, row, "Item"),
                    Cell(section, row, "Exclusion Reason"),
                    section.RowLines[row]))
                .ToList();

    /// <implements epic="0132" feature="F2" claim="B-008" />
    /// <implements epic="0132" feature="F2" claim="B-015" />
    private static List<OpenQuestionFacts> OpenQuestions(SpechtSection? section) =>
        section is null
            ? []
            : section.Rows
                .Select((cells, row) =>
                {
                    var resolution = Cell(section, row, "Resolution");
                    return new OpenQuestionFacts(
                        FirstCell(cells),
                        Cell(section, row, "Question"),
                        Cell(section, row, "Blocks"),
                        resolution,
                        Resolved(resolution),
                        section.RowLines[row]);
                })
                .ToList();

    /// <implements epic="0132" feature="F2" claim="B-009" />
    /// <implements epic="0132" feature="F2" claim="B-015" />
    private static List<DeltaFacts> Deltas(SpechtSection? section) =>
        section is null
            ? []
            : section.Items
                .Select((text, item) => new DeltaFacts(text, [], section.ItemLines[item]))
                .Concat(section.Rows.Select((cells, row) => new DeltaFacts(null, cells, section.RowLines[row])))
                .OrderBy(static entry => entry.Line)
                .ToList();

    /// <implements epic="0132" feature="F2" claim="B-010" />
    /// <implements epic="0132" feature="F2" claim="B-015" />
    /// <implements epic="0132" feature="F2" claim="B-016" />
    private static List<SignOffFacts> SignOff(SpechtSection? section, IReadOnlyList<string> headers) =>
        section is null
            ? []
            : section.Rows
                .Select((_, row) => new SignOffFacts(
                    Header(section, row, headers, 0),
                    Header(section, row, headers, 1),
                    Header(section, row, headers, 2),
                    Header(section, row, headers, 3),
                    section.RowLines[row]))
                .ToList();

    /// <implements epic="0132" feature="F2" claim="B-010" />
    /// <implements epic="0132" feature="F2" claim="B-016" />
    private static string? Header(SpechtSection section, int row, IReadOnlyList<string> headers, int place) =>
        place < headers.Count ? Cell(section, row, headers[place]) : null;

    /// <implements epic="0132" feature="F2" claim="B-011" />
    private static List<RecordFacts> Records(IReadOnlyList<RecordFile> records) =>
        records
            .Select(static record => new RecordFacts(
                record.Kind,
                FirstWord(record.RelativePath[(record.RelativePath.LastIndexOf('/') + 1)..]),
                record.Frontmatter.Text("title"),
                RecordStatus(record),
                record.RelativePath))
            .ToList();

    /// <implements epic="0132" feature="F2" claim="B-011" />
    private static string? RecordStatus(RecordFile record) =>
        string.Equals(record.Kind, AdrKind, StringComparison.Ordinal) ? FirstWord(record.StatusLine) : null;

    /// <implements epic="0132" feature="F2" claim="B-008" />
    private static bool Resolved(string? resolution) =>
        !string.IsNullOrEmpty(resolution) && !string.Equals(FirstWord(resolution), OpenMarker, StringComparison.Ordinal);

    /// <implements epic="0132" feature="F2" claim="B-008" />
    /// <implements epic="0132" feature="F2" claim="B-011" />
    private static string? FirstWord(string? text) =>
        string.Concat((text ?? string.Empty).TakeWhile(char.IsLetterOrDigit)) is { Length: > 0 } word ? word : null;

    /// <implements epic="0132" feature="F2" claim="B-006" />
    /// <implements epic="0132" feature="F2" claim="B-007" />
    /// <implements epic="0132" feature="F2" claim="B-008" />
    /// <implements epic="0132" feature="F2" claim="B-009" />
    /// <implements epic="0132" feature="F2" claim="B-015" />
    private static SpechtSection? Listed(IReadOnlyList<string> listed, FeatureSpec feature, string title) =>
        listed.Contains(title, StringComparer.Ordinal) ? feature.Document.Section(title) : null;

    /// <implements epic="0132" feature="F2" claim="B-006" />
    /// <implements epic="0132" feature="F2" claim="B-007" />
    /// <implements epic="0132" feature="F2" claim="B-008" />
    private static string FirstCell(IReadOnlyList<string> cells) => cells.Count == 0 ? string.Empty : cells[0];

    /// <implements epic="0132" feature="F2" claim="B-024" />
    private static string Line(string path, string kind, string id, int line, string? text) =>
        string.Join('\t', Field(path), kind, Field(id), line.ToString(CultureInfo.InvariantCulture), Field(text));

    /// <implements epic="0132" feature="F2" claim="B-024" />
    private static string Field(string? text) =>
        (text ?? string.Empty).Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');

    /// <implements epic="0132" feature="F2" claim="B-005" />
    /// <implements epic="0132" feature="F2" claim="B-006" />
    /// <implements epic="0132" feature="F2" claim="B-007" />
    /// <implements epic="0132" feature="F2" claim="B-008" />
    /// <implements epic="0132" feature="F2" claim="B-010" />
    private static string? Cell(SpechtSection section, int row, string header)
    {
        var index = section.Headers.ToList().IndexOf(header);
        var cells = section.Rows[row];
        return index >= 0 && index < cells.Count ? cells[index] : null;
    }

    private const string ConstraintsTitle = "4. Constraints";

    private const string OutOfScopeTitle = "5. Out of Scope";

    private const string OpenQuestionsTitle = "11. Open Questions";

    private const string DeltasTitle = "10. Lessons / Spec Deltas";

    private const string SignOffRole = "signOff";

    private const string OpenMarker = "Open";

    private const string AdrKind = "adr";

    private const string ClaimKind = "claim";

    private const string ConstraintKind = "constraint";

    private const string OutOfScopeKind = "outOfScope";

    private const string OpenQuestionKind = "openQuestion";

    private static readonly IReadOnlyList<string> EngineSignOffHeaders = ["Section", "Status", "Reviewer", "Note"];

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        NewLine = "\n",
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
