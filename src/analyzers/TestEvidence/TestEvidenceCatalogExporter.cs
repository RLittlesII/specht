using System;
using System.Collections.Generic;
using System.Linq;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.TestEvidence;

/// <summary>Projects the analyzer's native claim catalogue into the records the test-evidence artifact carries.</summary>
/// <implements epic="0201" feature="F7" claim="B-030" />
public sealed class TestEvidenceCatalogExporter
{
    public IReadOnlyList<TestEvidenceCatalogFeature> Project(BindingCatalog catalog) => Array.AsReadOnly(catalog.Features
        .OrderBy(static feature => feature.Identity.Epic is not null).ThenBy(static feature => feature.Identity.Epic, StringComparer.Ordinal)
        .ThenBy(static feature => feature.Identity.Feature, StringComparer.Ordinal)
        .Select(static feature => new TestEvidenceCatalogFeature(feature.Identity.Epic, feature.Identity.Feature ?? string.Empty,
            feature.State ?? "current", feature.RelativePath, Array.AsReadOnly(feature.Claims.Select(claim => new TestEvidenceCatalogClaim(
                claim.Id, claim.State ?? "Active", Spelling(claim.Element), claim.RelativePath ?? feature.RelativePath, claim.Line,
                Array.AsReadOnly((claim.Cases ?? []).Select(static item => new TestEvidenceCatalogCase(item.Line, item.Planned)).ToArray()))).ToArray())))
        .ToArray());

    private static string Spelling(BindingClaimElement? element) => element is { } kind
        ? char.ToLowerInvariant(kind.ToString()[0]) + kind.ToString().Substring(1) : "scenario";
}
