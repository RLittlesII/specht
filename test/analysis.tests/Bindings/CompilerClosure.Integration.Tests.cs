using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Versioning;
using System.Text.Json;
using AwesomeAssertions;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

[Trait("Tier", "Integration")]
public sealed class CompilerClosureIntegrationTests
{
    [Fact]
    public void PortableComponent_WhenLoadedWithoutRuntimeEngineOrTestFrameworks_ShouldPreserveResolutionAndTextProjection()
    {
        // Given
        var component = typeof(BindingResolver).Assembly;
        var projectRoot = new DirectoryInfo(AppContext.BaseDirectory);
        while (projectRoot is not null && !File.Exists(Path.Combine(projectRoot.FullName, "src", "Analyzers", "Analyzers.csproj")))
        {
            projectRoot = projectRoot.Parent;
        }

        projectRoot.Should().NotBeNull();
        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(projectRoot!.FullName,
            "src", "Analyzers", "obj", "project.assets.json")));
        var selectedTarget = assets.RootElement.GetProperty("targets").GetProperty("netstandard2.0");
        var packageFolders = assets.RootElement.GetProperty("packageFolders").EnumerateObject()
            .Select(static folder => folder.Name).ToArray();
        var paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var selectedAssets = new List<string>();
        foreach (var package in selectedTarget.EnumerateObject())
        {
            if (!package.Value.TryGetProperty("runtime", out var runtime))
            {
                continue;
            }

            var packagePath = assets.RootElement.GetProperty("libraries").GetProperty(package.Name).GetProperty("path").GetString()!;
            foreach (var asset in runtime.EnumerateObject().Where(static entry => entry.Name.EndsWith(".dll", StringComparison.Ordinal)))
            {
                selectedAssets.Add(asset.Name);
                var candidate = packageFolders.Select(folder => Path.Combine(folder, packagePath, asset.Name)).Single(File.Exists);
                paths.Add(Path.GetFileNameWithoutExtension(candidate), candidate);
            }
        }

        var context = new BindingComponentLoadContext(paths);
        var text = """
            ---
            id: F7
            epic: 0042
            spec_status: draft
            ---
            ## 3. Acceptance Criteria
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-007 | Current | agreement | Active |
            | B-008 | Retired | agreement | Withdrawn |
            | B-009 | First declaration | agreement | Active |
            | B-009 | Second declaration | agreement | Active |
            """;
        BindingPolicy policy = new BindingPolicyFixture();

        try
        {
            // When
            var isolated = context.LoadFromAssemblyPath(component.Location);
            var pending = new Queue<Assembly>(new[] { isolated });
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (pending.TryDequeue(out var assembly))
            {
                if (!visited.Add(assembly.GetName().Name!))
                {
                    continue;
                }

                foreach (var reference in assembly.GetReferencedAssemblies())
                {
                    var dependency = context.LoadFromAssemblyName(reference);
                    if (AssemblyLoadContext.GetLoadContext(dependency) == context)
                    {
                        pending.Enqueue(dependency);
                    }
                }
            }

            var bindingNamespace = "Specht.Analysis.Bindings.";
            var omissionType = isolated.GetType(bindingNamespace + "LifecycleOmissions", true)!;
            var policyType = isolated.GetType(bindingNamespace + "BindingPolicy", true)!;
            var omissions = Activator.CreateInstance(omissionType, false, false, false)!;
            var isolatedPolicy = Activator.CreateInstance(policyType, policy.SchemaVersion, policy.Identifiers,
                policy.Markers, policy.DeclaresEpicGrammar, policy.ClaimsSection, omissions)!;
            var readerType = isolated.GetType(bindingNamespace + "BindingFeatureReader", true)!;
            var feature = readerType.GetMethod("Read")!.Invoke(Activator.CreateInstance(readerType),
                new[] { text, "src/sample/.spec/README.md", isolatedPolicy })!;
            var features = Array.CreateInstance(isolated.GetType(bindingNamespace + "BindingFeature", true)!, 1);
            features.SetValue(feature, 0);
            var adrs = Array.CreateInstance(isolated.GetType(bindingNamespace + "BindingAdr", true)!, 0);
            var catalog = Activator.CreateInstance(isolated.GetType(bindingNamespace + "BindingCatalog", true)!, features, adrs)!;
            var resolverType = isolated.GetType(bindingNamespace + "BindingResolver", true)!;
            var resolver = Activator.CreateInstance(resolverType, catalog, isolatedPolicy)!;
            var referenceType = isolated.GetType(bindingNamespace + "ClaimReference", true)!;
            var resolve = resolverType.GetMethod("Resolve", new[] { referenceType })!;
            var findings = new List<(string Status, bool? Eligible, string? Id, string? State, int? Line)>();
            foreach (var claim in new[] { "B-007", "B-008", "B-009", "B-010", "invalid" })
            {
                var reference = Activator.CreateInstance(referenceType, "0042", "F7", claim)!;
                var result = resolve.Invoke(resolver, new[] { reference })!;
                var target = result.GetType().GetProperty("Target")!.GetValue(result);
                findings.Add((result.GetType().GetProperty("Status")!.GetValue(result)!.ToString()!,
                    (bool?)result.GetType().GetProperty("IsEligible")!.GetValue(result),
                    (string?)target?.GetType().GetProperty("Id")!.GetValue(target),
                    (string?)target?.GetType().GetProperty("State")!.GetValue(target),
                    (int?)target?.GetType().GetProperty("Line")!.GetValue(target)));
            }

            var directFeature = new BindingFeatureReader().Read(text, "src/sample/.spec/README.md", policy);
            BindingCatalog directCatalog = new BindingCatalogFixture().WithFeatures(new[] { directFeature });
            var directResolver = new BindingResolver(directCatalog, policy);
            var directFindings = new[] { "B-007", "B-008", "B-009", "B-010", "invalid" }
                .Select(claim => directResolver.Resolve(new ClaimReference("0042", "F7", claim)))
                .Select(static result => (result.Status.ToString(), result.IsEligible,
                    result.Target?.Id, result.Target?.State, result.Target?.Line)).ToArray();

            // Then
            isolated.GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName.Should().Be(".NETStandard,Version=v2.0");
            selectedAssets.Should().NotBeEmpty().And.OnlyContain(static path => path.StartsWith("lib/netstandard2.0/", StringComparison.Ordinal));
            context.Assemblies.Select(static assembly => assembly.GetName().Name!).Should()
                .NotContain(name => context.IsForbidden(name));
            context.Assemblies.Should().Contain(static assembly => assembly.GetName().Name == "Markdig");
            context.Assemblies.Should().Contain(static assembly => assembly.GetName().Name == "YamlDotNet");
            findings.Should().Equal(new (string, bool?, string?, string?, int?)[]
            {
                ("Resolved", true, "B-007", "Active", 9),
                ("Resolved", false, "B-008", "Withdrawn", 10),
                ("Ambiguous", null, null, null, null),
                ("Unresolved", null, null, null, null),
                ("Invalid", null, null, null, null),
            });
            directFindings.Should().Equal(findings);
        }
        finally
        {
            context.Unload();
        }
    }
}
