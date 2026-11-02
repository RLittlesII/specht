using Specht.Rules.Matrix;

namespace Specht.Tests.Rules.Matrix;

/// <summary>Scenario cells resolved against one companion, with the expected title, status and keyword line of each (B-008, B-009).</summary>
public sealed class ScenarioTitleResolutions : TheoryData<string?, bool, string, string, string[], MatrixScenarioStatus[], int?[]>
{
    public ScenarioTitleResolutions()
    {
        const string on = """{"scenarioTitles":true}""";
        const string off = """{"scenarioTitles":false}""";
        const MatrixScenarioStatus resolved = MatrixScenarioStatus.Resolved;
        const MatrixScenarioStatus untagged = MatrixScenarioStatus.Untagged;
        const MatrixScenarioStatus none = MatrixScenarioStatus.NoSuchTitle;
        const MatrixScenarioStatus prose = MatrixScenarioStatus.Unconfigured;
        Add(on, false, "B-002", "First case", ["First case"], [resolved], [5]);
        Add(on, false, "B-003", "Second case", ["Second case"], [resolved], [9]);
        Add(on, false, "B-001", "First case; Second case", ["First case", "Second case"], [resolved, resolved], [5, 9]);
        Add(on, false, "B-004", "Second case", ["Second case"], [resolved], [9]);
        Add(on, false, "B-002", "Second case", ["Second case"], [untagged], [9]);
        Add(on, false, "B-002", " First case ;; Second case ; ", ["First case", "Second case"], [resolved, untagged], [5, 9]);
        Add(on, false, "B-002", "first case", ["first case"], [none], [null]);
        Add(on, false, "B-002", "First", ["First"], [none], [null]);
        Add(on, false, "B-005", "An example", ["An example"], [none], [null]);
        Add(on, false, "B-007", "Twin", ["Twin"], [resolved], [26]);
        Add(on, false, "B-002", "Twin", ["Twin"], [untagged], [22]);
        Add(on, false, "b-002", "First case", ["First case"], [untagged], [5]);
        Add(on, true, "b-002", "First case", ["First case"], [resolved], [5]);
        Add(off, false, "B-002", "First case; A title none has", ["First case", "A title none has"], [prose, prose], [null, null]);
        Add(null, false, "B-002", "A title none has", ["A title none has"], [prose], [null]);
    }
}
