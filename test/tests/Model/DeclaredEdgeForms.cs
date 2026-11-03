namespace Specht.Tests.Model;

/// <summary>
/// A schema 0.3.0 manifest's <c>forms</c>, an edge written in the form it declares, the epic of the Feature <c>F2</c> the
/// edge names, and that Feature's identity under the declared identity form (<c>0001-F5</c> B-005; decision 0010).
/// </summary>
public sealed class DeclaredEdgeForms : TheoryData<string, string, string, string>
{
    public DeclaredEdgeForms()
    {
        Add("""{"qualifiedEdge":"{epic}:{id}"}""", "0002:F2", "0002", "0002-F2");
        Add("""{"localEdge":"~{id}"}""", "~F2", "0001", "0001-F2");
        Add("""{"identity":"{epic}.{id}"}""", "0002/F2", "0002", "0002.F2");
    }
}
