namespace Specht.Tests.Identifiers;

/// <summary>
/// A type's <c>ids</c> record under schema <c>0.4.0</c>, the frontmatter of <c>features/one</c>'s specification, the Feature a
/// per-Feature type is minted for, and the id a Nano ID draw of <c>aa</c> is minted as (<c>0001-F19</c> A-9; item
/// <c>0437</c>): the record's form with the Feature's claim prefix (B-063), the record's plain text outside a prefixed Feature
/// (B-065), a record with no form in a prefixed Feature (B-066), every type in a Feature whose prefix <c>0001-F5</c> B-078 or
/// B-100 reports (B-067), and a size of 9 after the head (B-072). Each record opts its type in with the alphabet <c>ab</c> and an
/// <c>id</c> that admits the draw, and leaves every member it does not name to the shipped record.
/// </summary>
public sealed class RecordHeads : TheoryData<string, string, string, string?, string>
{
    /// <summary>Initializes the B-063, B-065, B-066, B-067 and B-072 rows.</summary>
    public RecordHeads()
    {
        Add("{ \"form\": \"{prefix}-{id}\", " + OptIn + " }", "claim", Activity, Feature, "ACTIVITY-aa");
        Add("{ \"form\": \"{prefix}-SCENARIO-{id}\", " + OptIn + " }", "scenario", Activity, Feature, "ACTIVITY-SCENARIO-aa");
        Add("{ \"form\": \"{prefix}-CONSTRAINT-{id}\", " + OptIn + " }", "constraint", Activity, Feature, "ACTIVITY-CONSTRAINT-aa");
        Add("{ \"form\": \"{prefix}-Q-{id}\", " + OptIn + " }", "openQuestion", Activity, Feature, "ACTIVITY-Q-aa");
        Add("{ \"form\": \"{prefix}-D-{id}\", " + OptIn + " }", "decision", Activity, Feature, "ACTIVITY-D-aa");
        Add("{ " + OptIn + " }", "claim", Activity, Feature, "ACTIVITYaa");
        Add("{ \"plain\": \"REQ-{id}\", " + OptIn + " }", "claim", string.Empty, Feature, "REQ-aa");
        Add("{ \"plain\": \"ADR-{id}\", " + OptIn + " }", "adr", Activity, null, "ADR-aa");
        Add("{ \"plain\": \"{id}\", " + OptIn + " }", "lesson", Activity, null, "aa");
        Add("{ " + OptIn + " }", "decision", Activity, Feature, "aa");
        Add("{ " + OptIn + " }", "constraint", Activity, Feature, "C-aa");
        Add("{ " + OptIn + " }", "openQuestion", Activity, Feature, "OQ-aa");
        Add("{ " + OptIn + " }", "scenario", Activity, Feature, "S-aa");
        Add("{ " + OptIn + " }", "claim", WhiteSpace, Feature, "B-aa");
        Add("{ " + OptIn + " }", "claim", ScenarioHead, Feature, "B-aa");
        Add("{ \"form\": \"{prefix}-C-{id}\", " + OptIn + " }", "constraint", WhiteSpace, Feature, "C-aa");
        Add("{ \"form\": \"{prefix}-C-{id}\", " + OptIn + " }", "constraint", ScenarioHead, Feature, "C-aa");
        Add(
            """{ "form": "{prefix}-{id}", "id": "[ab]{9}", "mint": { "strategy": "nanoid", "nanoid": { "alphabet": "ab", "size": 9 } } }""",
            "claim",
            Activity,
            Feature,
            "ACTIVITY-aaaaaaaaa");
    }

    private const string OptIn = "\"id\": \"[ab]+\", \"mint\": { \"strategy\": \"nanoid\", \"nanoid\": { \"alphabet\": \"ab\", \"size\": 2 } }";

    private const string Feature = "features/one";

    private const string Activity = "---\nclaim_prefix: \"ACTIVITY\"\n---\n";

    private const string WhiteSpace = "---\nclaim_prefix: \" \"\n---\n";

    private const string ScenarioHead = "---\nclaim_prefix: \"S-\"\n---\n";
}
