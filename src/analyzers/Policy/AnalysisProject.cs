using System.Collections.Generic;

namespace Specht.Analysis.Policy;

/// <summary>A declared aggregation project, its selected target frameworks and its required-case scope.</summary>
public sealed record AnalysisProject(string Path, IReadOnlyList<string> TargetFrameworks, CaseScope Scope);
