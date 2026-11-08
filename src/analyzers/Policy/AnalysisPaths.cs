using System;
using System.Linq;
using System.Text.RegularExpressions;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Policy;

/// <summary>Matches supplied root-relative paths with the checker's segment glob semantics.</summary>
internal sealed class AnalysisPaths
{
    public bool Matches(string path, string glob)
    {
        var parts = Normalize(glob).Split('/');
        var segments = parts.Select((segment, index) => segment == "**"
            ? index == parts.Length - 1 ? ".*" : "(?:[^/]+/)*"
            : Regex.Escape(segment).Replace(@"\*", "[^/]*") + "/");
        return new Regex(@"\A" + string.Concat(segments).TrimEnd('/') + @"\z", RegexOptions.CultureInvariant).IsMatch(Normalize(path));
    }

    public string Normalize(string path)
    {
        new BindingProvenanceGuard().Validate(path);
        var parts = new System.Collections.Generic.List<string>();
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
}
