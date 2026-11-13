namespace Specht.Tests.Facts;

/// <summary>
/// The four broken roots of the <c>@B-021</c> outline, each with the exit code the check gives it (<c>0132-F2</c> B-021):
/// whether the root exists, the manifest text or none, and the code.
/// </summary>
public sealed class FactsBrokenRoots : TheoryData<bool, string?, int>
{
    public FactsBrokenRoots()
    {
        Add(false, null, 2);
        Add(true, null, 2);
        Add(true, "{", 3);
        Add(true, """{"schemaVersion":"7.0.0"}""", 3);
    }
}
