namespace Specht.Tests.Manifest;

/// <summary>
/// A form whose value misses or repeats a placeholder it must hold exactly once, and whether the manifest declares the
/// epic grammar, which does not change the check (<c>0001-F5</c> B-052; decision 0018).
/// </summary>
public sealed class MalformedForms : TheoryData<string, string, bool>
{
    public MalformedForms()
    {
        Add("identity", "{id}", true);
        Add("identity", "{epic}-{id}-{id}", true);
        Add("identity", "{epic}{epic}-{id}", false);
        Add("qualifiedEdge", "{id}", true);
        Add("qualifiedEdge", "{epic}/{id}/{id}", false);
        Add("localEdge", "fixed", true);
        Add("localEdge", "{id}{id}", false);
    }
}
