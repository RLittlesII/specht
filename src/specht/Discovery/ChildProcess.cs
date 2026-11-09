namespace Specht.Discovery;

/// <summary>Runs a program as a child process of the tool (<c>0001-F6</c> B-005, B-006, decision 0002).</summary>
/// <implements epic="0001" feature="F6" claim="B-005" />
public sealed class ChildProcess : IChildProcess
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F6" claim="B-005" />
    /// <implements epic="0001" feature="F6" claim="B-006" />
    public ChildProcessResult? Run(string fileName, IReadOnlyList<string> arguments, string workingDirectory) =>
        throw new NotImplementedException();
}
