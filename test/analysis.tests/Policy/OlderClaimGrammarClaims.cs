namespace Specht.Analysis.Tests.Policy;

/// <summary>
/// A Feature's claim and whether the analysis is available when the projection supplies the older claim grammar
/// <c>^need/[0-9]+$</c> and no claim record (<c>0201-F1</c> B-024).
/// </summary>
public sealed class OlderClaimGrammarClaims : TheoryData<string, bool>
{
    /// <summary>Initializes the rows.</summary>
    public OlderClaimGrammarClaims()
    {
        Add("need/7", true);
        Add("want/7", false);
        Add("need/7a", false);
    }
}
