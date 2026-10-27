using System.Text.Json.Nodes;

namespace Specht.Manifest;

/// <summary>A validated manifest and the invocation-owned JSON that preserves its declarations and selected defaults.</summary>
/// <param name="Structure">The check's parsed interpretation.</param>
/// <param name="Document">The declared JSON with the parser's omission defaults materialized.</param>
/// <implements epic="0001" feature="F9" claim="B-003" />
public sealed record SpecManifestDocument(SpecStructure Structure, JsonObject Document);
