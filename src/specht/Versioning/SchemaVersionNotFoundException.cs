namespace Specht.Versioning;

/// <summary>A target version named by the caller is not in the shipped catalog (0001-F7 B-048).</summary>
/// <param name="value">The requested version.</param>
/// <param name="available">The shipped versions.</param>
public sealed class SchemaVersionNotFoundException(string value, IEnumerable<SemanticVersion> available)
    : Exception($"schema version '{value}' is not shipped; this tool ships {string.Join(", ", available)}.");
