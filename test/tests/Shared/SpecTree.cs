using System.Text;

namespace Specht.Tests.Shared;

/// <summary>
/// Builds a synthetic specification tree under a temporary root, so a rule can
/// be exercised without reading this repository's real specifications.
/// </summary>
/// <remarks>
/// The schemas are the exception: they are copied from the build output, which
/// links the real <c>.spec/schema/</c>. They are the contract under
/// enforcement, so a forked copy would test the wrong thing.
/// </remarks>
public sealed class SpecTree : IDisposable
{
    /// <summary>A tree whose root is a fresh temporary directory, or <paramref name="nesting"/> below one.</summary>
    /// <param name="nesting">Folders between the temporary directory and the root, outermost first; none by default.</param>
    public SpecTree(IReadOnlyList<string>? nesting = null)
    {
        _temporary = Path.Combine(Path.GetTempPath(), "hooked-specgov-" + Guid.NewGuid().ToString("N"));
        Root = Path.Combine([_temporary, .. nesting ?? []]);

        var schema = Path.Combine(Root, ".spec", "schema");

        Directory.CreateDirectory(schema);

        foreach (var file in Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, ".spec", "schema"), "*.json"))
        {
            File.Copy(file, Path.Combine(schema, Path.GetFileName(file)));
        }
    }

    /// <summary>The canonical section bodies a valid specification carries.</summary>
    public static IReadOnlyList<string> Sections { get; } =
    [
        "## 1. Business Goal\n\nRemoves a failure state.\n",
        "## 2. User Needs\n\n| # | Persona | Need | Pain Point Today |\n| - | ------- | ---- | ---------------- |\n| 1 | Operator | Wants it | Has none |\n",
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n| B-001 | It does the thing. | brd | Active |\n",
        "## 4. Constraints\n\n| ID | Constraint | Rules Out |\n| -- | ---------- | --------- |\n| C-1 | Must be fast. | Slow things |\n",
        "## 5. Out of Scope\n\n| # | Item | Exclusion Reason |\n| - | ---- | ---------------- |\n| 1 | Other things | Owned elsewhere |\n",
        "## 6. Concern Separation\n\nNone.\n",
        "## 7. Technical Design\n\nNone.\n",
        "## 8. Testing Strategy\n\nNone.\n",
        "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n"
            + "| -------- | -------- | ---- | ------ |\n"
            + "| B-001 | It does the thing | Missing | Missing |\n",
        "## 10. Lessons / Spec Deltas\n\nNone.\n",
        "## 11. Open Questions\n\nNone.\n",
        "## 12. Sign-off\n\n| Section | Status | Reviewer | Note |\n| ------- | ------ | -------- | ---- |\n| 1-5 | \U0001F7E1 | spec-reviewer | Draft |\n",
        "## Tasks\n\nNone.\n",
        "## Scoring\n\n| Field | Value | Basis |\n| ----- | ----- | ----- |\n| value | 4 | guess |\n",
    ];

    /// <summary>The temporary repository root.</summary>
    public string Root { get; }

    /// <summary>The canonical sections with one replaced by title, or appended when absent.</summary>
    public static IReadOnlyList<string> SectionsWith(string title, string replacement)
    {
        var sections = Sections.ToList();
        var index = sections.FindIndex(section => section.StartsWith($"## {title}\n", StringComparison.Ordinal));

        if (index < 0)
        {
            sections.Add(replacement);
        }
        else
        {
            sections[index] = replacement;
        }

        return sections;
    }

    /// <summary>Overwrites the Gherkin file beside the Feature at <paramref name="specPath"/>.</summary>
    public void WriteFeatureFile(string specPath, string content) =>
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(specPath)!, "feature.feature"), content);

    /// <summary>Runs every rule against the tree.</summary>
    public SpechtReport Run() => (SpechtReport)SpechtRunner.Run(Root).Value!;

    /// <inheritdoc />
    public void Dispose()
    {
        if (Directory.Exists(_temporary))
        {
            Directory.Delete(_temporary, recursive: true);
        }
    }

    /// <summary>Writes a Feature specification, with optional frontmatter and section overrides.</summary>
    public string WriteFeature(
        string epic,
        string id,
        IReadOnlyDictionary<string, string>? frontmatter = null,
        IReadOnlyList<string>? sections = null,
        string? featureFile = "feature")
    {
        var directory = Path.Combine(Root, "epics", $"{epic}-epic", $"{id}-feature");

        Directory.CreateDirectory(directory);

        var body = new StringBuilder();

        body.Append(Frontmatter(epic, id, frontmatter));
        body.Append($"# Specification: {id}\n\n");
        body.Append(string.Join('\n', sections ?? Sections));

        var path = Path.Combine(directory, "spec.md");

        File.WriteAllText(path, body.ToString());

        if (featureFile is not null)
        {
            File.WriteAllText(
                Path.Combine(directory, featureFile + ".feature"),
                "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n");
        }

        return path;
    }

    /// <summary>Writes a co-located Feature specification under <paramref name="area"/>, with optional frontmatter and section overrides.</summary>
    public string WriteCoLocatedFeature(
        string area,
        string epic,
        string id,
        IReadOnlyDictionary<string, string>? frontmatter = null,
        IReadOnlyList<string>? sections = null)
    {
        var directory = Path.Combine(Root, area.Replace('/', Path.DirectorySeparatorChar), ".spec");

        Directory.CreateDirectory(directory);

        var body = new StringBuilder();

        body.Append(Frontmatter(epic, id, frontmatter));
        body.Append($"# Specification: {id}\n\n");
        body.Append(string.Join('\n', sections ?? Sections));

        var path = Path.Combine(directory, "README.md");

        File.WriteAllText(path, body.ToString());
        File.WriteAllText(
            Path.Combine(directory, "feature.feature"),
            "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n");

        return path;
    }

    /// <summary>
    /// Writes a Feature specification at <paramref name="relativePath"/> under the root, a place no method above names,
    /// with no companion beside it.
    /// </summary>
    public string WriteSpecification(
        string relativePath,
        string epic,
        string id,
        IReadOnlyDictionary<string, string>? frontmatter = null,
        IReadOnlyList<string>? sections = null) =>
        WriteRaw(relativePath, Frontmatter(epic, id, frontmatter) + $"# Specification: {id}\n\n" + string.Join('\n', sections ?? Sections));

    /// <summary>Writes a child item beside the Feature at <paramref name="specPath"/>.</summary>
    public void WriteItem(
        string specPath,
        string id,
        string parent,
        string type = "task",
        string? idOverride = null,
        string slug = "item",
        IReadOnlyDictionary<string, string>? frontmatter = null)
    {
        var directory = Path.GetDirectoryName(specPath)!;
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["id"] = $"\"{idOverride ?? id}\"",
            ["parent"] = $"\"{parent}\"",
            ["type"] = type,
            ["status"] = "ready",
            ["priority"] = "med",
            ["created"] = "\"2026-10-07\"",
            ["updated"] = "\"2026-10-07\"",
            ["github_issue"] = "null",
        };

        File.WriteAllText(Path.Combine(directory, $"{id}-{slug}.md"), Render(values, frontmatter) + "# Item\n");
    }

    /// <summary>Writes an <c>epic.md</c> for <paramref name="epic"/>, with optional frontmatter overrides.</summary>
    public void WriteEpic(string epic, IReadOnlyDictionary<string, string>? frontmatter = null)
    {
        var directory = Path.Combine(Root, "epics", $"{epic}-epic");

        Directory.CreateDirectory(directory);

        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["id"] = $"\"{epic}\"",
            ["type"] = "epic",
            ["status"] = "ready",
            ["priority"] = "med",
            ["milestone"] = "null",
            ["children"] = "[]",
            ["created"] = "\"2026-10-07\"",
            ["updated"] = "\"2026-10-07\"",
            ["github_issue"] = "null",
        };

        File.WriteAllText(Path.Combine(directory, "epic.md"), Render(values, frontmatter) + "# Epic\n");
    }

    /// <summary>Writes an arbitrary file at <paramref name="relativePath"/> under the root.</summary>
    public string WriteRaw(string relativePath, string content)
    {
        var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);

        return path;
    }

    private static string Render(Dictionary<string, string> values, IReadOnlyDictionary<string, string>? overrides)
    {
        if (overrides is not null)
        {
            foreach (var (key, value) in overrides)
            {
                values[key] = value;
            }
        }

        var text = new StringBuilder("---\n");

        foreach (var (key, value) in values)
        {
            text.Append($"{key}: {value}\n");
        }

        return text.Append("---\n\n").ToString();
    }

    private static string Frontmatter(string epic, string id, IReadOnlyDictionary<string, string>? overrides)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["id"] = $"\"{id}\"",
            ["epic"] = $"\"{epic}\"",
            ["type"] = "feature",
            ["spec_status"] = "draft",
            ["status"] = "ready",
            ["priority"] = "med",
            ["value"] = "4",
            ["risk"] = "3",
            ["rank"] = "50",
            ["scored_by"] = "\"groomed\"",
            ["scored_on"] = "\"2026-10-07\"",
            ["domain"] = "\"Testing\"",
            ["author"] = "\"spec-author\"",
            ["milestone"] = "null",
            ["children"] = "[]",
            ["depends_on"] = "[]",
            ["blocks"] = "[]",
            ["spikes"] = "[]",
            ["created"] = "\"2026-10-07\"",
            ["updated"] = "\"2026-10-07\"",
            ["github_issue"] = "null",
            ["synced_at"] = "null",
        };

        if (overrides is not null)
        {
            foreach (var (key, value) in overrides)
            {
                values[key] = value;
            }
        }

        var text = new StringBuilder("---\n");

        foreach (var (key, value) in values)
        {
            text.Append($"{key}: {value}\n");
        }

        return text.Append("---\n\n").ToString();
    }

    private readonly string _temporary;
}
