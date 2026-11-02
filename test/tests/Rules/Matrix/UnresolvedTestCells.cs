namespace Specht.Tests.Rules.Matrix;

/// <summary>Test cells under configured markers, declared files and forms, and no tests key (B-002 to B-007).</summary>
public sealed class UnresolvedTestCells : TheoryData<string?, string, string, string?>
{
    public UnresolvedTestCells()
    {
        const string declared = """{"files":["test/**/*.cs"]}""";
        const string forms = """{"files":["test/**/*.fs"],"declarations":["let {test} "]}""";
        Add(declared, "`AlphaTest`, `BetaTest`", "Done", null);
        Add(declared, "`AlphaTest; BetaTest`", "Partial", null);
        Add(declared, "`AlphaTest, GammaTest`", "Done", "GammaTest");
        Add(declared, "`AlphaTest` and the GammaTest one", "Done", null);
        Add(declared, "`ReportSteps.AlphaTest`", "Done", null);
        Add(declared, "`ScenarioNobodyWrote`", "Absent", "ScenarioNobodyWrote");
        Add(declared, "`HiddenTest`", "Done", "HiddenTest");
        Add(forms, "`DeclaredTest`", "Done", null);
        Add(forms, "`CommentedTest`", "Done", "CommentedTest");
        Add(declared, "Absent", "Done", null);
        Add(declared, "spec-reviewer, 2026-10-14", "Excused", null);
        Add(declared, "`ScenarioNobodyWrote`", "Retired", null);
        Add(declared, "replaced by B-009", "Replaced", null);
        Add(declared, "`ScenarioNobodyWrote`", "Waived", "ScenarioNobodyWrote");
        Add(declared, "AlphaTest", "Done", "inline code");
        Add(declared, "Missing", "Done", "inline code");
        Add(declared, "", "Partial", "inline code");
        Add(null, "`ScenarioNobodyWrote`", "Done", null);
        Add(null, "AlphaTest", "Done", null);
    }
}
