namespace Specht.Tests.Model;

/// <summary>
/// Whether a schema 0.3.0 manifest declares the epic grammar, the frontmatter of the Feature holding an item whose
/// <c>parent</c> is the bare <c>F2</c>, and the lines <c>SPEC5003</c> reports a parent mismatch at: none without the
/// grammar, whatever the Feature's frontmatter carries (<c>0001-F5</c> B-030), and the <c>parent</c> line with it (B-031).
/// </summary>
public sealed class BareItemParents : TheoryData<bool, string, int[]>
{
    public BareItemParents()
    {
        Add(false, "---\nid: F2\n---\n", []);
        Add(false, "---\nepic: '0001'\nid: F2\n---\n", []);
        Add(true, "---\nepic: '0001'\nid: F2\n---\n", [3]);
    }
}
