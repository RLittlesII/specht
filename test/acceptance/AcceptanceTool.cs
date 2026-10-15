using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Specht.Tool.Features.Check;

namespace Specht.Acceptance;

/// <summary>
/// The built tool, launched as a shell launches it, so a scenario sees the process's stdout, stderr and exit code; Spectre's
/// command tester captures stdout alone (0001-F2 C-7). Any other process a scenario runs the same way, such as the build's
/// entry script and the pre-commit hook a check is called through (B-010, B-011). It also reads the version an assembly
/// states from the file's own metadata, the launched tool's (0001-F2 B-040) or one a package carries (B-030).
/// </summary>
internal static class AcceptanceTool
{
    /// <summary>Runs the tool in <paramref name="workingDirectory"/> with <paramref name="args"/> and waits for it to exit.</summary>
    /// <param name="workingDirectory">The directory the process starts in.</param>
    /// <param name="args">The arguments, each passed as one.</param>
    /// <returns>What the process wrote to stdout and to stderr, and its exit code.</returns>
    public static (string Stdout, string Stderr, int ExitCode) Launch(string workingDirectory, params string[] args) =>
        Launch(workingDirectory, new Dictionary<string, string>(StringComparer.Ordinal), args);

    /// <summary>
    /// Runs the tool in <paramref name="workingDirectory"/> with <paramref name="args"/>, its process carrying
    /// <paramref name="environment"/> over what this process hands it, and waits for it to exit.
    /// </summary>
    /// <param name="workingDirectory">The directory the process starts in.</param>
    /// <param name="environment">The environment variables set for the process, by name.</param>
    /// <param name="args">The arguments, each passed as one.</param>
    /// <returns>What the process wrote to stdout and to stderr, and its exit code.</returns>
    public static (string Stdout, string Stderr, int ExitCode) Launch(
        string workingDirectory,
        IReadOnlyDictionary<string, string> environment,
        params string[] args)
    {
        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = workingDirectory };
        foreach (var (name, value) in environment)
        {
            start.Environment[name] = value;
        }

        start.ArgumentList.Add(typeof(CheckCommand).Assembly.Location);
        foreach (var arg in args)
        {
            start.ArgumentList.Add(arg);
        }

        return Run(start);
    }

    /// <summary>
    /// Runs the process <paramref name="start"/> describes and waits for it to exit. A process whose standard input is
    /// redirected reads none: the input is closed as soon as it starts.
    /// </summary>
    /// <param name="start">The process to run; its stdout and stderr are redirected here.</param>
    /// <returns>What the process wrote to stdout and to stderr, and its exit code.</returns>
    public static (string Stdout, string Stderr, int ExitCode) Run(ProcessStartInfo start)
    {
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;

        using var process = Process.Start(start) ?? throw new InvalidOperationException($"{start.FileName} did not start.");
        if (start.RedirectStandardInput)
        {
            process.StandardInput.Close();
        }

        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEnd();
        var output = stdout.GetAwaiter().GetResult();
        process.WaitForExit();

        return (output, stderr, process.ExitCode);
    }

    /// <summary>Reads the informational version the launched tool's assembly states.</summary>
    /// <returns>The version, or <see langword="null"/> when the assembly states none.</returns>
    public static string? InformationalVersion() => InformationalVersion(File.ReadAllBytes(typeof(CheckCommand).Assembly.Location));

    /// <summary>Reads the informational version an assembly states, from its metadata and without loading it.</summary>
    /// <param name="assembly">The assembly file's bytes.</param>
    /// <returns>The version, or <see langword="null"/> when the assembly states none.</returns>
    public static string? InformationalVersion(byte[] assembly)
    {
        using var portable = new PEReader(new MemoryStream(assembly));
        var reader = portable.GetMetadataReader();

        foreach (var handle in reader.GetAssemblyDefinition().GetCustomAttributes())
        {
            var attribute = reader.GetCustomAttribute(handle);
            if (attribute.Constructor.Kind == HandleKind.MemberReference
                && reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent is { Kind: HandleKind.TypeReference } type
                && reader.GetString(reader.GetTypeReference((TypeReferenceHandle)type).Name) == nameof(AssemblyInformationalVersionAttribute))
            {
                var value = reader.GetBlobReader(attribute.Value);
                value.ReadUInt16();

                return value.ReadSerializedString();
            }
        }

        return null;
    }
}
