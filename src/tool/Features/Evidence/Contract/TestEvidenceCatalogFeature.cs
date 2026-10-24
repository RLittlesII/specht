using System.Collections.Generic;

namespace Specht.Tool.Features.Evidence.Contract;

public sealed record TestEvidenceCatalogFeature(
    string? Epic,
    string Feature,
    string State,
    string Path,
    IReadOnlyList<TestEvidenceCatalogClaim> Claims);
