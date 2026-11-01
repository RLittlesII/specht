namespace Specht.Comparison;

/// <summary>A report bundle confirmed as retained on the release it compares.</summary>
public sealed record ReportReference(ReleaseRecord Release, PublishedBundle Bundle);
