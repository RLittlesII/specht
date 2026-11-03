namespace Specht.Tests.Model;

/// <summary>
/// Whether a schema 0.3.0 manifest declares the epic grammar, its local edge form, an edge written by Feature <c>F1</c>
/// to Feature <c>F2</c> - neither carrying an <c>epic</c> - the identity <c>F1</c> is read under, and the edges the
/// graph resolves and leaves unresolved. Without the grammar a local edge resolves by <c>id</c> (<c>0001-F5</c> B-028),
/// in the declared local form too (B-005), and a qualified edge names no Feature (B-029); with it, both Features are
/// read under the identity form rendered with an empty epic (B-069; decision 0020).
/// </summary>
public sealed class EpicLessEdges : TheoryData<bool, string, string, string, string[], string[]>
{
    public EpicLessEdges()
    {
        Add(false, "{id}", "F2", "F1", ["F2"], []);
        Add(false, "#{id}", "#F2", "F1", ["F2"], []);
        Add(false, "{id}", "0002/F2", "F1", [], ["0002/F2"]);
        Add(true, "{id}", "F2", "-F1", ["-F2"], []);
    }
}
