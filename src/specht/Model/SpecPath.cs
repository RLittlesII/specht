namespace Specht.Model;

/// <summary>The root-relative path mapping every path the engine emits passes through (<c>0001-F3</c> B-021; ADR-0005 (a)).</summary>
public static class SpecPath
{
    /// <summary>Gets <paramref name="path"/> relative to <paramref name="root"/>, with <c>/</c> separators (<c>0001-F3</c> B-021).</summary>
    /// <param name="root">The root the tool was given.</param>
    /// <param name="path">A path under it.</param>
    /// <returns>The root-relative path.</returns>
    public static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
}
