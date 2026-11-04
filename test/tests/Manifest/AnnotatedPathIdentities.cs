namespace Specht.Tests.Manifest;

/// <summary>
/// A layout's <c>identity</c> holding a member whose name begins with <c>$</c>, as the manifest's JSON writes it, and the
/// epic and Feature segments it is read as without that member (<c>0001-F5</c> B-074; decision 0021 call 3).
/// </summary>
public sealed class AnnotatedPathIdentities : TheoryData<string, int?, int?>
{
    public AnnotatedPathIdentities()
    {
        Add("""{ "epic": 1, "feature": 2, "$comment": "x" }""", 1, 2);
        Add("""{ "$comment": "the Feature folder", "feature": 3 }""", null, 3);
        Add("""{ "epic": 0, "$id": 7, "feature": 1 }""", 0, 1);
    }
}
