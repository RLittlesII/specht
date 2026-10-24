using System.Collections.Generic;

namespace Specht.Analysis.Scenarios;

/// <summary>Normalized, framework-neutral metadata for one Gherkin scenario, supplied by a consumer claim bridge.</summary>
/// <implements epic="0201" feature="F4" claim="B-008" />
public sealed record ScenarioMetadata(
    string Path,
    string? ScenarioId,
    int Line,
    ScenarioScope FeatureScope,
    ScenarioScope? ScopeOverride,
    IReadOnlyList<string> ClaimTags,
    IReadOnlyList<ScenarioRow> Rows,
    bool GeneratorInputPresent);
