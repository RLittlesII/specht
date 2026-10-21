using System.Text.Json.Nodes;

namespace Specht.Tool.Features.Evidence;

/// <summary>A reconciliation report and its gate exit.</summary>
/// <implements epic="0201" feature="F5" claim="B-001" />
public sealed record EvidenceResult(JsonObject Report, int ExitCode);
