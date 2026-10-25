using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Specht.Analysis.TestEvidence;

namespace Specht.Analysis.Documentation;

/// <summary>Reads supplied method recognition for analysis and invoked repairs.</summary>
public sealed class DocumentationTestMethods
{
    public TestMethodRecognition Read(AnalyzerOptions options, Compilation compilation, CancellationToken cancellationToken)
    {
        var methods = new HashSet<string>(StringComparer.Ordinal);
        var authoritative = false;

        CompilationIdentity? identity = null;
        foreach (var file in options.AdditionalFiles)
        {
            if (!options.AnalyzerConfigOptionsProvider.GetOptions(file)
                    .TryGetValue("build_metadata.AdditionalFiles.SpechtInputKind", out var kind) || kind != "test-evidence")
            {
                continue;
            }

            var evidence = new TestEvidenceArtifactReader().Read(file.GetText(cancellationToken)?.ToString() ?? "null");
            if (!evidence.IsValid)
            {
                continue;
            }

            identity ??= Identity(options, compilation, cancellationToken);
            var artifact = evidence.Artifact!;
            if (identity is null || artifact.Discovery.State != TestEvidenceDiscoveryState.Complete
                || artifact.ProjectPath != identity.ProjectPath || artifact.TargetFramework != identity.TargetFramework
                || artifact.ConfigurationName != identity.Configuration || artifact.SourceFingerprint != identity.SourceFingerprint
                || artifact.PolicyFingerprint != identity.PolicyFingerprint)
            {
                continue;
            }

            authoritative = true;
            var eligibleMethods = new HashSet<string>(StringComparer.Ordinal);
            foreach (var binding in artifact.Bindings)
            {
                if (binding.ShapeValid && binding.Status == "resolved" && binding.IsEligible == true)
                {
                    eligibleMethods.Add(binding.SymbolId);
                }
            }

            foreach (var method in artifact.Discovery.Methods)
            {
                if (eligibleMethods.Contains(method.SymbolId))
                {
                    methods.Add(method.SymbolId);
                }
            }
        }

        return new TestMethodRecognition(methods, authoritative);
    }

    private static CompilationIdentity? Identity(AnalyzerOptions options, Compilation compilation, CancellationToken cancellationToken)
    {
        var global = options.AnalyzerConfigOptionsProvider.GlobalOptions;
        if (!global.TryGetValue("build_property.SpechtRoot", out var root)
            || !global.TryGetValue("build_property.SpechtProjectPath", out var projectPath)
            || !global.TryGetValue("build_property.TargetFramework", out var framework)
            || !global.TryGetValue("build_property.Configuration", out var configuration))
        {
            return null;
        }

        var indexer = new TestEvidenceCompilationInputs();
        root = Path.GetFullPath(root);
        var project = indexer.NormalizeRelative(root, Path.GetFullPath(Path.Combine(root, projectPath)));
        if (project is null)
        {
            return null;
        }

        var inputs = indexer.Read(root, options.AdditionalFiles, compilation.SyntaxTrees, options.AnalyzerConfigOptionsProvider,
            cancellationToken);
        var fingerprint = new TestEvidenceFingerprint();
        return new CompilationIdentity(project, framework, configuration, fingerprint.Source(inputs), fingerprint.Policy(inputs));
    }

    private sealed record CompilationIdentity(string ProjectPath, string TargetFramework, string Configuration,
        string SourceFingerprint, string PolicyFingerprint);
}
