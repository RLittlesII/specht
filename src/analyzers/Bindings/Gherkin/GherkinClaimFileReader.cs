using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Specht.Analysis.Bindings.Gherkin;

/// <summary>Reads claim declarations, cases and findings from English Gherkin text, line by line, with no I/O.</summary>
/// <implements epic="0201" feature="F7" claim="B-008" />
public sealed class GherkinClaimFileReader
{
    public GherkinClaimFileReader(GherkinClaimPolicy policy) => Policy = policy;

    public GherkinClaimPolicy Policy { get; }

    public GherkinClaimFile Read(string relativePath, string text)
    {
        var lines = new List<string>();
        using (var reader = new StringReader(text))
        {
            while (reader.ReadLine() is { } line)
            {
                lines.Add(line.Trim());
            }
        }

        CheckLanguage(relativePath, lines);
        var document = new Document(this, relativePath);
        for (var index = 0; index < lines.Count; index++)
        {
            document.Add(lines[index], index + 1);
        }

        return document.Complete();
    }

    private static void CheckLanguage(string relativePath, IReadOnlyList<string> lines)
    {
        foreach (var line in lines)
        {
            if (line.Length == 0 || line[0] == '@')
            {
                continue;
            }

            if (line[0] != '#')
            {
                return;
            }

            var comment = line.Substring(1).TrimStart();
            if (comment.StartsWith("language", StringComparison.Ordinal))
            {
                var rest = comment.Substring("language".Length).TrimStart();
                if (rest.StartsWith(":", StringComparison.Ordinal) && rest.Substring(1).Trim() != "en")
                {
                    throw new GherkinInputException(relativePath, "Claim file declares a Gherkin language other than English.");
                }
            }
        }
    }

    private sealed record Tag(string Text, int Line);

    private sealed class Element(BindingClaimElement kind, int line, IReadOnlyList<Tag> tags)
    {
        public BindingClaimElement Kind { get; set; } = kind;
        public int Line { get; } = line;
        public IReadOnlyList<Tag> Tags { get; } = tags;
        public List<string> Ids { get; } = [];
    }

    private sealed class Examples(IReadOnlyList<Tag> tags)
    {
        public IReadOnlyList<Tag> Tags { get; } = tags;
        public int Header { get; set; } = -1;
        public List<int> Rows { get; } = [];
    }

    private sealed class Case(Element self, Element? rule, string title)
    {
        public Element Self { get; } = self;
        public Element? Rule { get; } = rule;
        public string Title { get; } = title;
        public List<Examples> Blocks { get; } = [];
    }

    private sealed class Document(GherkinClaimFileReader owner, string path)
    {
        public void Add(string line, int number)
        {
            if (_docString is not null)
            {
                if (line.StartsWith(_docString, StringComparison.Ordinal))
                {
                    _docString = null;
                }

                return;
            }

            if (line.Length == 0 || line[0] == '#')
            {
                return;
            }

            if (line[0] == '@')
            {
                ReadTags(line, number);
            }
            else if (line.StartsWith("\"\"\"", StringComparison.Ordinal) || line.StartsWith("```", StringComparison.Ordinal))
            {
                Content();
                _docString = line.Substring(0, 3);
            }
            else if (line[0] == '|')
            {
                Content();
                Row(line, number);
            }
            else if (!Keyword(line, number))
            {
                Content();
            }
        }

        public GherkinClaimFile Complete()
        {
            if (_docString is not null || _pending.Count > 0 || _feature is null)
            {
                throw Invalid();
            }

            foreach (var element in _elements)
            {
                foreach (var tag in element.Tags)
                {
                    if (owner.Policy.Claim.Read(tag.Text) is { } id && !element.Ids.Contains(id, StringComparer.Ordinal))
                    {
                        element.Ids.Add(id);
                    }
                }
            }

            var claims = new List<BindingClaim>();
            foreach (var element in _elements.Where(static element => element.Ids.Count > 0))
            {
                var covered = element.Kind == BindingClaimElement.Feature ? _cases
                    : element.Kind == BindingClaimElement.Rule ? _cases.Where(item => item.Rule == element)
                    : _cases.Where(item => item.Self == element);
                var cases = Array.AsReadOnly(covered.SelectMany(Cases).ToArray());
                foreach (var id in element.Ids)
                {
                    claims.Add(new BindingClaim(id, State(element), element.Line, RelativePath: path, Element: element.Kind, Cases: cases));
                }
            }

            foreach (var item in _cases.Where(item => item.Self.Ids.Count == 0 && item.Rule?.Ids.Count is null or 0 && _feature.Ids.Count == 0))
            {
                _findings.Add(new BindingFinding("SPCT0012", path, item.Self.Line,
                    (item.Self.Kind == BindingClaimElement.ScenarioOutline ? "Scenario Outline" : "Scenario") + " \"" + item.Title + "\" declares no claim."));
            }

            return new GherkinClaimFile(path, Array.AsReadOnly(claims.ToArray()),
                Array.AsReadOnly(_findings.OrderBy(static finding => finding.Line)
                    .ThenBy(static finding => finding.DiagnosticId, StringComparer.Ordinal).ToArray()),
                Array.AsReadOnly(_featureValues.ToArray()), Array.AsReadOnly(_epicValues.ToArray()));
        }

