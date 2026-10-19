using System;
using System.Collections.Generic;
using System.Linq;
using Specht.Analysis.Bindings;
using Specht.Analysis.Documentation;

namespace Specht.Analysis.TestEvidence;

public sealed class TestEvidenceBindingExporter
{
    public IReadOnlyList<TestEvidenceBinding> Export(DocumentationAnalysis analysis, string relativePath)
    {
        if (!IsRepositoryRelative(relativePath))
        {
            throw new ArgumentException("The source path must be repository-relative.", nameof(relativePath));
        }

        return analysis.Elements.Select(binding => Export(binding, relativePath)).ToArray();
    }

    private static TestEvidenceBinding Export(DocumentationBinding binding, string path)
    {
        var resolution = binding.Resolution;
        if (binding.ShapeValid && resolution is null)
        {
            throw new InvalidOperationException("A valid relationship must carry its F2 resolution.");
        }

        var lineSpan = binding.Location.GetLineSpan();
        var location = lineSpan.IsValid
            ? new TestEvidenceLocation(path, lineSpan.StartLinePosition.Line + 1,
                lineSpan.StartLinePosition.Character + 1)
            : null;
        return new TestEvidenceBinding(binding.SymbolId, binding.Role, binding.Attributes, binding.ShapeValid,
            binding.ShapeValid ? resolution!.Status.ToString().ToLowerInvariant() : null,
            binding.ShapeValid ? resolution!.IsEligible : null,
            Target(resolution?.Target), resolution?.Matches.Select(static match => Target(match)!).ToArray()
                ?? Array.Empty<TestEvidenceTarget>(), location);
    }

    private static TestEvidenceTarget? Target(BindingTarget? target) => target is null ? null : new TestEvidenceTarget(
        target.Kind.ToString().ToLowerInvariant(), target.Epic, target.Feature, target.Id, target.State, target.FeatureState,
        target.RelativePath, target.Line);

    private static bool IsRepositoryRelative(string path) => !string.IsNullOrEmpty(path)
        && path[0] != '/' && !path.Contains('\\') && !(path.Length > 1 && char.IsLetter(path[0]) && path[1] == ':')
        && !path.Split('/').Any(static component => component is "" or "." or "..");
}
