namespace Specht.Tests.Model;

/// <summary>
/// Whether a schema 0.3.0 manifest declares the epic grammar, where a Feature sits, its frontmatter, and the identity it
/// takes: <c>{epic}-{id}</c> with the grammar (<c>0001-F5</c> B-024), the identity form rendered with an empty epic for a
/// Feature with no <c>epic</c> under it (B-069), and the <c>id</c> alone without it, whatever the frontmatter or the
/// folder says of an epic (B-025; decision 0020).
/// </summary>
public sealed class EpicsOptInIdentities : TheoryData<bool, string, string, string>
{
    public EpicsOptInIdentities()
    {
        Add(true, "src/area/.spec/README.md", "---\nepic: '0001'\nid: F2\n---\n", "0001-F2");
        Add(true, "src/area/.spec/README.md", "---\nid: F2\n---\n", "-F2");
        Add(false, "src/area/.spec/README.md", "---\nid: F2\n---\n", "F2");
        Add(false, "src/area/.spec/README.md", "---\nepic: '0001'\nid: F2\n---\n", "F2");
        Add(false, "epics/0001-epic/F2-feature/spec.md", "---\nid: F2\n---\n", "F2");
    }
}
