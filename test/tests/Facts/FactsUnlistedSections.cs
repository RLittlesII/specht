namespace Specht.Tests.Facts;

/// <summary>
/// Each section read under the title the manifest's <c>sections</c> lists, and the member it fills, which is empty when
/// <c>sections</c> does not list that title, so that no entry carries its leading number (<c>0132-F2</c> B-015, B-016, C-7,
/// decision 0003).
/// </summary>
public sealed class FactsUnlistedSections : TheoryData<string, string>
{
    public FactsUnlistedSections()
    {
        Add("4. Constraints", "constraints");
        Add("5. Out of Scope", "outOfScope");
        Add("10. Lessons / Spec Deltas", "deltas");
        Add("11. Open Questions", "openQuestions");
    }
}
