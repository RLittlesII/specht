namespace Specht.Tests.Rules;

/// <summary>
/// Every message that names a frontmatter key, under a schema 0.3.0 manifest that renames the key under <c>keys</c>
/// (<c>0001-F5</c> B-009, B-004): the rule id, the <c>keys</c> entry, the Feature at <c>src/area/.spec/README.md</c>, the
/// file written with the new name beside it, the key and value the one finding names and the default key it no longer
/// names. SPEC043 reads the item keys, SPEC011 the Feature keys of a specification under the epics layout. The Feature
/// beside it carries its epic under the key the row names, so under the epic grammar it is no missing-epic finding
/// (<c>0001-F5</c> B-035).
/// </summary>
public sealed class RenamedKeyMessages : TheoryData<string, string, string, string, string, string, string>
{
    public RenamedKeyMessages()
    {
        Add(
            "SPEC043",
            """{"task":{"id":"work_id"}}""",
            "---\nepic: '0001'\nid: F1\n---\n",
            "src/area/.spec/0001-01-item.md",
            "---\nwork_id: '0001-02'\nparent: 0001-F1\ntype: task\n---\n",
            "frontmatter work_id '0001-02'",
            "frontmatter id ");
        Add(
            "SPEC043",
            """{"task":{"parent":"owner"}}""",
            "---\nepic: '0001'\nid: F1\n---\n",
            "src/area/.spec/0001-01-item.md",
            "---\nid: '0001-01'\nowner: 0001-F9\ntype: task\n---\n",
            "frontmatter owner '0001-F9'",
            "frontmatter parent ");
        Add(
            "SPEC011",
            """{"feature":{"epic":"initiative"}}""",
            "---\ninitiative: '0001'\nid: F1\n---\n",
            "epics/0001-epic/F1-feature/spec.md",
            "---\ninitiative: '0002'\nid: F1\n---\n",
            "frontmatter initiative '0002'",
            "frontmatter epic ");
        Add(
            "SPEC011",
            """{"feature":{"epic":"initiative"}}""",
            "---\ninitiative: '0001'\nid: F1\n---\n",
            "epics/0001-epic/F2-feature/spec.md",
            "---\nid: F2\n---\n",
            "frontmatter has no initiative - the manifest declares the epic grammar",
            "frontmatter has no epic ");
        Add(
            "SPEC011",
            """{"feature":{"id":"feature_id"}}""",
            "---\nepic: '0001'\nid: F1\n---\n",
            "epics/0001-epic/F1-feature/spec.md",
            "---\nepic: '0001'\nfeature_id: F2\n---\n",
            "frontmatter feature_id 'F2'",
            "frontmatter id ");
    }
}
