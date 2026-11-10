namespace Specht.Tests.Identifiers;

/// <summary>
/// A repository-wide type and the names of the files its ids are held by in a tree that holds an <c>adr/</c> and a
/// <c>lessons/</c> folder in the root's <c>.spec/</c> and in a Feature's (<c>0001-F19</c> B-038, B-039).
/// </summary>
public sealed class HeldPlaces : TheoryData<string, string[]>
{
    /// <summary>Initializes the <c>adr</c> and <c>lesson</c> rows.</summary>
    public HeldPlaces()
    {
        Add("adr", ["a-root.md", "b-feature.md"]);
        Add("lesson", ["c-root.md", "d-feature.md"]);
    }
}
