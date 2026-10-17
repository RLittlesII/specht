namespace Specht.Tests.Rules.Matrix;

/// <summary>The inline-code and delimiter boundaries of a matrix Test cell (B-003).</summary>
public sealed class MatrixNameCells : TheoryData<string, string[]>
{
    public MatrixNameCells()
    {
        Add("`Alpha`, `Beta`", ["Alpha", "Beta"]);
        Add("`Alpha; Beta`", ["Alpha", "Beta"]);
        Add("` Alpha, ; Beta; `", ["Alpha", "Beta"]);
        Add("`Alpha` and the Gamma one", ["Alpha"]);
        Add("Alpha and Beta", []);
        Add(@"\`Alpha\`", []);
        Add("``Alpha`Beta``", ["Alpha`Beta"]);
        Add("`Alpha` `Alpha`", ["Alpha", "Alpha"]);
    }
}
