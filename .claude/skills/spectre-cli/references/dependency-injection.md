---
title: Dependency Injection
description: The ITypeRegistrar/ITypeResolver bridge to Microsoft.Extensions.DependencyInjection.
type: reference
---

# Dependency Injection

Spectre.Console.Cli doesn't depend on `Microsoft.Extensions.DependencyInjection` directly — it
integrates through two bridge interfaces you implement once per app.

## `ITypeRegistrar` / `ITypeResolver`

```csharp
public sealed class TypeRegistrar(IServiceCollection services) : ITypeRegistrar
{
    public ITypeResolver Build() => new TypeResolver(services.BuildServiceProvider());

    public void Register(Type service, Type implementation) =>
        services.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation) =>
        services.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> factory) =>
        services.AddSingleton(service, _ => factory());
}

public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver, IDisposable
{
    public object? Resolve(Type? type) => type == null ? null : provider.GetService(type);
    public void Dispose() => (provider as IDisposable)?.Dispose();
}
```

Wire it up once in `Program.cs`:

```csharp
var services = new ServiceCollection();
// register app services here...

var app = new CommandApp(new TypeRegistrar(services));
```

## What This Buys You

- **Constructor injection** — commands declare dependencies in their constructor; the framework
  resolves them through the registered `ITypeResolver`.
- **Settings injection** — `CommandSettings` instances are registered automatically and available
  for injection alongside regular services.
- **`IAnsiConsole`** — inject it instead of using static `AnsiConsole.*` calls; see
  [Help & Output](help-and-output.md) for why this matters for testing.
- **Keyed services (.NET 8+)** — multiple implementations of the same interface, selected by a
  discriminator key, resolve the same way as anywhere else in the DI container.

## Mandatory Requirements

- **NEVER** call `services.BuildServiceProvider()` more than once — `TypeRegistrar.Build()` is the
  single place that happens.
- **NEVER** hand one `ServiceCollection` to a second `CommandApp`. On each run Spectre registers
  its configuration, every command and settings type and the console into it through the
  registrar, so the collection is single-use. _This repository's decision: ADR-0003._

> What a command injects is a `dotnet-tool` concern, not a `spectre-cli` one — see
> [dotnet-tool](../../dotnet-tool/SKILL.md). This skill only covers the generic
> `ITypeRegistrar`/`ITypeResolver` bridge; it stays agnostic about what gets registered in it.
