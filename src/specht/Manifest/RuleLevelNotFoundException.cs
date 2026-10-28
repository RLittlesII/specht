namespace Specht.Manifest;

/// <summary>A requested rule or level is outside the pinned rule-setting vocabulary.</summary>
/// <implements epic="0001" feature="F11" claim="B-007" />
public sealed class RuleLevelNotFoundException(string message) : Exception(message);
