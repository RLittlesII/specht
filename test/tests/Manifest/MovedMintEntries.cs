namespace Specht.Tests.Manifest;

/// <summary>
/// A <c>mint.claim</c> entry as the manifest's JSON writes it, and the <c>mint</c> its <c>ids</c> record gets
/// (<c>0001-F7</c> B-156): an object less its <c>prefix</c>, an object without one, and entries that are not an object - text,
/// a number, <c>null</c> and a list - as they are.
/// </summary>
public sealed class MovedMintEntries : TheoryData<string, string>
{
    public MovedMintEntries()
    {
        Add("""{"strategy":"nanoid","nanoid":{"size":9},"prefix":"B-"}""", """{"strategy":"nanoid","nanoid":{"size":9}}""");
        Add("""{"strategy":"snowflake","attempts":3}""", """{"strategy":"snowflake","attempts":3}""");
        Add("\"nanoid\"", "\"nanoid\"");
        Add("9", "9");
        Add("null", "null");
        Add("""["nanoid"]""", """["nanoid"]""");
    }
}
