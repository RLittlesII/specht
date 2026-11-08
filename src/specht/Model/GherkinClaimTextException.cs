using System;

namespace Specht.Model;

/// <summary>A claim file or tag form the Gherkin claim reader cannot read, naming the file, or no path for the manifest.</summary>
/// <implements epic="0201" feature="F7" claim="B-014" />
public sealed class GherkinClaimTextException : Exception
{
    /// <implements epic="0201" feature="F7" claim="B-014" />
    public GherkinClaimTextException(string? relativePath, string message)
        : base(message) => RelativePath = relativePath;

    public string? RelativePath { get; }
}
