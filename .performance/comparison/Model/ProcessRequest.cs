using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>One child process: a repository-relative working directory and its complete environment.</summary>
public sealed record ProcessRequest(
    string FileName,
    ImmutableArray<string> Arguments,
    string WorkingDirectory,
    ImmutableSortedDictionary<string, string> Environment);
