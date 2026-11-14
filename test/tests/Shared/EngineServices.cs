using System.IO.Abstractions;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Specht.Check;
using Specht.Composition;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;
using Specht.Tool;
using Specht.Tool.Features.Evidence;
using Specht.Tool.Features.Frontmatter;
using Specht.Tool.Features.Schema;
using Specht.Tree;
using Specht.Versioning;
using Spectre.Console;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Specht.Tests.Shared;

/// <summary>Composes the real engine graph with optional in-memory file system and catalog substitutions.</summary>
public static class EngineServices
{
    /// <summary>Exercises the standalone frontmatter command over substituted input, file-system and, when given, catalog seams.</summary>
    public static CommandAppTester FrontmatterApp(
        IFileSystem fileSystem, TestConsole console, TextReader input, TextWriter errors, SchemaVersions? versions = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(fileSystem);
        if (versions is not null)
        {
            services.AddSingleton(versions);
        }

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

    /// <summary>Exercises the schema slice's production registrations with simulated terminal input and, when given, a synthetic catalog.</summary>
    public static CommandAppTester CommandApp(IFileSystem fileSystem, TestConsole console, SchemaVersions? versions = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(fileSystem);
        if (versions is not null)
        {
            services.AddSingleton(versions);
        }

        services.AddSingleton<IAnsiConsole>(console);
        var app = new CommandAppTester(new TypeRegistrar(services.AddSpechtEngine()), console: console);
        app.Configure(static configuration => configuration.AddSchema());
        return app;
    }

    /// <summary>Resolves direct schema operations with an optional synthetic catalog.</summary>
    public static SchemaVersionManager Commands(SchemaVersions? versions = null) =>
        (SchemaVersionManager) Resolve(typeof(SchemaVersionManager), versions: versions);

    /// <summary>Resolves the engine's check with the chosen catalog, over the supplied file system and child process or the real ones.</summary>
    public static SpechtCheck Check(SchemaVersions? versions = null, IFileSystem? fileSystem = null, IChildProcess? childProcess = null) =>
        (SpechtCheck) Resolve(typeof(SpechtCheck), fileSystem, versions, childProcess);

    /// <summary>Resolves the model reader over the supplied file system and catalog, or the real ones.</summary>
    public static SpechtModelReader Reader(IFileSystem? fileSystem = null, SchemaVersions? versions = null) =>
        (SpechtModelReader) Resolve(typeof(SpechtModelReader), fileSystem, versions);

    /// <summary>Resolves the composed rule set.</summary>
    public static IEnumerable<ISpechtRule> Rules() => (IEnumerable<ISpechtRule>) Resolve(typeof(IEnumerable<ISpechtRule>));

    /// <summary>Resolves the shipped instance catalog.</summary>
    public static SchemaVersions Versions() => (SchemaVersions) Resolve(typeof(SchemaVersions));

    /// <summary>Resolves the pure manifest parser.</summary>
    public static SpechtManifestParser Parser() => (SpechtManifestParser) Resolve(typeof(SpechtManifestParser));

    /// <summary>Resolves the tree generator with the chosen catalog.</summary>
    public static SpechtTreeGenerator Generator(SchemaVersions? versions = null) =>
        (SpechtTreeGenerator) Resolve(typeof(SpechtTreeGenerator), versions: versions);

    /// <summary>Resolves the tree store over the supplied file system.</summary>
    public static SpechtTreeStore Store(IFileSystem fileSystem) => (SpechtTreeStore) Resolve(typeof(SpechtTreeStore), fileSystem);

    /// <summary>Reads and parses a manifest without check-only pin resolution.</summary>
    public static SpechtStructure Manifest(IFileSystem fileSystem, string root) => Reader(fileSystem).ReadManifest(root);

    /// <summary>Reads the manifest document over the existing validation and file-system seams.</summary>
    public static JsonObject EffectiveManifest(string root, IFileSystem? fileSystem = null, SchemaVersions? versions = null) =>
        ((EffectiveManifestReader) Resolve(typeof(EffectiveManifestReader), fileSystem, versions)).Read(root);

    /// <summary>Reads the embedded or verified upstream schemas.</summary>
    public static SpechtSchemas ReadSchemas(IFileSystem fileSystem, string root, SchemaVersions versions) =>
        Reader(fileSystem, versions).ReadSchemas(root, null);

    /// <summary>Reads the explicitly requested local schemas.</summary>
    public static SpechtSchemas ReadDiskSchemas(IFileSystem fileSystem, string root) =>
        Reader(fileSystem).ReadSchemas(root, SchemaSourceSelection.Disk);

    private static object Resolve(Type service, IFileSystem? fileSystem = null, SchemaVersions? versions = null, IChildProcess? childProcess = null)
    {
        var services = new ServiceCollection();
        if (fileSystem is not null)
        {
            services.AddSingleton(fileSystem);
        }

        if (childProcess is not null)
        {
            services.AddSingleton(childProcess);
        }

        if (versions is not null)
        {
            services.AddSingleton(versions);
        }

        return new TypeRegistrar(services.AddSpechtEngine()).Build().Resolve(service)
            ?? throw new InvalidOperationException($"AddSpechtEngine() registers no {service.Name}.");
    }
}
