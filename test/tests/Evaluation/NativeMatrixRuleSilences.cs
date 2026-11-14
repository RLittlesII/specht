namespace Specht.Tests.Evaluation;

/// <summary>
/// The manifest members and the pinned id of each rule on <c>main</c> that resolves a Feature's claims through
/// <c>MatrixResolver.ResolveClaims</c> (<c>0001-F16</c> B-003; § 7's skip table, lifted by item <c>0396</c>, B-023,
/// B-024): <c>SPEC4008</c> at its default level, and <c>SPEC4006</c>, off by default, set to <c>error</c>.
/// </summary>
public sealed class NativeMatrixRuleSilences : TheoryData<string, string>
{
    /// <summary>Initializes the missing-scenario rule and the retired-claim rule.</summary>
    public NativeMatrixRuleSilences()
    {
        Add("{}", "SPEC4008");
        Add("""{ "rules": { "SPEC4006": "error" } }""", "SPEC4006");
    }
}