        private static string? Match(string line, string keyword) =>
            line.StartsWith(keyword, StringComparison.Ordinal) ? line.Substring(keyword.Length).Trim() : null;

        private static int Cells(string row)
        {
            var pipes = 0;
            for (var index = 0; index < row.Length; index++)
            {
                if (row[index] == '\\')
                {
                    index++;
                }
                else if (row[index] == '|')
                {
                    pipes++;
                }
            }

            return pipes - 1;
        }

        private static void AddScope(GherkinTagForm? form, Tag tag, List<string> values)
        {
            if (form?.Read(tag.Text) is { } value)
            {
                values.Add(value);
            }
        }

        private bool Keyword(string line, int number)
        {
            if (Match(line, "Feature:") is not null)
            {
                if (_feature is not null)
                {
                    throw Invalid();
                }

                _feature = Open(BindingClaimElement.Feature, number);
                foreach (var tag in _feature.Tags)
                {
                    AddScope(owner.Policy.FeatureScope, tag, _featureValues);
                    AddScope(owner.Policy.EpicScope, tag, _epicValues);
                }

                return true;
            }

            if (_feature is null)
            {
                throw Invalid();
            }

            if (Match(line, "Rule:") is not null)
            {
                _rule = Open(BindingClaimElement.Rule, number);
                _case = null;
                _examples = null;
            }
            else if (Match(line, "Background:") is not null)
            {
                if (_pending.Count > 0)
                {
                    throw Invalid();
                }

                _case = null;
                _examples = null;
            }
            else if ((Match(line, "Scenario Outline:") ?? Match(line, "Scenario Template:") ?? Match(line, "Scenario:")
                ?? Match(line, "Example:")) is { } title)
            {
                var kind = line.StartsWith("Scenario Outline:", StringComparison.Ordinal) || line.StartsWith("Scenario Template:", StringComparison.Ordinal)
                    ? BindingClaimElement.ScenarioOutline : BindingClaimElement.Scenario;
                _case = new Case(Open(kind, number), _rule, title);
                _cases.Add(_case);
                _examples = null;
            }
            else if (Match(line, "Examples:") is not null || Match(line, "Scenarios:") is not null)
            {
                if (_case is null)
                {
                    throw Invalid();
                }

                _examples = new Examples(Take());
                _case.Blocks.Add(_examples);
                _case.Self.Kind = BindingClaimElement.ScenarioOutline;
                foreach (var tag in _examples.Tags.Where(tag => owner.Policy.Claim.Read(tag.Text) is not null))
                {
                    _findings.Add(new BindingFinding("SPCT0011", path, tag.Line,
                        "Tag " + tag.Text + " declares a claim where no claim can be declared."));
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private Element Open(BindingClaimElement kind, int number)
        {
            var element = new Element(kind, number, Take());
            _elements.Add(element);
            return element;
        }

        private IReadOnlyList<Tag> Take()
        {
            var tags = _pending.ToArray();
            _pending.Clear();
            return tags;
        }

        private void ReadTags(string line, int number)
        {
            foreach (var token in line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                if (token[0] == '#')
                {
                    return;
                }

                if (token[0] != '@' || token.Length < 2)
                {
                    throw Invalid();
                }

                _pending.Add(new Tag(token, number));
            }
        }

        private void Row(string line, int number)
        {
            if (_examples is null)
            {
                return;
            }

            var cells = Cells(line);
            if (_examples.Header < 0)
            {
                _examples.Header = cells;
            }
            else if (cells != _examples.Header)
            {
                throw Invalid();
            }
            else
            {
                _examples.Rows.Add(number);
            }
        }

        private IEnumerable<BindingClaimCase> Cases(Case item)
        {
            if (item.Blocks.Count == 0)
            {
                yield return new BindingClaimCase(item.Self.Line, Planned(item, null));
                yield break;
            }

            foreach (var block in item.Blocks)
            {
                foreach (var row in block.Rows)
                {
                    yield return new BindingClaimCase(row, Planned(item, block));
                }
            }
        }

        private bool Planned(Case item, Examples? block) => new[] { _feature!, item.Rule, item.Self }.OfType<Element>()
            .SelectMany(static element => element.Tags).Concat(block?.Tags ?? [])
            .Any(tag => owner.Policy.PlannedTags.Contains(tag.Text, StringComparer.Ordinal));

        private string State(Element element) =>
            element.Tags.Any(tag => owner.Policy.SupersededTags.Contains(tag.Text, StringComparer.Ordinal)) ? "Superseded"
            : element.Tags.Any(tag => owner.Policy.WithdrawnTags.Contains(tag.Text, StringComparer.Ordinal)) ? "Withdrawn" : "Active";

        private void Content()
        {
            if (_feature is null || _pending.Count > 0)
            {
                throw Invalid();
            }
        }

        private GherkinInputException Invalid() => new(path, "Claim file is not valid Gherkin.");

        private readonly List<Tag> _pending = [];
        private readonly List<Element> _elements = [];
        private readonly List<Case> _cases = [];
        private readonly List<BindingFinding> _findings = [];
        private readonly List<string> _featureValues = [];
        private readonly List<string> _epicValues = [];
        private Element? _feature;
        private Element? _rule;
        private Case? _case;
        private Examples? _examples;
        private string? _docString;
    }
}
