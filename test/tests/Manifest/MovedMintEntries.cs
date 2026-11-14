namespace Specht.Tests.Manifest;

/// <summary>
/// A type, its <c>mint</c> entry as the manifest's JSON writes it, and the <c>mint</c> its <c>ids</c> record gets
/// (<c>0001-F7</c> B-156): for <c>claim</c>, an object less its <c>prefix</c>, an object without one, and entries that are not
/// an object - text, a number, <c>null</c> and a list - as they are; and for <c>epic</c>, a name outside the seven
/// <c>0001-F19</c> C-13 types, an object less its <c>prefix</c>, which the parse after the move then rejects as no id type
/// (<c>0001-F5</c> B-083).
/// </summary>
public sealed class MovedMintEntries : TheoryData<string, string, string>
{
    public MovedMintEntries()
    {
        Add("claim", """{"strategy":"nanoid","nanoid":{"size":9},"prefix":"B-"}""", """{"strategy":"nanoid","nanoid":{"size":9}}""");
        Add("claim", """{"strategy":"snowflake","attempts":3}""", """{"strategy":"snowflake","attempts":3}""");
        Add("claim", "\"nanoid\"", "\"nanoid\"");
        Add("claim", "9", "9");
        Add("claim", "null", "null");
        Add("claim", """["nanoid"]""", """["nanoid"]""");
        Add("epic", """{"strategy":"nanoid","prefix":"E-"}""", """{"strategy":"nanoid"}""");
    }
}
