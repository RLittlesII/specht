namespace Specht.Tests.Model;

/// <summary>
/// One frontmatter key role a schema 0.3.0 manifest renames under <c>keys</c>, a Feature at
/// <c>src/area/.spec/README.md</c> and an item at <c>src/area/.spec/0001-01-item.md</c> written with the new name, and
/// the finding the rule reading that role gives: its rule id, identifier, file and the line of the renamed key
/// (<c>0001-F5</c> B-004; decisions 0010 and 0017). Each row names an edge, child or spike that does not resolve, or a
/// value a rule reports, so a finding proves the role was read by the manifest's name.
/// </summary>
public sealed class RenamedFrontmatterKeys : TheoryData<string, string, string, string, string, string, int>
{
    public RenamedFrontmatterKeys()
    {
        Add("""{"feature":{"children":"tasks"}}""", "---\nepic: '0001'\nid: F1\ntasks: ['0001-09']\n---\n", Item, "SPEC5001", "0001-09", Feature, 4);
        Add(
            """{"feature":{"spikes":"investigations"}}""",
            "---\nepic: '0001'\nid: F1\ninvestigations: ['0001-09']\n---\n",
            Item,
            "SPEC5002",
            "0001-09",
            Feature,
            4);
        Add("""{"feature":{"dependsOn":"requires"}}""", "---\nepic: '0001'\nid: F1\nrequires: ['F9']\n---\n", Item, "SPEC6001", "0001-F9", Feature, 4);
        Add("""{"feature":{"blocks":"unblocks"}}""", "---\nepic: '0001'\nid: F1\nunblocks: ['F9']\n---\n", Item, "SPEC6001", "0001-F9", Feature, 4);
        Add("""{"feature":{"id":"feature_id"}}""", "---\nepic: '0001'\nfeature_id: F1\ndepends_on: ['F9']\n---\n", Item, "SPEC6001", "0001-F9", Feature, 4);
        Add("""{"feature":{"epic":"initiative"}}""", "---\ninitiative: '0001'\nid: F1\ndepends_on: ['F9']\n---\n", Item, "SPEC6001", "0001-F9", Feature, 4);
        Add("""{"feature":{"status":"delivery"}}""", "---\nepic: '0001'\nid: F1\ndelivery: done\n---\n\n" + Matrix, Item, "SPEC7001", "B-001", Feature, 11);
        Add(
            """{"feature":{"specStatus":"maturity"}}""",
            "---\nepic: '0001'\nid: F1\nmaturity: approved\n---\n\n" + SignOff,
            Item,
            "SPEC7002",
            "1-5",
            Feature,
            11);
        Add("""{"task":{"id":"work_id"}}""", Plain, "---\nwork_id: '0001-02'\nparent: 0001-F1\ntype: task\n---\n", "SPEC5003", "0001-02", ItemPath, 2);
        Add("""{"task":{"parent":"owner"}}""", Plain, "---\nid: '0001-01'\nowner: 0001-F9\ntype: task\n---\n", "SPEC5003", "0001-01", ItemPath, 3);
        Add(
            """{"task":{"type":"kind"}}""",
            "---\nepic: '0001'\nid: F1\nspikes: ['0001-01']\n---\n",
            "---\nid: '0001-01'\nparent: 0001-F1\nkind: task\n---\n",
            "SPEC5002",
            "0001-01",
            ItemPath,
            4);
    }

    private const string Feature = "src/area/.spec/README.md";

    private const string ItemPath = "src/area/.spec/0001-01-item.md";

    private const string Plain = "---\nepic: '0001'\nid: F1\n---\n";

    private const string Item = "---\nid: '0001-01'\nparent: 0001-F1\ntype: task\n---\n";

    private const string Matrix = "## 9. Traceability Matrix\n\n| Claim ID | Test |\n| -------- | ---- |\n| B-001 | Missing |\n";

    private const string SignOff = "## 12. Sign-off\n\n| Section | Status |\n| ------- | ------ |\n| 1-5 | \U0001F7E1 |\n";
}
