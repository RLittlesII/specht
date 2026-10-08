using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AwesomeAssertions;
using Reqnroll;
using specht;

namespace specht.acceptance.Engine;

/// <summary>
/// Steps for <c>src/specht/.spec/engine.feature</c> (0001-F1). B-005 reads the built library itself: its assembly name,
/// and the namespace of every type it declares, which is where a root namespace shows once compiled.
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
    public void GivenTheLibraryIsBuilt() => _library = typeof(SpecCheckRunner).Assembly;

    [When("its assembly name and root namespace are read")]
    public void WhenItsAssemblyNameAndRootNamespaceAreRead()
    {
        _assemblyName = Library.GetName().Name;
        _namespaces = Library.GetTypes()
            .Where(static type => type.Namespace is not null && !type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)))
            .Select(static type => type.Namespace!)
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

    private Assembly Library => _library ?? throw new InvalidOperationException("No library was built.");

    private static string FindRepository(string directory) =>
        File.Exists(Path.Combine(directory, "build.sh")) && Directory.Exists(Path.Combine(directory, ".nuke"))
            ? directory
            : FindRepository(Path.GetDirectoryName(directory.TrimEnd(Path.DirectorySeparatorChar))
                ?? throw new InvalidOperationException("No repository root above the test assembly."));

    private readonly string _root = Path.Combine(Path.GetTempPath(), "specht-engine", Guid.NewGuid().ToString("N"));
    private Assembly? _library;
    private string? _assemblyName;
    private string[] _namespaces = [];
}
