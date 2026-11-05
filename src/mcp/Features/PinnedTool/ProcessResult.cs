namespace Specht.Mcp.Features.PinnedTool;

/// <summary>What a program wrote and how it ended.</summary>
/// <param name="ExitCode">The exit code.</param>
/// <param name="StandardOutput">Everything it wrote on standard output, as written.</param>
/// <param name="StandardError">Everything it wrote on standard error.</param>
public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
