using System.Collections.Generic;

namespace Specht.Model;

/// <summary>One claim a Gherkin element declares, with its lifecycle state, keyword line, element kind and cases.</summary>
/// <implements epic="0201" feature="F7" claim="B-008" />
public sealed record GherkinClaimDeclaration(
    string Id,
    string State,
    int Line,
    GherkinClaimElement Element,
    IReadOnlyList<GherkinClaimCaseLine> Cases);
