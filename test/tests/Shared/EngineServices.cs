using System.IO.Abstractions;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Specht.Composition;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;
using Specht.Tool;
using Specht.Tool.Features.Evidence;
using Specht.Tool.Features.Frontmatter;
using Specht.Tool.Features.Schema;
using Specht.Versioning;
using Spectre.Console;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Specht.Tests.Shared;

/// <summary>Composes the real engine graph with optional in-memory file system and catalog substitutions.</summary>
public static class EngineServices
{
    /// <summary>Exercises the standalone frontmatter command over substituted input and file-system seams.</summary>
    public static CommandAppTester FrontmatterApp(IFileSystem fileSystem, TestConsole console, TextReader input, TextWriter errors)
    {
        var services = new ServiceCollection();
        services.AddSingleton(fileSystem);
        services.AddSingleton<IAnsiConsole>(console);
        services.AddSingleton(input);
        services.AddSingleton(errors);
        var app = new CommandAppTester(new TypeRegistrar(services.AddSpechtEngine()), console: console);
        app.Configure(static configuration => configuration.AddFrontmatter());
        return app;
    }

    /// <summary>Exercises the evidence command over its production registrations and a capturing console.</summary>
    public static CommandAppTester EvidenceApp(TestConsole console)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAnsiConsole>(console);
        services.AddSingleton<EvidenceReconciler>();
        var app = new CommandAppTester(new TypeRegistrar(services.AddSpechtEngine()), console: console);
        app.Configure(static configuration => configuration.AddCommand<EvidenceCommand>("evidence"));
        return app;
    }

    /// <summary>Exercises the schema slice's production registrations with simulated terminal input.</summary>
    public static CommandAppTester CommandApp(IFileSystem fileSystem, TestConsole console)
    {
        var services = new ServiceCollection();
        services.AddSingleton(fileSystem);
        services.AddSingleton<IAnsiConsole>(console);
        var app = new CommandAppTester(new TypeRegistrar(services.AddSpechtEngine()), console: console);
        app.Configure(static configuration => configuration.AddSchema());
        return app;
    }

    /// <summary>Resolves direct schema operations with an optional synthetic catalog.</summary>
    public static SchemaVersionManager Commands(SchemaVersions? versions = null) =>
        (SchemaVersionManager)Resolve(typeof(SchemaVersionManager), versions: versions);

    /// <summary>Resolves a runner with the chosen catalog.</summary>
    public static SpechtRunner Runner(SchemaVersions? versions = null) => (SpechtRunner)Resolve(typeof(SpechtRunner), versions: versions);

    /// <summary>Resolves the model loader over the supplied file system, or the real one.</summary>
    public static SpechtModelLoader Loader(IFileSystem? fileSystem = null) => (SpechtModelLoader)Resolve(typeof(SpechtModelLoader), fileSystem);

    /// <summary>Resolves the composed rule set.</summary>
    public static IEnumerable<ISpechtRule> Rules() => (IEnumerable<ISpechtRule>)Resolve(typeof(IEnumerable<ISpechtRule>));

    /// <summary>Resolves the shipped instance catalog.</summary>
    public static SchemaVersions Versions() => (SchemaVersions)Resolve(typeof(SchemaVersions));

    /// <summary>Resolves the pure manifest parser.</summary>
    public static SpechtManifestParser Parser() => (SpechtManifestParser)Resolve(typeof(SpechtManifestParser));

    /// <summary>Resolves the schema stage over the supplied file system and catalog.</summary>
    public static SpechtSchemasLoader Schemas(IFileSystem fileSystem, SchemaVersions? versions = null) =>
        (SpechtSchemasLoader)Resolve(typeof(SpechtSchemasLoader), fileSystem, versions);

    /// <summary>Reads and parses a manifest without check-only pin resolution.</summary>
    public static SpechtStructure Manifest(IFileSystem fileSystem, string root) => Schemas(fileSystem).LoadManifest(root);

    /// <summary>Reads the manifest document over the existing validation and file-system seams.</summary>
    public static JsonObject EffectiveManifest(string root, IFileSystem? fileSystem = null, SchemaVersions? versions = null) =>
        ((EffectiveManifestReader)Resolve(typeof(EffectiveManifestReader), fileSystem, versions)).Read(root);

    /// <summary>Loads the embedded or verified upstream schemas.</summary>
    public static SpechtSchemas LoadSchemas(IFileSystem fileSystem, string root, SchemaVersions versions) => Schemas(fileSystem, versions).Load(root);

    /// <summary>Loads the explicitly requested local schemas.</summary>
    public static SpechtSchemas LoadOnDiskSchemas(IFileSystem fileSystem, string root) => Schemas(fileSystem).LoadOnDisk(root);

    private static object Resolve(Type service, IFileSystem? fileSystem = null, SchemaVersions? versions = null)
    {
        var services = new ServiceCollection();
        if (fileSystem is not null)
        {
            services.AddSingleton(fileSystem);
        }

        if (versions is not null)
        {
            services.AddSingleton(versions);
        }

        return new TypeRegistrar(services.AddSpechtEngine()).Build().Resolve(service)
            ?? throw new InvalidOperationException($"AddSpechtEngine() registers no {service.Name}.");
    }
}
