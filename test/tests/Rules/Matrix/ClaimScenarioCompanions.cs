namespace Specht.Tests.Rules.Matrix;

/// <summary>
/// A claim id resolved against two companions, <c>a.feature</c> and <c>b.feature</c>, with the scenarios carrying its tag
/// (<c>0001-F13</c> B-001, B-003): whether tags compare ignoring case, the claim, and each scenario's title, companion and
/// keyword line, in companion order and then file order. A tag above the Feature, a Rule or an outline's Examples is carried
/// by the scenarios under it, and a scenario two of whose tags match is listed once.
/// </summary>
public sealed class ClaimScenarioCompanions : TheoryData<bool, string, string[], string[], int[]>
{
    public ClaimScenarioCompanions()
    {
        const string a = "src/sample/.spec/a.feature";
        const string b = "src/sample/.spec/b.feature";
        Add(false, "B-001", ["First case", "Twice tagged", "Second case"], [a, a, a], [5, 9, 16]);
        Add(false, "B-002", ["First case", "Twice tagged", "Third case"], [a, a, b], [5, 9, 4]);
        Add(false, "B-003", ["Second case"], [a], [16]);
        Add(false, "B-004", ["Second case"], [a], [16]);
        Add(false, "B-005", ["Second case"], [a], [16]);
        Add(false, "B-006", [], [], []);
        Add(false, "b-003", [], [], []);
        Add(true, "b-003", ["Second case"], [a], [16]);
        Add(true, "B-002", ["First case", "Twice tagged", "Third case"], [a, a, b], [5, 9, 4]);
    }
}
