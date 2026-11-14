namespace Specht.Tests.Rules.Matrix;

/// <summary>
/// Scenario cells with title matching on, off and absent, against one companion's titles and tags, under any Status (B-008 to
/// B-010, B-025); a cell that names no title is not reported (§ 5 row 12).
/// </summary>
public sealed class ScenarioTitleCells : TheoryData<string?, string, string, string[]>
{
    public ScenarioTitleCells()
    {
        const string on = """{"scenarioTitles":true}""";
        const string off = """{"scenarioTitles":false}""";
        const string partial = "Partial";
        Add(on, "First case", partial, []);
        Add(on, "Second case", partial, []);
        Add(on, "First case; Second case", partial, []);
        Add(on, "First case; A title none has", partial, ["A title none has"]);
        Add(on, "A title no scenario has", partial, ["A title no scenario has"]);
        Add(on, "First", partial, ["First"]);
        Add(on, "A title none has; Another title none has", partial, ["A title none has", "Another title none has"]);
        Add(on, "Setting a level succeeds", partial, ["Setting a level succeeds"]);
        Add(on, "First case; Setting a level succeeds", partial, ["Setting a level succeeds"]);
        Add(on, "", partial, []);
        Add(on, " ; ; ", partial, []);
        Add(null, "A title no scenario has", partial, []);
        Add(off, "A title no scenario has", partial, []);
        Add(off, "Setting a level succeeds", partial, []);
        Add(on, "A title no scenario has", "Missing", ["A title no scenario has"]);
        Add(on, "A title no scenario has", "Waived", ["A title no scenario has"]);
        Add(on, "A title no scenario has", "Withdrawn", ["A title no scenario has"]);
        Add(on, "A title no scenario has", "Superseded", ["A title no scenario has"]);
    }
}
