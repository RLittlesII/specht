using System;
using Spectre.Console.Cli;

namespace Specht.Tool;

/// <summary>Resolves Spectre's commands and their dependencies from the built service provider.</summary>
/// <param name="provider">The one provider <see cref="TypeRegistrar.Build"/> builds.</param>
/// <implements epic="0001" feature="F7" claim="B-001" />
public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver, IDisposable
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F7" claim="B-001" />
    public object? Resolve(Type? type) => type is null ? null : provider.GetService(type);

    /// <inheritdoc />
    /// <implements epic="0001" feature="F7" claim="B-001" />
    public void Dispose() => (provider as IDisposable)?.Dispose();
}
