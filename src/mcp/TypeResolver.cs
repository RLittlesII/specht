using System;
using Spectre.Console.Cli;

namespace Specht.Mcp;

/// <summary>Resolves Spectre's command and its dependencies from the built service provider.</summary>
/// <param name="provider">The one provider <see cref="TypeRegistrar.Build"/> builds.</param>
public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver, IDisposable
{
    /// <inheritdoc />
    public object? Resolve(Type? type) => type is null ? null : provider.GetService(type);

    /// <inheritdoc />
    public void Dispose() => (provider as IDisposable)?.Dispose();
}
