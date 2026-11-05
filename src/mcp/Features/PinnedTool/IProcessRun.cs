using System.Threading;
using System.Threading.Tasks;

namespace Specht.Mcp.Features.PinnedTool;

/// <summary>
/// The one place the host starts a program (<c>0132-F3</c> § 7). It is a seam because a test decides the exit codes, a program that
/// does not start and output with no trailing newline without a process.
/// </summary>
public interface IProcessRun
{
    /// <summary>Runs <paramref name="request"/> to its end.</summary>
    /// <param name="request">The program, its arguments and where it runs.</param>
    /// <param name="cancellationToken">Ends the program when the session ends.</param>
    /// <returns>What it wrote and its exit code; <see langword="null"/> when the system could not start it.</returns>
    Task<ProcessResult?> RunAsync(ProcessRequest request, CancellationToken cancellationToken);
}
