using System.Collections.Generic;

namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceCatalogClaim(
    string Id,
    string State,
    string Element,
    string Path,
    int Line,
    IReadOnlyList<TestEvidenceCatalogCase> Cases);
