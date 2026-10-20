using System.Text.Json.Nodes;

namespace Specht.Tool.Features.Evidence;

/// <summary>A reconciliation report and its gate exit.</summary>
public sealed record EvidenceResult(JsonObject Report, int ExitCode);
