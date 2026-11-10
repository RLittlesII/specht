using Specht.Discovery;

namespace Specht.Tests.Discovery;

/// <summary>
/// The child process of a discovery that walks: it fails the test it is started in, so a construction over a tree with
/// no <c>.git</c> stays in walk mode (<c>0001-F6</c> B-006).
/// </summary>
internal sealed class UnexpectedChildProcess : IChildProcess
{
    public ChildProcessResult? Run(string fileName, IReadOnlyList<string> arguments, string workingDirectory) =>
        throw new InvalidOperationException($"Discovery started '{fileName}' where it walks.");
}
