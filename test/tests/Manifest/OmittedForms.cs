namespace Specht.Tests.Manifest;

/// <summary>
/// A form a schema 0.3.0 manifest leaves out while declaring the other two, and the value the pinned default manifest
/// gives it (<c>0001-F5</c> B-019, B-048; decision 0010).
/// </summary>
public sealed class OmittedForms : TheoryData<string, string>
{
    public OmittedForms()
    {
        Add("identity", "{epic}-{id}");
        Add("localEdge", "{id}");
        Add("qualifiedEdge", "{epic}/{id}");
    }
}
