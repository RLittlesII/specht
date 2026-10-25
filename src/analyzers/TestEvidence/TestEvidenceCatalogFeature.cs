using System.Collections.Generic;

namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceCatalogFeature(
    string? Epic,
    string Feature,
    string State,
    string Path,
    IReadOnlyList<TestEvidenceCatalogClaim> Claims);
