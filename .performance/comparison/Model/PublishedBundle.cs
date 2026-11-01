namespace Specht.Comparison;

/// <summary>A report bundle confirmed as a retained release asset.</summary>
public sealed record PublishedBundle(long AssetId, string Name, string Sha256);
