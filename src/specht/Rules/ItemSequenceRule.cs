using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC044 - item ids are globally unique, and each epic's item sequence runs contiguously from 01.</summary>
/// <remarks>
/// Tasks are numbered per EPIC, not per Feature, which is why uniqueness and
/// contiguity are checked across the epic rather than within the directory
/// that happens to hold the file.
/// </remarks>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class ItemSequenceRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC044";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => [Id];

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpechtViolation> Evaluate(SpechtModel model)
    {
        foreach (var violation in CheckReuse(model))
        {
            yield return violation;
        }

        foreach (var violation in CheckSequences(model))
        {
            yield return violation;
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static int Number(string id)
    {
        var match = Regex.Match(id, @"^\d{4}-(\d{2})$");

        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private IEnumerable<SpechtViolation> CheckReuse(SpechtModel model)
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
                yield return new SpechtViolation(
                    Id,
                    SpechtSeverity.Error,
                    item.RelativePath,
                    item.Frontmatter.LineOf("id"),
                    item.Id,
                    $"item id '{item.Id}' is already used by {seen[item.Id]} - ids are never reused")
                {
                    Expected = new JsonObject { ["fileName"] = $"{item.Id}-*.md", ["firstDeclaration"] = seen[item.Id] },
                };
            }
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private IEnumerable<SpechtViolation> CheckSequences(SpechtModel model)
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
                    yield return new SpechtViolation(
                        Id,
                        SpechtSeverity.Error,
                        numbers[index].Item.RelativePath,
                        numbers[index].Item.Frontmatter.LineOf("id"),
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
}
