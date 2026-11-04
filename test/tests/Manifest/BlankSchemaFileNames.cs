namespace Specht.Tests.Manifest;

/// <summary>
/// Each frontmatter schema kind, and the empty text or <c>.</c> as its file name (<c>0001-F5</c> B-075; decision 0021
/// call 4).
/// </summary>
public sealed class BlankSchemaFileNames : TheoryData<string, string>
{
    public BlankSchemaFileNames()
    {
        foreach (var kind in new[] { "feature", "task", "epic" })
        {
            Add(kind, string.Empty);
            Add(kind, ".");
        }
    }
}
