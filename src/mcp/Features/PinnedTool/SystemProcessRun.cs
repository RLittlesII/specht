using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
public sealed class SystemProcessRun : IProcessRun
{
    /// <inheritdoc />
    /// <implements epic="0132" feature="F3" claim="B-017" />
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

        var descendants = Descendants(process.Id);
        for (var index = descendants.Count - 1; index >= 0; index--)
        {
            try
            {
                using var descendant = Process.GetProcessById(descendants[index]);
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

    private static List<int> Descendants(int root)
    {
        Func<int, IEnumerable<int>> childrenOf = OperatingSystem.IsMacOS() ? ChildrenOnMacOS : ChildrenOnLinux();
        var descendants = new List<int>();
        var parents = new Queue<int>([root]);
        while (parents.TryDequeue(out var parent))
        {
            foreach (var child in childrenOf(parent))
            {
                descendants.Add(child);
                parents.Enqueue(child);
            }
        }

        return descendants;
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

    private static Func<int, IEnumerable<int>> ChildrenOnLinux()
    {
        var children = new Dictionary<int, List<int>>();
        foreach (var directory in Directory.EnumerateDirectories("/proc"))
        {
            if (!int.TryParse(Path.GetFileName(directory), NumberStyles.None, CultureInfo.InvariantCulture, out var pid))
            {
                continue;
            }

            string stat;
            try
            {
                stat = File.ReadAllText(Path.Combine(directory, "stat"));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                continue;
            }

            var fields = stat[(stat.LastIndexOf(')') + 2)..].Split(' ');
            if (fields.Length > 1 && int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out var parent))
            {
                if (!children.TryGetValue(parent, out var siblings))
                {
                    siblings = [];
                    children[parent] = siblings;
                }

                siblings.Add(pid);
            }
        }

        return parent => children.TryGetValue(parent, out var siblings) ? siblings : [];
    }

    [DllImport("libproc", EntryPoint = "proc_listchildpids")]
    private static extern int ListChildProcessIds(int parent, int[] buffer, int bufferSize);
}
