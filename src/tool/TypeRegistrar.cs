using System;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Specht.Tool;

/// <summary>Spectre's bridge to <see cref="IServiceCollection"/> (<c>dotnet-tool</c> § Vertical Slice).</summary>
/// <param name="services">The registrations <c>Program.cs</c> lists by hand.</param>
/// <implements epic="0001" feature="F7" claim="B-001" />
public sealed class TypeRegistrar(IServiceCollection services) : ITypeRegistrar
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F7" claim="B-001" />
    public ITypeResolver Build() => new TypeResolver(services.BuildServiceProvider());

    /// <inheritdoc />
    /// <implements epic="0001" feature="F7" claim="B-001" />
    public void Register(Type service, Type implementation) => services.AddSingleton(service, implementation);

    /// <inheritdoc />
    /// <implements epic="0001" feature="F7" claim="B-001" />
    public void RegisterInstance(Type service, object implementation) => services.AddSingleton(service, implementation);

    /// <inheritdoc />
    /// <implements epic="0001" feature="F7" claim="B-001" />
    public void RegisterLazy(Type service, Func<object> factory) => services.AddSingleton(service, _ => factory());
}
