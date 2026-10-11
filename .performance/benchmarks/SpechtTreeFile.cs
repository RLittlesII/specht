using System;

namespace Specht.Benchmarks;

/// <summary>One file of a <see cref="SpechtTree"/>.</summary>
/// <param name="RelativePath">The file's path under the tree's root, its segments separated by <c>/</c>.</param>
/// <param name="Content">The file's bytes.</param>
public sealed record SpechtTreeFile(string RelativePath, ReadOnlyMemory<byte> Content);
