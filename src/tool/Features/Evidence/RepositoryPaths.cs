using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Specht.Tool.Features.Evidence;

/// <summary>Normalises root-relative claim file set paths and globs and matches a path to a glob, reading them as the analyzer's AnalysisPaths does.</summary>
/// <implements epic="0201" feature="F5" claim="B-035" />
/// <implements epic="0201" feature="F5" claim="B-038" />
public sealed class RepositoryPaths
{
    /// <implements epic="0201" feature="F5" claim="B-038" />
    public bool Matches(string path, string glob)
    {
        var parts = Normalize(glob).Split('/');
        var segments = parts.Select((segment, index) => segment == "**"
            ? index == parts.Length - 1 ? ".*" : "(?:[^/]+/)*"
            : string.Join("[^/]*", segment.Split('*').Select(Regex.Escape)) + "/");
        return new Regex(@"\A" + string.Concat(segments).TrimEnd('/') + @"\z", RegexOptions.CultureInvariant).IsMatch(Normalize(path));
    }

    /// <implements epic="0201" feature="F5" claim="B-038" />
    public string Normalize(string path)
    {
        Validate(path);
        var parts = new List<string>();
        foreach (var segment in path.Replace('\\', '/').Split('/'))
        {
            if (segment == "..")
            {
                parts.RemoveAt(parts.Count - 1);
            }
            else if (segment.Length > 0 && segment != ".")
            {
                parts.Add(segment);
            }
        }

        return parts.Count > 0 ? string.Join("/", parts) : throw new ArgumentException("An input path must name a repository file or glob.");
    }

    /// <implements epic="0201" feature="F5" claim="B-038" />
    private void Validate(string path)
    {
        if (string.IsNullOrEmpty(path) || path[0] is '/' or '\\'
            || path.Length > 1 && path[1] == ':' && (path[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z'))
        {
            throw new ArgumentException(Error);
        }

        var depth = 0;
        foreach (var segment in path.Split('/', '\\'))
        {
            if (segment == "..")
            {
                depth--;
                if (depth < 0)
                {
                    throw new ArgumentException(Error);
                }
            }
            else if (segment.Length > 0 && segment != ".")
            {
                depth++;
            }
        }
    }

    private const string Error = "Binding provenance must be repository-relative and remain within the root.";
}
