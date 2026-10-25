namespace Specht.Tool.Features;

/// <summary>One shipped file: its root-relative path with <c>/</c> separators and its embedded bytes.</summary>
/// <param name="Relative">The path under a consumer root, such as <c>.specht/templates/feature.md</c>.</param>
/// <param name="Bytes">The embedded bytes.</param>
/// <implements epic="0001" feature="F7" claim="B-005" />
public sealed record ShippedFile(string Relative, byte[] Bytes);
