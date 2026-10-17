namespace Specht.Discovery;

/// <summary>A declared test file read once through the discovery store for matrix and trace resolution.</summary>
/// <param name="RelativePath">The file's path relative to the repository root.</param>
/// <param name="Text">The file's complete text.</param>
public sealed record SpecTestFile(string RelativePath, string Text);
