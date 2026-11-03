namespace Specht.Tests.Manifest;

/// <summary>
/// A task grammar that compiles and has no named <c>sequence</c> group, and whether the manifest declares the epic
/// grammar: the grammar the default manifests of earlier versions declare, with and without the epic grammar, and one
/// naming only its epic group (<c>0001-F5</c> B-050).
/// </summary>
public sealed class GrouplessTaskGrammars : TheoryData<string, bool>
{
    public GrouplessTaskGrammars()
    {
        Add("^[0-9]{4}-[0-9]{2}$", false);
        Add("^[0-9]{4}-[0-9]{2}$", true);
        Add("^(?<epic>[0-9]{4})-[0-9]{2}$", true);
    }
}
