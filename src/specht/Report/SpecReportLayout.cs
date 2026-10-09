namespace specht.Report;

/// <summary>One layout and how many specifications were discovered in it (B-005).</summary>
/// <param name="Layout">The layout.</param>
/// <param name="SpecificationCount">How many specifications it holds.</param>
public sealed record SpecReportLayout(SpecLayout Layout, int SpecificationCount);
