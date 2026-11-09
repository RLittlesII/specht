namespace Specht.Tests.Discovery;

/// <summary>
/// A finder, the one exclusion its manifest declares, every directory of the walked tree that entry excludes, and the
/// root-relative paths the finder still finds (<c>0001-F6</c> B-006, C-1).
/// </summary>
public sealed class ExcludedDirectories : TheoryData<string, string, string[], string[]>
{
    public ExcludedDirectories()
    {
        Add(
            "specifications",
            "obj",
            ["obj", "epics/obj", "test/obj"],
            ["a/b/x/.spec/README.md", "a/c/.spec/README.md", "src/area/.spec/README.md"]);
        Add(
            "specifications",
            "/a/b",
            ["a/b"],
            ["a/c/.spec/README.md", "obj/one/two/three/.spec/README.md", "src/area/.spec/README.md"]);
        Add("epics", "obj", ["obj", "epics/obj", "test/obj"], ["epics/0001-example/epic.md"]);
        Add("test files", "obj", ["obj", "epics/obj", "test/obj"], ["test/a.cs"]);
    }
}
