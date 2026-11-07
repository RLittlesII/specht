using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace Specht.IdGeneration;

/// <summary>Where the collision guard looks for a type's existing ids: repository-wide types over the root, per-Feature types in one Feature.</summary>
/// <param name="fileSystem">The file system.</param>
public sealed class HeldIds(IFileSystem fileSystem)
{
    /// <summary>Whether a type's ids are numbered per Feature, so the guard needs the Feature's folder.</summary>
    /// <param name="type">The id type.</param>
    /// <returns>True for claims, constraints, open questions, decisions and scenarios.</returns>
    public static bool PerFeature(string type) => type is "claim" or "constraint" or "openQuestion" or "decision" or "scenario";

    /// <summary>Reads the ids the tree holds for a type, compared ignoring case.</summary>
    /// <param name="root">The root.</param>
    /// <param name="type">The id type.</param>
    /// <param name="feature">The Feature's folder, relative to the root, for a per-Feature type.</param>
    /// <returns>The held ids.</returns>
    public IReadOnlySet<string> Read(string root, string type, string? feature)
    {
        var held = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var scope = feature is null ? root : fileSystem.Path.Combine(root, feature);
        var readme = fileSystem.Path.Combine(scope, ".spec", "README.md");

        switch (type)
        {
            case "adr":
                Names(root, "adr", @"^([^-]+)-.*\.md$", held);
                break;
            case "lesson":
                Names(root, "lessons", @"^([^-]+)-.*\.md$", held);
                break;
            case "task":
                Names(root, ".issue", @"^([^-]+)-.*\.(yml|md)$", held);
                break;
            case "decision":
                Names(fileSystem.Path.Combine(scope, ".spec"), "decisions", @"^([^-]+)-.*\.md$", held);
                break;
            case "claim":
                Matches(readme, @"^\| (B-[^ |]+) ", held);
                break;
            case "constraint":
                Matches(readme, @"^\| (C-[^ |]+) ", held);
                break;
            case "openQuestion":
                Matches(readme, @"^\| (OQ-[^ |]+) ", held);
                break;
            case "scenario":
                foreach (var file in Files(fileSystem.Path.Combine(scope, ".spec")).Where(static f => f.EndsWith(".feature", StringComparison.Ordinal)))
                {
                    Matches(file, @"@(S-[^\s]+)", held);
                }

                break;
            case "feature":
                foreach (var file in Files(root).Where(static f => f.EndsWith("/.spec/README.md", StringComparison.Ordinal)))
                {
                    Matches(file, @"^id: ""?([^""\s]+)""?$", held);
                }

                break;
            case "epic":
                foreach (var file in Files(root).Where(static f => f.EndsWith("/epic.md", StringComparison.Ordinal)))
                {
                    Matches(file, @"^id: ""?([^""\s]+)""?$", held);
                }

                Names(root, ".issue", @"^([^-]+)-.*\.yml$", held);
                break;
        }

        return held;
    }

    private void Names(string under, string folder, string pattern, HashSet<string> held)
    {
        foreach (var file in Files(under).Where(f => fileSystem.Path.GetFileName(fileSystem.Path.GetDirectoryName(f)) == folder))
        {
            var match = Regex.Match(fileSystem.Path.GetFileName(file), pattern);
            if (match.Success)
            {
                held.Add(match.Groups[1].Value);
            }
        }
    }

    private void Matches(string file, string pattern, HashSet<string> held)
    {
        if (!fileSystem.File.Exists(file))
        {
            return;
        }

        foreach (Match match in Regex.Matches(fileSystem.File.ReadAllText(file), pattern, RegexOptions.Multiline))
        {
            held.Add(match.Groups[1].Value);
        }
    }

    private IEnumerable<string> Files(string directory)
    {
        if (!fileSystem.Directory.Exists(directory))
        {
            yield break;
        }

        foreach (var file in fileSystem.Directory.EnumerateFiles(directory))
        {
            yield return file.Replace('\\', '/');
        }

        foreach (var child in fileSystem.Directory.EnumerateDirectories(directory))
        {
            if (!Skipped.Contains(fileSystem.Path.GetFileName(child)))
            {
                foreach (var file in Files(child))
                {
                    yield return file;
                }
            }
        }
    }

    private static readonly HashSet<string> Skipped = new(StringComparer.Ordinal) { ".git", ".artifacts", ".claude", "bin", "obj", "node_modules" };
}
