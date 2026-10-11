using Microsoft.Extensions.DependencyInjection;
using Specht.Composition;
using Specht.Model;
using Specht.Tool;

namespace Specht.Tests.Shared;

/// <summary>
/// The engine as <see cref="SpechtEngineServiceCollectionExtensions.AddSpechtEngine"/> registers it (ADR-0001 stage D,
/// item 0107), over the real file system: the graph the host resolves, so a test that runs the engine over a tree on
/// disk runs the one that ships. Each read builds its own provider through <see cref="TypeRegistrar.Build"/>, the one
/// call site that builds one (ADR-0003), as each run of the tool does.
/// </summary>
public static class EngineServices
{
    /// <summary>Resolves the runner.</summary>
    /// <returns>The runner.</returns>
    public static SpechtRunner Runner() => (SpechtRunner)Resolve(typeof(SpechtRunner));

    /// <summary>Resolves the model loader.</summary>
    /// <returns>The loader.</returns>
    public static SpecModelLoader Loader() => (SpecModelLoader)Resolve(typeof(SpecModelLoader));

    private static object Resolve(Type service) =>
        new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build().Resolve(service)
        ?? throw new InvalidOperationException($"AddSpechtEngine() registers no {service.Name}.");
}
