using System.Collections.Generic;

namespace Specht.Tool.Features.Evidence.Contract;

/// <implements epic="0201" feature="F3" claim="B-009" />
public sealed record TestEvidenceCatalogClaim(
    string Id,
    string State,
    string Element,
    string Path,
    int Line,
    IReadOnlyList<TestEvidenceCatalogCase> Cases);
