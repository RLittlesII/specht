using System.Collections.Generic;

namespace Specht.Model;

/// <summary>
/// One claim a Gherkin element declares, with its lifecycle state, keyword line, element kind, cases, and the keyword lines
/// of the Scenarios and Scenario Outlines carrying it, in document order.
/// </summary>
/// <implements epic="0201" feature="F7" claim="B-008" />
/// <implements epic="0001" feature="F16" claim="B-025" />
public sealed record GherkinClaimDeclaration(
    string Id,
    string State,
    int Line,
    GherkinClaimElement Element,
    IReadOnlyList<GherkinClaimCaseLine> Cases,
    IReadOnlyList<int> Scenarios);
