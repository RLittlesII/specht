namespace Specht.Tests.Model;

/// <summary>
/// The text of an epic file under a schema 0.3.0 manifest declaring no epic grammar: one whose frontmatter fails the epic
/// schema, and one with no frontmatter (<c>0001-F5</c> B-034; decision 0020 call 8).
/// </summary>
public sealed class UncheckedEpicFiles : TheoryData<string>
{
    public UncheckedEpicFiles()
    {
        Add("---\nid: '0001'\ntype: feature\n---\n# Epic\n");
        Add("# Epic\n");
    }
}
