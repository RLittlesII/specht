namespace Specht.Versioning;

/// <summary>A target version named by the caller is not in the shipped catalog (0001-F7 B-048).</summary>
/// <param name="value">The requested version.</param>
/// <param name="available">The shipped versions.</param>
/// <implements epic="0001" feature="F7" claim="B-001" />
public sealed class SchemaVersionNotFoundException(string value, IEnumerable<SemanticVersion> available)
    : Exception($"schema version '{value}' is not shipped; this tool ships {string.Join(", ", available)}.");
