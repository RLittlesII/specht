namespace Specht.Discovery;

/// <summary>What a child process that started gave back (<c>0001-F6</c> B-005, decision 0002).</summary>
/// <param name="ExitCode">The process's exit code.</param>
/// <param name="StandardOutput">The process's standard output, decoded as UTF-8.</param>
/// <implements epic="0001" feature="F6" claim="B-005" />
public sealed record ChildProcessResult(int ExitCode, string StandardOutput);
