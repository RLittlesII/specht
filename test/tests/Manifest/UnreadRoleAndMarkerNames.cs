namespace Specht.Tests.Manifest;

/// <summary>
/// A pin, <c>roles</c> or <c>markers</c>, a name the pinned version does not read there, and a text that would be rejected
/// were the name read: a title the sections lack (B-015) or the empty text (B-040) (<c>0001-F5</c> B-072; decision 0021
/// call 1).
/// </summary>
public sealed class UnreadRoleAndMarkerNames : TheoryData<string, string, string, string>
{
    public UnreadRoleAndMarkerNames()
    {
        foreach (var pin in new[] { "0.1.0", "0.2.0", "0.2.1", "0.3.0" })
        {
            Add(pin, "roles", "constraints", "Glossary");
            Add(pin, "markers", "pending", string.Empty);
        }

        Add("0.1.0", "markers", "done", string.Empty);
    }
}
