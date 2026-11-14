namespace Specht.Tests.Model;

/// <summary>
/// A specification's text and the list items the parse gives its <c>10. Lessons / Spec Deltas</c> section, each with its
/// one-based first line (<c>0132-F2</c> B-009, C-1): every item of every list among the section's own blocks, a list after
/// a <c>###</c> heading included and a list in a quote or the next <c>##</c> section not; an item's paragraphs, a nested
/// item's included, flattened as a cell is, each line break written as one space and the paragraphs joined by one space; a
/// link giving its text and a code block giving none.
/// </summary>
public sealed class SectionListItems : TheoryData<string, string[], int[]>
{
    public SectionListItems()
    {
        Add("## 10. Lessons / Spec Deltas\n\n- First lesson.\n- Second lesson.\n", ["First lesson.", "Second lesson."], [3, 4]);
        Add("## 10. Lessons / Spec Deltas\n\n1. First lesson.\n2. Second lesson.\n", ["First lesson.", "Second lesson."], [3, 4]);
        Add("## 10. Lessons / Spec Deltas\n\nIntro.\n\n### Round 1\n\n- After the heading.\n", ["After the heading."], [7]);
        Add("## 10. Lessons / Spec Deltas\n\n- A lesson that\n  wraps onto a second line.\n", ["A lesson that wraps onto a second line."], [3]);
        Add("## 10. Lessons / Spec Deltas\n\n- First paragraph.\n\n  Second paragraph.\n", ["First paragraph. Second paragraph."], [3]);
        Add("## 10. Lessons / Spec Deltas\n\n- Parent.\n  - Child.\n- Next.\n", ["Parent. Child.", "Next."], [3, 5]);
        Add("## 10. Lessons / Spec Deltas\n\n> - Quoted.\n\n- Own.\n", ["Own."], [5]);
        Add(
            "## 10. Lessons / Spec Deltas\n\n- See `B-001` and **the** [lesson 0001](lessons/0001-a-lesson.md).\n",
            ["See B-001 and the lesson 0001."],
            [3]);
        Add("## 10. Lessons / Spec Deltas\n\n- Run the build.\n\n  ```sh\n  ./build.sh\n  ```\n", ["Run the build."], [3]);
        Add("## 10. Lessons / Spec Deltas\n\n- One.\n\nProse.\n\n- Two.\n", ["One.", "Two."], [3, 7]);
        Add("## 10. Lessons / Spec Deltas\n\n- Mine.\n\n## 11. Open Questions\n\n- Not mine.\n", ["Mine."], [3]);
        Add("## 10. Lessons / Spec Deltas\n\n| Trigger | Change |\n| ------- | ------ |\n| A | B |\n", [], []);
        Add("## 10. Lessons / Spec Deltas\n\nNone.\n", [], []);
    }
}
