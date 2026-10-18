using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

internal partial class SpechtBuild
{
    private async Task<int> RunSelfCheckCommandAsync(ProcessStartInfo start)
    {
        Log.Information("dotnet {Arguments}", string.Join(' ', start.ArgumentList));
        using var process = Process.Start(start) ?? throw new InvalidOperationException("The self-check process did not start.");
        process.StandardInput.Close();
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var output = (await stdout).TrimEnd('\r', '\n').Split('\n')
            .Where(static line => line.Length > 0)
            .Select(static line => line.TrimEnd('\r')).ToArray();
        var checking = start.ArgumentList[0] == "tool" && start.ArgumentList[1] == "run";
        foreach (var line in output)
        {
            if (checking)
            {
                Log.Debug("{Line}", line);
            }
            else
            {
                Log.Information("{Line}", line);
            }
        }

        foreach (var line in (await stderr).Split('\n').Where(static line => line.Length > 0))
        {
            Log.Warning("{Line}", line.TrimEnd('\r'));
        }

        if (checking)
        {
            AnnotateChangedFiles(output);
        }

        return process.ExitCode;
    }
}
