namespace Specht.Tests.Evaluation;

/// <summary>
/// The manifest members and the pinned id of each rule on <c>main</c> that reads a Feature's claims or matrix through
/// <c>MatrixResolver</c> (<c>0001-F16</c> B-003; § 7's skip table, lifted by item <c>0396</c>, B-023, B-024):
/// <c>SPEC4008</c> and <c>SPEC4010</c> at their default level, and <c>SPEC4006</c>, off by default, set to <c>error</c>.
/// Each set names <c>.spec/features/arithmetic/README.md</c> as its record.
/// </summary>
public sealed class NativeMatrixRuleSilences : TheoryData<string, string>
{
    /// <summary>Initializes the missing-scenario rule, the retired-claim rule and the status rule.</summary>
    public NativeMatrixRuleSilences()
    {
        Add($$"""{ {{Files}} }""", "SPEC4008");
        Add($$"""{ {{Files}}, "rules": { "SPEC4006": "error" } }""", "SPEC4006");
        Add($$"""{ {{Files}} }""", "SPEC4010");
    }

    private const string Files = """
        "claim": { "files": [
          { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" } ] }
        """;
}
