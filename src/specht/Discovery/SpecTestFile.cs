namespace Specht.Discovery;

/// <summary>A declared test file read once through the discovery store for matrix and trace resolution.</summary>
/// <param name="RelativePath">The file's path relative to the repository root.</param>
/// <param name="Text">The file's complete text.</param>
/// <implements epic="0001" feature="F6" claim="B-001" />
public sealed record SpecTestFile(string RelativePath, string Text);
