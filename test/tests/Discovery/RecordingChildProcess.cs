using Specht.Discovery;

namespace Specht.Tests.Discovery;

/// <summary>
/// The production <see cref="ChildProcess"/>, wrapped so a test can read what each run gave back (<c>0001-F6</c> B-005,
/// B-010, B-011).
/// </summary>
internal sealed class RecordingChildProcess : IChildProcess
{
    public List<ChildProcessResult?> Results { get; } = [];

    public ChildProcessResult? Run(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
    {
        var result = new ChildProcess().Run(fileName, arguments, workingDirectory);
        Results.Add(result);
        return result;
    }
}
