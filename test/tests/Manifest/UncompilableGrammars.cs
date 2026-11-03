namespace Specht.Tests.Manifest;

/// <summary>
/// Every grammar a schema 0.3.0 manifest declares under <c>identifiers</c>, by its key, each compiled at parse
/// (<c>0001-F5</c> B-014; decision 0016).
/// </summary>
public sealed class UncompilableGrammars : TheoryData<string>
{
    public UncompilableGrammars()
    {
        Add("claim");
        Add("task");
        Add("feature");
        Add("epic");
        Add("constraint");
        Add("openQuestion");
    }
}
