using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Specht.Model;

/// <summary>The shared parsed YAML mapping and original one-based key positions.</summary>
internal sealed record FrontmatterText(JsonObject? Node, int StartLine, IReadOnlyDictionary<string, int> KeyLines);
