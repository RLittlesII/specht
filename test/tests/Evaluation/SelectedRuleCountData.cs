namespace Specht.Tests.Evaluation;

/// <summary>A vocabulary, the id of each rule given, and how many rules count as evaluated (<c>0001-F1</c> B-004; <c>0001-F7</c> B-014).</summary>
public sealed class SelectedRuleCountData : TheoryData<string, string[], string[], int>
{
    /// <summary>Initializes the empty, in-vocabulary, outside-vocabulary, shared-id and unused-id cases.</summary>
    public SelectedRuleCountData()
    {
        Add("no rule evaluates nothing", ["FAKE001"], [], 0);
        Add("every rule's id is in the vocabulary", ["FAKE001", "FAKE002", "FAKE003"], ["FAKE001", "FAKE002", "FAKE003"], 3);
        Add("a rule whose id is outside the vocabulary is not counted", ["FAKE001"], ["FAKE001", "FAKE002"], 1);
        Add("rules are counted, not distinct ids, so two rules under one id count twice", ["FAKE001"], ["FAKE001", "FAKE001"], 2);
        Add("a vocabulary id no rule has is not counted", ["FAKE001", "FAKE002"], ["FAKE001"], 1);
    }
}
