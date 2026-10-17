namespace Specht.Tests.Rules.Matrix;

/// <summary>Whole words, dotted qualifiers and literal declaration forms (B-004, B-005).</summary>
public sealed class MatrixNameMatches : TheoryData<string, string, string[], int?>
{
    public MatrixNameMatches()
    {
        Add("Alpha", "// preface\nvoid Alpha() {}", [], 2);
        Add("Alpha", "// preface\r\nvoid Alpha() {}", [], 2);
        Add("Alpha", "// preface\rvoid Alpha() {}", [], 2);
        Add("Alpha", "AlphaCase", [], null);
        Add("Alpha", "BeforeAlpha", [], null);
        Add("Alpha", "Alpha_other", [], null);
        Add("Alpha", "éAlpha", [], null);
        Add("Alpha", "alpha", [], null);
        Add("Steps.Alpha", "class Steps\nvoid Alpha() {}", [], 2);
        Add("Steps.Alpha", "class OtherSteps\nvoid Alpha() {}", [], null);
        Add("Suite.Steps.Alpha", "namespace Suite\nclass Steps\nvoid Alpha() {}", [], 3);
        Add("Suite.Steps.Alpha", "class Steps\nvoid Alpha() {}", [], null);
        Add("Alpha", "// Alpha\nlet Alpha ", ["let {test} "], 2);
        Add("Alpha", "// Alpha", ["let {test} "], null);
        Add("Alpha", "let AlphaCase ", ["let {test} "], null);
        Add("Alpha", "[Alpha]", ["[{test}]"], 1);
        Add("Alpha", "xAlpha", ["x{test}"], 1);
        Add("Alpha", "let Alpha ", ["void {test}(", "let {test} "], 1);
        Add("Steps.Alpha", "Steps\nlet Alpha ", ["let {test} "], 2);
        Add("Steps.Alpha", "Steps\n// Alpha", ["let {test} "], null);
        Add("Alpha", "let \nAlpha ", ["let \n{test} "], 2);
    }
}
