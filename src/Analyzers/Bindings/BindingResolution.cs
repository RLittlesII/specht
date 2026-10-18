namespace Specht.Analysis.Bindings;

/// <summary>Identity validation and declaration cardinality, independently of lifecycle eligibility.</summary>
public enum BindingResolution
{
    Incomplete,
    Invalid,
    Unresolved,
    Ambiguous,
    Resolved,
}
