using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Policy;

/// <summary>
/// A claim reference to the Feature <c>cap:read</c>, which declares <c>need/7</c>, and its resolution under a projected
/// claim record of <c>plain</c> <c>need/{id}</c> and <c>id</c> <c>[0-9]+</c> (<c>0201-F1</c> B-023): the declared claim,
/// a well-formed claim it does not declare, another head, a partial match, and a head in another case.
/// </summary>
public sealed class ClaimRecordReferences : TheoryData<string, BindingResolution>
{
    /// <summary>Initializes the rows.</summary>
    public ClaimRecordReferences()
    {
        Add("need/7", BindingResolution.Resolved);
        Add("need/8", BindingResolution.Unresolved);
        Add("want/7", BindingResolution.Invalid);
        Add("need/7a", BindingResolution.Invalid);
        Add("NEED/7", BindingResolution.Invalid);
    }
}
