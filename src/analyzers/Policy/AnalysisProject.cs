using System.Collections.Generic;

namespace Specht.Analysis.Policy;

/// <summary>A declared aggregation project and its selected target frameworks.</summary>
public sealed record AnalysisProject(string Path, IReadOnlyList<string> TargetFrameworks);
