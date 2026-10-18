using Microsoft.CodeAnalysis;

namespace Specht.Analysis.Documentation;

/// <summary>A binding finding located on the original compiler syntax.</summary>
public sealed record DocumentationFinding(string Id, Location Location, string Reference);
