using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>A finished child process: its exit code and standard output lines.</summary>
public sealed record ProcessOutcome(
    int ExitCode,
    ImmutableArray<string> Output);
