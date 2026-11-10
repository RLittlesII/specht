namespace Specht.Tests.Mint;

/// <summary>
/// A repository-wide type and the folder <c>--feature</c> names for it (<c>0001-F19</c> B-016): the folder that holds a
/// co-located Feature's <c>.spec/</c>, a folder above it that holds no <c>.spec/</c> of its own, and a folder that does not
/// exist, so the refusal holds whatever folder is named and under every answer to OQ-16.
/// </summary>
public sealed class FoldersNamedForRepositoryWideTypes : TheoryData<string, string>
{
    /// <summary>Initializes one row per type and folder.</summary>
    public FoldersNamedForRepositoryWideTypes()
    {
        foreach (var type in new[] { "adr", "lesson" })
        {
            Add(type, "features/one");
            Add(type, "features");
            Add(type, "absent");
        }
    }
}
