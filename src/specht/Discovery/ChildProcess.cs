using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Specht.Discovery;

/// <summary>Runs a program as a child process of the tool (<c>0001-F6</c> B-005, B-006, decision 0002).</summary>
/// <implements epic="0001" feature="F6" claim="B-005" />
public sealed class ChildProcess : IChildProcess
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F6" claim="B-005" />
    /// <implements epic="0001" feature="F6" claim="B-006" />
    public ChildProcessResult? Run(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
    {
        var start = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = start };
        try
        {
            process.Start();
        }
        catch (Win32Exception)
        {
            return null;
        }

        process.StandardInput.Close();
        process.BeginErrorReadLine();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return new ChildProcessResult(process.ExitCode, output);
    }
}
