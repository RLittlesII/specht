using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Specht.Build;

/// <summary>Checks that each shipped schema version is numbered for what changed since the one below it (0001-F7 B-040, B-066, B-088).</summary>
public sealed class SchemaVersionNumbering(IFileSystem fileSystem, string root)
{
    /// <summary>Compares each shipped version with the next higher one and returns every fault.</summary>
    public async Task<IReadOnlyList<SchemaVersionNumberingFinding>> VerifyAsync()
    {
        var versions = new SchemaVersionLock(fileSystem, root).ReadVersions().Order().ToArray();
        var findings = new List<SchemaVersionNumberingFinding>();
        foreach (var (lower, higher) in versions.Zip(versions.Skip(1)))
        {
            var lowerRules = RuleIds(lower);
            var higherRules = RuleIds(higher);
            var added = higherRules.Except(lowerRules, StringComparer.Ordinal).ToArray();
            var newRule = added.Length > 0;
            if (newRule && lower.Major >= 1 && higher.Major <= lower.Major)
            {
                findings.Add(new(lower.ToString(3), higher.ToString(3), SchemaVersionNumberingFault.NewRuleWithoutHigherMajor));
            }

            if (newRule
                && higher.Major < 1
                && higher.Minor <= lower.Minor
                && !await AllOptInAsync(lower.ToString(3), higher.ToString(3), added))
            {
                findings.Add(new(lower.ToString(3), higher.ToString(3), SchemaVersionNumberingFault.NewRuleWithoutHigherMinor));
            }

            if (lowerRules.SetEquals(higherRules)
                && (higher.Major != lower.Major || higher.Minor != lower.Minor)
                && await DifferOnlyInTemplatesOrRulePagesAsync(lower.ToString(3), higher.ToString(3)))
            {
                findings.Add(new(lower.ToString(3), higher.ToString(3), SchemaVersionNumberingFault.TemplateOrRulePageChangeNotAPatch));
            }
        }

        return findings;
    }

    private static bool Defines(JsonNode? manifest, string key) =>
        key.Split('.').Aggregate(
            manifest,
            static (node, segment) => node is JsonObject json && json.TryGetPropertyValue(segment, out var value) ? value : null) is not null;

    private static bool StatesVersion(string tree, string line, string version) => tree switch
    {
        "src/tool/schema" => line.Contains("\"$id\"", StringComparison.Ordinal) && line.Contains($"/v{version}/", StringComparison.Ordinal),
        "src/tool/manifest" => line.Contains("\"schemaVersion\"", StringComparison.Ordinal) && line.Contains($"\"{version}\"", StringComparison.Ordinal),
        RulesTree => line.StartsWith("| Schema version ", StringComparison.Ordinal) && line.Contains($" {version} ", StringComparison.Ordinal),
        _ => false,
    };

    private HashSet<string> RuleIds(Version version)
    {
        var folder = fileSystem.Path.Combine(root, RulesTree, $"v{version.ToString(3)}");
        return fileSystem.Directory.Exists(folder)
            ? fileSystem.Directory.GetFiles(folder, "*.md").Select(page => fileSystem.Path.GetFileNameWithoutExtension(page)).ToHashSet(StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
    }

    private async Task<bool> AllOptInAsync(string lower, string higher, string[] added)
    {
        var lowerManifest = await ReadManifestAsync(lower);
        var higherManifest = await ReadManifestAsync(higher);
        foreach (var id in added)
        {
            var key = await OptInKeyAsync(fileSystem.Path.Combine(root, RulesTree, $"v{higher}", $"{id}.md"));
            if (key is null || Defines(lowerManifest, key) || !Defines(higherManifest, key))
            {
                return false;
            }
        }

        return true;
    }

    private async Task<string?> OptInKeyAsync(string page)
    {
        var lines = (await fileSystem.File.ReadAllTextAsync(page)).Split('\n').Select(static line => line.TrimEnd('\r')).ToArray();
        if (lines.Length == 0 || lines[0] != "---")
        {
            return null;
        }

        return lines.Skip(1)
            .TakeWhile(static line => line != "---")
            .Where(static line => line.StartsWith(OptInField, StringComparison.Ordinal))
            .Select(static line => line[OptInField.Length..].Trim().Trim('"', '\''))
            .FirstOrDefault(static key => key.Length > 0);
    }

    private async Task<JsonNode?> ReadManifestAsync(string version)
    {
        var path = fileSystem.Path.Combine(root, "src/tool/manifest", $"v{version}", "manifest.json");
        return fileSystem.File.Exists(path) ? JsonNode.Parse(await fileSystem.File.ReadAllTextAsync(path)) : null;
    }

    private async Task<bool> DifferOnlyInTemplatesOrRulePagesAsync(string lower, string higher)
    {
        var lowerFiles = await ReadStatementFreeAsync(lower);
        var higherFiles = await ReadStatementFreeAsync(higher);
        var text = lowerFiles.Keys.Union(higherFiles.Keys, StringComparer.Ordinal)
            .ToLookup(path => path.StartsWith(TemplatesTree + "/", StringComparison.Ordinal) || path.StartsWith(RulesTree + "/", StringComparison.Ordinal));
        bool Equal(string path) =>
            lowerFiles.TryGetValue(path, out var left)
            && higherFiles.TryGetValue(path, out var right)
            && string.Equals(left, right, StringComparison.Ordinal);
        return text[false].All(Equal) && !text[true].All(Equal);
    }

    private async Task<Dictionary<string, string>> ReadStatementFreeAsync(string version)
    {
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var tree in SchemaVersionLock.ShippingTrees)
        {
            var folder = fileSystem.Path.Combine(root, tree, $"v{version}");
            if (!fileSystem.Directory.Exists(folder))
            {
                continue;
            }

            foreach (var path in fileSystem.Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
            {
                var lines = (await fileSystem.File.ReadAllTextAsync(path))
                    .Split('\n')
                    .Where(line => !StatesVersion(tree, line, version));
                files.Add($"{tree}/{fileSystem.Path.GetRelativePath(folder, path).Replace('\\', '/')}", string.Join('\n', lines));
            }
        }

        return files;
    }

    private const string TemplatesTree = "src/tool/templates";

    private const string RulesTree = "docs/rules";

    private const string OptInField = "opt_in:";
}
