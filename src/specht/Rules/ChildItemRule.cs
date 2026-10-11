using System.Text.RegularExpressions;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// SPEC040-SPEC044 - a Feature's declared children resolve to real files,
/// each item's id agrees with its file name and its parent, item ids are
/// globally unique, and the per-epic sequence is contiguous.
/// </summary>
/// <remarks>
/// Tasks are numbered per EPIC, not per Feature, which is why uniqueness and
/// contiguity are checked across the epic rather than within the directory
/// that happens to hold the file.
/// </remarks>
public sealed class ChildItemRule : ISpecRule
{
    /// <inheritdoc />
    public string Id => "SPEC040";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => ["SPEC040", "SPEC041", "SPEC043", "SPEC044"];

    /// <inheritdoc />
    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        var byDirectory = model.Items
            .GroupBy(static item => item.ParentDirectory, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.ToList(), StringComparer.Ordinal);

        foreach (var feature in model.Features)
        {
            var siblings = byDirectory.TryGetValue(feature.Location.Directory, out var items) ? items : [];

            foreach (var violation in CheckDeclared(feature, siblings, "children", "SPEC040", null))
            {
                yield return violation;
            }

            // A spike is referenced repository-wide, not just locally: three
            // Features cite 0001-16, which lives beside 0001-F1, and 0004-F1
            // cites a spike from epic 0003. children[] is the opposite - it is
            // ownership, so it must resolve beside the specification itself.
            foreach (var violation in CheckDeclared(feature, model.Items, "spikes", "SPEC041", "spike"))
            {
                yield return violation;
            }
        }

        foreach (var violation in CheckItems(model))
        {
            yield return violation;
        }

        foreach (var violation in CheckSequences(model))
        {
            yield return violation;
        }
    }

    private static IEnumerable<SpecViolation> CheckDeclared(
        FeatureSpec feature,
        IReadOnlyCollection<ChildItem> candidates,
        string key,
        string ruleId,
        string? requiredType)
    {
        foreach (var declared in feature.Strings(key))
        {
            var matches = candidates
                .Where(item => string.Equals(item.FileNameId, declared, StringComparison.Ordinal))
                .ToList();

            if (matches.Count == 0)
            {
                yield return new SpecViolation(
                    ruleId,
                    SpecSeverity.Error,
                    feature.RelativePath,
                    feature.Document.Frontmatter.LineOf(key),
                    declared,
                    $"frontmatter {key} names '{declared}', but no '{declared}-*.md' was discovered");

                continue;
            }

            if (matches.Count > 1)
            {
                yield return new SpecViolation(
                    ruleId,
                    SpecSeverity.Error,
                    feature.RelativePath,
                    feature.Document.Frontmatter.LineOf(key),
                    declared,
                    $"frontmatter {key} names '{declared}', which resolves to {matches.Count} files");

                continue;
            }

            if (requiredType is not null && !string.Equals(matches[0].Type, requiredType, StringComparison.Ordinal))
            {
                yield return new SpecViolation(
                    ruleId,
                    SpecSeverity.Error,
                    matches[0].RelativePath,
                    matches[0].Frontmatter.LineOf("type"),
                    declared,
                    $"'{declared}' is declared in frontmatter {key} but its type is '{matches[0].Type}', not '{requiredType}'");
            }
        }
    }

    private static IEnumerable<SpecViolation> CheckItems(SpecModel model)
    {
        var seen = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var item in model.Items)
        {
            if (item.Id is null)
            {
                continue;
            }

            if (!string.Equals(item.Id, item.FileNameId, StringComparison.Ordinal))
            {
                yield return new SpecViolation(
                    "SPEC043",
                    SpecSeverity.Error,
                    item.RelativePath,
                    item.Frontmatter.LineOf("id"),
                    item.Id,
                    $"frontmatter id '{item.Id}' does not match the file name prefix '{item.FileNameId}'");
            }

            if (!seen.TryAdd(item.Id, item.RelativePath))
            {
                yield return new SpecViolation(
                    "SPEC044",
                    SpecSeverity.Error,
                    item.RelativePath,
                    item.Frontmatter.LineOf("id"),
                    item.Id,
                    $"item id '{item.Id}' is already used by {seen[item.Id]} - ids are never reused");
            }

            var parent = model.Features.FirstOrDefault(feature =>
                string.Equals(feature.Location.Directory, item.ParentDirectory, StringComparison.Ordinal));

            if (parent is not null && item.Parent is not null
                                   && !string.Equals(item.Parent, parent.Identity, StringComparison.Ordinal))
            {
                yield return new SpecViolation(
                    "SPEC043",
                    SpecSeverity.Error,
                    item.RelativePath,
                    item.Frontmatter.LineOf("parent"),
                    item.Id,
                    $"frontmatter parent '{item.Parent}' does not match the Feature it sits in ('{parent.Identity}')");
            }
        }
    }

    private static IEnumerable<SpecViolation> CheckSequences(SpecModel model)
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
                    yield return new SpecViolation(
                        "SPEC044",
                        SpecSeverity.Error,
                        numbers[index].Item.RelativePath,
                        numbers[index].Item.Frontmatter.LineOf("id"),
                        numbers[index].Item.Id,
                        $"epic {group.Key}'s item sequence skips {expected:D2} - tasks are numbered per epic, contiguously from 01");

                    break;
                }
            }
        }
    }

    private static int Number(string id)
    {
        var match = Regex.Match(id, @"^\d{4}-(\d{2})$");

        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }
}
