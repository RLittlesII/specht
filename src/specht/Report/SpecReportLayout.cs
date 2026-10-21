namespace Specht.Report;

/// <summary>One layout and how many specifications were discovered in it (B-005).</summary>
/// <param name="Layout">The layout's name in the manifest (<c>0001-F6</c> B-009).</param>
/// <param name="SpecificationCount">How many specifications it holds.</param>
/// <implements epic="0001" feature="F3" claim="B-001" />
public sealed record SpecReportLayout(string Layout, int SpecificationCount);
