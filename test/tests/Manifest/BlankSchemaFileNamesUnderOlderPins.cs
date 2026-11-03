namespace Specht.Tests.Manifest;

/// <summary>
/// Each shipped version below schema <c>0.3.0</c>, and the empty text or <c>.</c> as the Feature schema's file name,
/// which that pin does not reject when the manifest loads (<c>0001-F5</c> B-049, § 5 row 21; decision 0021 call 7).
/// </summary>
public sealed class BlankSchemaFileNamesUnderOlderPins : TheoryData<string, string>
{
    public BlankSchemaFileNamesUnderOlderPins()
    {
        foreach (var pin in new[] { "0.1.0", "0.2.0", "0.2.1" })
        {
            Add(pin, string.Empty);
            Add(pin, ".");
        }
    }
}
