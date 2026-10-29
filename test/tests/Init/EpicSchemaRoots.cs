namespace Specht.Tests.Init;

/// <summary>
/// A root's existing manifest, or none, with the <c>--epics</c> choice and whether the epic frontmatter schema is written,
/// for <c>0001-F4</c> B-018, B-019, B-023 and C-5, C-6.
/// </summary>
public sealed class EpicSchemaRoots : TheoryData<string, string?, bool, bool>
{
    public EpicSchemaRoots()
    {
        Add("a bare root with epics", null, true, true);
        Add("a bare root without epics", null, false, false);
        Add("a manifest declaring no epic grammar, without epics", NoEpicGrammar, false, false);
        Add("a manifest declaring no epic grammar, with epics", NoEpicGrammar, true, false);
        Add("a manifest declaring the epic grammar, without epics", EpicGrammar, false, true);
        Add("a manifest declaring the epic grammar, with epics", EpicGrammar, true, true);
    }

    private const string NoEpicGrammar = """{ "identifiers": { "task": "^[0-9]{4}-[0-9]{2}$" } }""";

    private const string EpicGrammar = """{ "identifiers": { "task": "^[0-9]{4}-[0-9]{2}$", "epic": "^[0-9]{4}$" } }""";
}
