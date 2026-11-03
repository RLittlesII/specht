namespace Specht.Tests.Model;

/// <summary>
/// Whether a schema 0.3.0 manifest declares the epic grammar, and the identity two specifications declaring <c>F2</c>
/// and no <c>epic</c> share: <c>F2</c> without it (<c>0001-F5</c> B-026) and <c>-F2</c> with it (B-069; decision 0020).
/// </summary>
public sealed class EpicGrammarDuplicates : TheoryData<bool, string>
{
    public EpicGrammarDuplicates()
    {
        Add(false, "F2");
        Add(true, "-F2");
    }
}
