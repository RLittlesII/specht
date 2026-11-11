namespace Specht.Build.Tests;

internal static class BuildHost
{
    public static string Location()
    {
        var output = new DirectoryInfo(AppContext.BaseDirectory);
        var configuration = output.Parent?.Name ?? string.Empty;
        for (var directory = output; directory is not null; directory = directory.Parent)
        {
            var host = Path.Combine(directory.FullName, ".build", "bin", configuration, ".build.dll");
            if (File.Exists(host))
            {
                return host;
            }
        }

        throw new FileNotFoundException("Could not locate the build host in its own output directory.");
    }
}
