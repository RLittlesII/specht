namespace Specht.Analysis.Tests.Bindings;

internal sealed class BindingTextDirectory : IDisposable
{
    public BindingTextDirectory() => _directory = Directory.CreateTempSubdirectory("specht-text-");

    public string FullName => _directory.FullName;

    public void Dispose() => _directory.Delete(true);

    private readonly DirectoryInfo _directory;
}
