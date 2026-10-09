using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using AwesomeAssertions;
using Reqnroll;
using specht;

namespace specht.acceptance.Engine;

/// <summary>
/// Steps for <c>src/specht/.spec/engine.feature</c> (0001-F1). B-005 reads the built library itself: its assembly name,
/// and the namespace of every type it declares, which is where a root namespace shows once compiled. It reads the file
/// <c>src/specht</c> builds, from its metadata, and never the loaded assembly: a coverage run instruments the copy the
/// test process loads (0055-F3 B-001) and adds a type of its own to it.
/// </summary>
[Binding]
public sealed class EngineSteps
{
    // A synthetic root under the temp folder, holding this repository's live version 1 schema set (A-3).
    [Given("a repository root holding a manifest and the three frontmatter schemas of schema version {int}")]
    public void GivenARepositoryRootHoldingAManifestAndTheThreeFrontmatterSchemasOfSchemaVersion(int version)
    {
        version.Should().Be(1);
        var schema = Directory.CreateDirectory(Path.Combine(_root, ".spec", "schema"));
        foreach (var file in Directory.GetFiles(Path.Combine(FindRepository(AppContext.BaseDirectory), ".spec", "schema"), "*.json"))
        {
            File.Copy(file, Path.Combine(schema.FullName, Path.GetFileName(file)));
        }
    }

    [Given("the library is built")]
    public void GivenTheLibraryIsBuilt()
    {
        var output = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var configuration = typeof(EngineSteps).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration
            ?? throw new InvalidOperationException("The test assembly names no configuration.");
        _library = Path.Combine(
            FindRepository(output),
            "src",
            "specht",
            "bin",
            configuration,
            Path.GetFileName(output),
            Path.GetFileName(typeof(SpecCheckRunner).Assembly.Location));
        File.Exists(_library).Should().BeTrue(_library);
    }

    [When("its assembly name and root namespace are read")]
    public void WhenItsAssemblyNameAndRootNamespaceAreRead()
    {
        using var file = new PEReader(File.OpenRead(Library));
        var metadata = file.GetMetadataReader();
        _assemblyName = metadata.GetString(metadata.GetAssemblyDefinition().Name);
        _namespaces = metadata.TypeDefinitions
            .Select(metadata.GetTypeDefinition)
            .Where(type => !IsCompilerGenerated(metadata, type))
            .Select(type => NamespaceOf(metadata, type))
            .Where(static ns => ns.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    [Then("both are {string}")]
    public void ThenBothAre(string name)
    {
        _assemblyName.Should().Be(name);
        _namespaces.Should().NotBeEmpty().And.AllSatisfy(ns => (ns == name || ns.StartsWith($"{name}.", StringComparison.Ordinal)).Should().BeTrue(ns));
    }

    [Then("neither contains {string}")]
    public void ThenNeitherContains(string name)
    {
        _assemblyName.Should().NotContain(name);
        _namespaces.Should().AllSatisfy(ns => ns.Should().NotContain(name));
    }

    [AfterScenario]
    public void DeleteRoot()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private string Library => _library ?? throw new InvalidOperationException("No library was built.");

    private static string NamespaceOf(MetadataReader metadata, TypeDefinition type) =>
        type.GetDeclaringType().IsNil
            ? metadata.GetString(type.Namespace)
            : NamespaceOf(metadata, metadata.GetTypeDefinition(type.GetDeclaringType()));

    private static bool IsCompilerGenerated(MetadataReader metadata, TypeDefinition type) =>
        type.GetCustomAttributes()
            .Select(metadata.GetCustomAttribute)
            .Any(attribute => AttributeTypeName(metadata, attribute) == typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute).FullName);

    private static string? AttributeTypeName(MetadataReader metadata, CustomAttribute attribute)
    {
        var parent = attribute.Constructor.Kind switch
        {
            HandleKind.MemberReference => metadata.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent,
            HandleKind.MethodDefinition => metadata.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor).GetDeclaringType(),
            _ => default(EntityHandle),
        };

        return parent.Kind switch
        {
            HandleKind.TypeReference => metadata.GetTypeReference((TypeReferenceHandle)parent) is var reference
                ? $"{metadata.GetString(reference.Namespace)}.{metadata.GetString(reference.Name)}"
                : null,
            HandleKind.TypeDefinition => metadata.GetTypeDefinition((TypeDefinitionHandle)parent) is var definition
                ? $"{metadata.GetString(definition.Namespace)}.{metadata.GetString(definition.Name)}"
                : null,
            _ => null,
        };
    }

    private static string FindRepository(string directory) =>
        File.Exists(Path.Combine(directory, "build.sh")) && Directory.Exists(Path.Combine(directory, ".nuke"))
            ? directory
            : FindRepository(Path.GetDirectoryName(directory.TrimEnd(Path.DirectorySeparatorChar))
                ?? throw new InvalidOperationException("No repository root above the test assembly."));

    private readonly string _root = Path.Combine(Path.GetTempPath(), "specht-engine", Guid.NewGuid().ToString("N"));
    private string? _library;
    private string? _assemblyName;
    private string[] _namespaces = [];
}
