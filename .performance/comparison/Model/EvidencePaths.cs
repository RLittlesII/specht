using System;
using System.Linq;

namespace Specht.Comparison;

/// <summary>Checks portable evidence references without consulting ambient paths.</summary>
public static class EvidencePaths
{
    public static bool IsRelative(string? path) => !string.IsNullOrWhiteSpace(path)
        && !path.StartsWith('/') && !path.StartsWith('~') && !path.Contains('\\') && !path.Contains(':')
        && path.Split('/').All(static part => part.Length > 0 && part is not "." and not "..")
        && !path.Any(static c => char.IsControl(c));
}
