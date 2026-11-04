namespace Specht.Tests.Mcp;

/// <summary>Paths given as a root that are not a directory: one that does not exist, and one that is a file, each with whether the test writes it.</summary>
public sealed class NonDirectoryRoots : TheoryData<string, bool>
{
    public NonDirectoryRoots()
    {
        Add("no-such-directory", false);
        Add("a-file.txt", true);
    }
}
