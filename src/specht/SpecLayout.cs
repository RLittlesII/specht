namespace specht;

/// <summary>Which layout a specification was discovered in.</summary>
public enum SpecLayout
{
    /// <summary><c>epics/&lt;epic&gt;/&lt;feature&gt;/spec.md</c> - the layout being migrated away from.</summary>
    Legacy,

    /// <summary><c>&lt;area&gt;/.spec/README.md</c> - co-located with the code the Feature owns.</summary>
    CoLocated,
}
