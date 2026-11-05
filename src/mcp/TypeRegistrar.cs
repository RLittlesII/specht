using System;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Specht.Mcp;

/// <summary>Spectre's bridge to <see cref="IServiceCollection"/>.</summary>
/// <param name="services">The registrations the factory lists.</param>
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
