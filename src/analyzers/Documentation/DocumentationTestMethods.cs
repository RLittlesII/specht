using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Specht.Analysis.TestEvidence;

namespace Specht.Analysis.Documentation;

/// <summary>Reads supplied method recognition for analysis and invoked repairs.</summary>
public sealed class DocumentationTestMethods
{
    public HashSet<string> Read(AnalyzerOptions options, CancellationToken cancellationToken)
    {
        var methods = new HashSet<string>(StringComparer.Ordinal);
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

            var eligibleMethods = new HashSet<string>(StringComparer.Ordinal);
            foreach (var binding in evidence.Artifact!.Bindings)
            {
                if (binding.ShapeValid && binding.Status == "resolved" && binding.IsEligible == true)
                {
                    eligibleMethods.Add(binding.SymbolId);
                }
            }

            foreach (var method in evidence.Artifact.Discovery.Methods)
            {
                if (eligibleMethods.Contains(method.SymbolId))
                {
                    methods.Add(method.SymbolId);
                }
            }
        }

        return methods;
    }
}
