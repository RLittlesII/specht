namespace Specht.Analysis.Bindings;

/// <summary>Identity validation and declaration cardinality, independently of lifecycle eligibility.</summary>
public enum BindingResolution
{
    /// <summary>A required target or owner identity component is missing.</summary>
    Incomplete,
    /// <summary>A supplied identity component violates its configured grammar or is forbidden by the owner model.</summary>
    Invalid,
    /// <summary>A complete, valid identity has no matching target declaration in the selected owner.</summary>
    Unresolved,
    /// <summary>The selected owner has duplicate declarations or more than one target declaration matches.</summary>
    Ambiguous,
    /// <summary>Exactly one target declaration matches in a unique selected owner, independently of lifecycle eligibility.</summary>
    Resolved,
}
