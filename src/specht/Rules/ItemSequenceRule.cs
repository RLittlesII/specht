using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// SPEC044 - item ids are globally unique, and each epic's item sequence runs contiguously from 01; without an epic tier,
/// each Feature's.
/// </summary>
/// <remarks>
/// Where the manifest declares the epic grammar, tasks are numbered per EPIC, not per Feature, which is why uniqueness
/// and contiguity are checked across the epic rather than within the directory that happens to hold the file. Without
/// it, from schema 0.3.0, they are numbered per the Feature whose folder holds them (B-032, decision 0020).
/// </remarks>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class ItemSequenceRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC044";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        foreach (var finding in CheckReuse(model))
        {
            yield return finding;
        }

        foreach (var finding in CheckSequences(model))
        {
            yield return finding;
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static int Number(string id)
    {
        var match = Regex.Match(id, @"^\d{4}-(\d{2})$");

        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private IEnumerable<SpechtFinding> CheckReuse(SpechtModel model)
    {
        var seen = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var item in model.Items)
        {
            if (item.Id is null)
            {
                continue;
            }

            if (!seen.TryAdd(item.Id, item.RelativePath))
            {
                yield return new SpechtFinding(
                    item.RelativePath,
                    item.Frontmatter.LineOf(item.Keys.Id),
                    item.Id,
                    $"item id '{item.Id}' is already used by {seen[item.Id]} - ids are never reused")
                {
                    Expected = new JsonObject { ["fileName"] = $"{item.Id}-*.md", ["firstDeclaration"] = seen[item.Id] },
                };
            }
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F5" claim="B-006" />
    private IEnumerable<SpechtFinding> CheckSequences(SpechtModel model) =>
        model.Schemas.Structure.Forms is null ? CheckLegacySequences(model) : CheckGrammarSequences(model);

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private IEnumerable<SpechtFinding> CheckLegacySequences(SpechtModel model)
    {
        var byEpic = model.Items
            .Where(static item => item.Id is not null)
            .GroupBy(static item => item.Id![..4], StringComparer.Ordinal);

        foreach (var group in byEpic)
        {
            var numbers = group
                .Select(static item => (Item: item, Number: Number(item.Id!)))
                .Where(static entry => entry.Number > 0)
                .OrderBy(static entry => entry.Number)
                .ToList();

            for (var index = 0; index < numbers.Count; index++)
            {
                var expected = index + 1;

                if (numbers[index].Number != expected)
                {
                    yield return new SpechtFinding(
                        numbers[index].Item.RelativePath,
                        numbers[index].Item.Frontmatter.LineOf(numbers[index].Item.Keys.Id),
                        numbers[index].Item.Id,
                        $"epic {group.Key}'s item sequence skips {expected:D2} - tasks are numbered per epic, contiguously from 01")
                    {
                        Expected = new JsonObject { ["fileName"] = $"{group.Key}-{expected:D2}-*.md", ["nextNumber"] = expected },
                    };

                    break;
                }
            }
        }
    }

    /// <summary>
    /// Reads each item's sequence from the task grammar's named groups, and pads a missing number to the width of the
    /// sequence the reported item carries (B-006, B-053): grouped by the grammar's epic under the epic grammar, and
    /// otherwise by the Feature whose folder holds the item, an item no Feature's folder holds left out (B-032, B-070,
    /// B-071).
    /// </summary>
    /// <implements epic="0001" feature="F5" claim="B-006" />
    /// <implements epic="0001" feature="F5" claim="B-032" />
    /// <implements epic="0001" feature="F5" claim="B-053" />
    /// <implements epic="0001" feature="F5" claim="B-070" />
    /// <implements epic="0001" feature="F5" claim="B-071" />
    private IEnumerable<SpechtFinding> CheckGrammarSequences(SpechtModel model)
    {
        var grammar = new Regex(model.Schemas.Structure.Identifiers["task"], RegexOptions.CultureInvariant);
        var epicLess = model.Schemas.Structure.EpicLess;
        var holders = model.Features
            .GroupBy(static feature => feature.Location.Directory, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.First().Identity, StringComparer.Ordinal);
        var bySequence = model.Items
            .Where(static item => item.Id is not null)
            .Select(item => (Item: item, Match: grammar.Match(item.Id!)))
            .Where(static entry => entry.Match.Success && entry.Match.Groups["sequence"].Success)
            .Where(entry => !epicLess || holders.ContainsKey(entry.Item.ParentDirectory))
            .GroupBy(entry => epicLess ? entry.Item.ParentDirectory : entry.Match.Groups["epic"].Value, StringComparer.Ordinal);
        var numbering = epicLess ? "Feature" : "epic";

        foreach (var group in bySequence)
        {
            var owner = epicLess ? $"Feature {holders[group.Key]}" : $"epic {group.Key}";
            var numbers = group
                .Select(static entry => (entry.Item, Sequence: entry.Match.Groups["sequence"], Number: int.TryParse(
                    entry.Match.Groups["sequence"].ValueSpan, NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : 0))
                .Where(static entry => entry.Number > 0)
                .OrderBy(static entry => entry.Number)
                .ToList();

            for (var index = 0; index < numbers.Count; index++)
            {
                var expected = index + 1;
                var (item, sequence, number) = numbers[index];

                if (number != expected)
                {
                    var missing = expected.ToString(CultureInfo.InvariantCulture).PadLeft(sequence.Length, '0');
                    var id = item.Id![..sequence.Index] + missing + item.Id[(sequence.Index + sequence.Length)..];

                    yield return new SpechtFinding(
                        item.RelativePath,
                        item.Frontmatter.LineOf(item.Keys.Id),
                        item.Id,
                        $"{owner}'s item sequence skips {missing} - tasks are numbered per {numbering}, contiguously from 01")
                    {
                        Expected = new JsonObject { ["fileName"] = $"{id}-*.md", ["nextNumber"] = expected },
                    };

                    break;
                }
            }
        }
    }
}
