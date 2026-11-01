namespace Specht.Comparison;

/// <summary>A caller-supplied exporter document for one side, named by its repository-relative source path.</summary>
public sealed record RawExport(string Side, string SourcePath);
