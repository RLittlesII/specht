namespace Specht.Discovery;

/// <summary>
/// Runs a program as a child process and returns its exit code and standard output: the git-listing seam
/// (<c>0001-F6</c> B-005, B-006, decision 0002; ADR-0001 stage E).
/// </summary>
/// <implements epic="0001" feature="F6" claim="B-005" />
public interface IChildProcess
{
    /// <summary>Runs <paramref name="fileName"/> with <paramref name="arguments"/> in <paramref name="workingDirectory"/> to its exit.</summary>
    /// <param name="fileName">The program to start.</param>
    /// <param name="arguments">The arguments, each passed as written.</param>
    /// <param name="workingDirectory">The directory the program runs in.</param>
    /// <returns>The exit code and standard output, or <see langword="null"/> when the program cannot be started.</returns>
    /// <implements epic="0001" feature="F6" claim="B-005" />
    /// <implements epic="0001" feature="F6" claim="B-006" />
    ChildProcessResult? Run(string fileName, IReadOnlyList<string> arguments, string workingDirectory);
}
