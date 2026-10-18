using System.Reflection;
using System.Runtime.Loader;

namespace Specht.Analysis.Tests.Bindings;

internal sealed class BindingComponentLoadContext : AssemblyLoadContext
{
    public BindingComponentLoadContext(IReadOnlyDictionary<string, string> paths) : base(isCollectible: true) => _paths = paths;

    public bool IsForbidden(string name) =>
        string.Equals(name, "specht", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("Reqnroll", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("xunit", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("Microsoft.Playwright", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("Spectre", StringComparison.OrdinalIgnoreCase)
        || name.Contains("Workspaces", StringComparison.OrdinalIgnoreCase);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var name = assemblyName.Name!;
        if (IsForbidden(name))
        {
            throw new FileNotFoundException("A forbidden compiler dependency was requested: " + name);
        }

        if (_paths.TryGetValue(name, out var path))
        {
            return LoadFromAssemblyPath(path);
        }

        if (name is "netstandard" or "mscorlib" || name.StartsWith("System", StringComparison.Ordinal))
        {
            return Default.LoadFromAssemblyName(assemblyName);
        }

        throw new FileNotFoundException("An undeclared compiler dependency was requested: " + name);
    }

    private readonly IReadOnlyDictionary<string, string> _paths;
}
