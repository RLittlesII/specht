namespace Specht.Tests.Trace;

/// <summary>
/// The broken roots of the <c>@B-006</c> outline, each with the exit code the check gives it (<c>0001-F13</c> B-006):
/// whether the root exists, the manifest text or none, and the code.
/// </summary>
public sealed class TraceBrokenRoots : TheoryData<bool, string?, int>
{
    public TraceBrokenRoots()
    {
        Add(false, null, 2);
        Add(true, null, 2);
        Add(true, "{", 3);
        Add(true, """{"schemaVersion":"7.0.0"}""", 3);
    }
}
