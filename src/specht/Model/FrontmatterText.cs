using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Specht.Model;

/// <summary>The shared parsed YAML mapping and original one-based key positions.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
internal sealed record FrontmatterText(JsonObject? Node, int StartLine, IReadOnlyDictionary<string, int> KeyLines);
