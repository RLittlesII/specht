using System;

namespace Specht.Model;

/// <summary>A claim file or tag form the Gherkin claim reader cannot read, naming the file, or no path for the manifest.</summary>
/// <implements epic="0201" feature="F7" claim="B-014" />
public sealed class GherkinClaimTextException : Exception
{
    /// <implements epic="0201" feature="F7" claim="B-014" />
    public GherkinClaimTextException(string? relativePath, string message, int line = 0)
        : base(message)
    {
        RelativePath = relativePath;
        Line = line;
    }

    public string? RelativePath { get; }

    /// <summary>The one-based line of the text refused, or <c>0</c> for a refusal of the whole file (<c>0001-F16</c> B-014, B-015).</summary>
    public int Line { get; }
}
