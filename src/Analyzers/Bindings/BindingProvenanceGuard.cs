using System;

namespace Specht.Analysis.Bindings;

/// <summary>Rejects rooted, drive-qualified or escaping provenance under either path separator.</summary>
internal sealed class BindingProvenanceGuard
{
    public void Validate(string path)
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
