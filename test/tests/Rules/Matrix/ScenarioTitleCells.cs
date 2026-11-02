namespace Specht.Tests.Rules.Matrix;

/// <summary>Scenario cells with title matching on, off and left out, against one companion's titles and tags (B-008 to B-010).</summary>
public sealed class ScenarioTitleCells : TheoryData<string?, string, string[]>
{
    public ScenarioTitleCells()
    {
        const string on = """{"scenarioTitles":true}""";
        const string off = """{"scenarioTitles":false}""";
        Add(on, "First case", []);
        Add(on, "Second case", []);
        Add(on, "First case; Second case", []);
        Add(on, "First case; A title none has", ["A title none has"]);
        Add(on, "A title no scenario has", ["A title no scenario has"]);
        Add(on, "First", ["First"]);
        Add(on, "A title none has; Another title none has", ["A title none has", "Another title none has"]);
        Add(on, "Setting a level succeeds", ["Setting a level succeeds"]);
        Add(on, "First case; Setting a level succeeds", ["Setting a level succeeds"]);
        Add(null, "A title no scenario has", []);
        Add(off, "A title no scenario has", []);
        Add(off, "Setting a level succeeds", []);
    }
}
