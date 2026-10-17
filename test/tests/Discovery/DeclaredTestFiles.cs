namespace Specht.Tests.Discovery;

/// <summary>The discovery store reuses its glob and directory exclusion contract for declared tests.</summary>
public sealed class DeclaredTestFiles : TheoryData<string[], string[], string[]>
{
    public DeclaredTestFiles()
    {
        Add(["test/**/*.cs"], [], ["test/a.cs", "test/nested/b.cs", "test/obj/c.cs"]);
        Add(["test/**/*.cs", "test/a.cs"], ["obj"], ["test/a.cs", "test/nested/b.cs"]);
        Add(["test/*.cs"], [], ["test/a.cs"]);
        Add(["**/*.cs"], ["/test/nested", "obj"], ["other/d.cs", "test/a.cs"]);
        Add(["test/**/*.fs"], [], []);
    }
}
