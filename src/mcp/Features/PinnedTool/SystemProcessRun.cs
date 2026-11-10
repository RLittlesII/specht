using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Specht.Mcp.Features.PinnedTool;

/// <summary>
/// Runs a program as a process of the system. Its standard input is closed at once, so the program never reads the session's, and
/// what it writes is read as UTF-8 without a byte order mark and handed back unchanged.
/// </summary>
/// <implements epic="0132" feature="F3" claim="B-017" />
/// <implements epic="0132" feature="F3" claim="B-040" />
/// <implements epic="0132" feature="F3" claim="B-041" />
public sealed class SystemProcessRun : IProcessRun
{
    /// <inheritdoc />
    /// <implements epic="0132" feature="F3" claim="B-017" />
    /// <implements epic="0132" feature="F3" claim="B-040" />
    public async Task<ProcessResult?> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        var start = new ProcessStartInfo(request.FileName)
        {
            WorkingDirectory = request.WorkingDirectory,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = utf8,
            StandardErrorEncoding = utf8,
        };
        foreach (var argument in request.Arguments)
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
        var output = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = process.StandardError.ReadToEndAsync(cancellationToken);
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Kill(process);
            await Task.WhenAll(output, error).ContinueWith(static _ => { }, TaskScheduler.Default);
            throw;
        }

        return new ProcessResult(process.ExitCode, await output, await error);
    }

    private static void Kill(Process process)
    {
        if (!OperatingSystem.IsMacOS() && !OperatingSystem.IsLinux())
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }

            return;
        }

        foreach (var pid in Descendants(process.Id))
        {
            try
            {
                using var descendant = Process.GetProcessById(pid);
                descendant.Kill();
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or Win32Exception)
            {
            }
        }

        try
        {
            process.Kill();
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static IReadOnlyList<int> Descendants(int root)
    {
        try
        {
            var tree = OperatingSystem.IsMacOS() ? new ProcessTree(ChildrenOnMacOS) : new ProcessTree(new ProcScan().ChildrenOf);
            return tree.KillOrder(root);
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static int[] ChildrenOnMacOS(int parent)
    {
        var buffer = new int[64];
        while (true)
        {
            var count = ListChildProcessIds(parent, buffer, buffer.Length * sizeof(int));
            if (count < buffer.Length)
            {
                return count <= 0 ? [] : buffer[..count];
            }

            buffer = new int[buffer.Length * 2];
        }
    }

    [DllImport("libproc", EntryPoint = "proc_listchildpids")]
    private static extern int ListChildProcessIds(int parent, int[] buffer, int bufferSize);
}
