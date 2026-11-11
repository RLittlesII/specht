namespace Specht.Tests.Identifiers;

/// <summary>
/// A <c>mint</c> member with no grammar in <c>identifiers</c> that opts no C-13 type in (<c>0001-F19</c> B-003, B-043, B-002),
/// as a JSON object holding that one member: <c>foo</c>, which names none of C-13's types, and <c>lesson</c>, whose entry names
/// no strategy. Neither is a type B-043's check reaches, so a request for an opted-in <c>adr</c> beside it mints its id. An
/// <c>epic</c> entry naming no strategy and a <c>task</c> member that is a number opt nothing in either, so B-061's check does
/// not reach them (C-2; item <c>0421</c>).
/// </summary>
public sealed class MintMembersOptingNoTypeIn : TheoryData<string>
{
    /// <summary>Initializes the no-type, no-strategy and non-object rows.</summary>
    public MintMembersOptingNoTypeIn()
    {
        Add("""{ "foo": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }""");
        Add("""{ "lesson": { "nanoid": { "alphabet": "z", "size": 3 } } }""");
        Add("""{ "epic": { "nanoid": { "alphabet": "z", "size": 3 } } }""");
        Add("""{ "task": 7 }""");
    }
}
