using System;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Specht.Tool;

/// <summary>Spectre's bridge to <see cref="IServiceCollection"/> (<c>dotnet-tool</c> § Vertical Slice).</summary>
/// <param name="services">The registrations <c>Program.cs</c> lists by hand.</param>
public sealed class TypeRegistrar(IServiceCollection services) : ITypeRegistrar
{
    /// <inheritdoc />
    public ITypeResolver Build() => new TypeResolver(services.BuildServiceProvider());

    /// <inheritdoc />
    public void Register(Type service, Type implementation) => services.AddSingleton(service, implementation);

    /// <inheritdoc />
    public void RegisterInstance(Type service, object implementation) => services.AddSingleton(service, implementation);

    /// <inheritdoc />
    public void RegisterLazy(Type service, Func<object> factory) => services.AddSingleton(service, _ => factory());
}
