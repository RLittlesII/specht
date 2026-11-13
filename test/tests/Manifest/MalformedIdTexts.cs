namespace Specht.Tests.Manifest;

/// <summary>
/// The members of a schema 0.4.0 manifest with a <c>plain</c> or <c>form</c> <c>0001-F5</c> B-084 rejects, the type and
/// the text the rejection names: a <c>form</c> without one <c>{prefix}</c>, with another brace, or not ending with its
/// only <c>{id}</c>; a <c>plain</c> not ending with its only <c>{id}</c> or holding another brace; either not text; and a
/// <c>form</c> in the <c>adr</c> or <c>lesson</c> record.
/// </summary>
public sealed class MalformedIdTexts : TheoryData<string, string, string>
{
    /// <summary>Initializes the rows.</summary>
    public MalformedIdTexts()
    {
        Add("""{ "ids": { "claim": { "form": "ACTIVITY-{id}" } } }""", "claim", "ACTIVITY-{id}");
        Add("""{ "ids": { "claim": { "form": "{prefix}-{prefix}-{id}" } } }""", "claim", "{prefix}-{prefix}-{id}");
        Add("""{ "ids": { "claim": { "form": "{id}-{prefix}" } } }""", "claim", "{id}-{prefix}");
        Add("""{ "ids": { "claim": { "form": "{prefix}-{id}-x" } } }""", "claim", "{prefix}-{id}-x");
        Add("""{ "ids": { "claim": { "form": "{prefix}-{seq}-{id}" } } }""", "claim", "{prefix}-{seq}-{id}");
        Add("""{ "ids": { "claim": { "form": 7 } } }""", "claim", "7");
        Add("""{ "ids": { "claim": { "plain": "{prefix}-{id}" } } }""", "claim", "{prefix}-{id}");
        Add("""{ "ids": { "claim": { "plain": "B-{id}-{id}" } } }""", "claim", "B-{id}-{id}");
        Add("""{ "ids": { "claim": { "plain": "B-" } } }""", "claim", "B-");
        Add("""{ "ids": { "constraint": { "plain": 7 } } }""", "constraint", "7");
        Add("""{ "ids": { "adr": { "form": "{prefix}-{id}" } } }""", "adr", "{prefix}-{id}");
        Add("""{ "ids": { "lesson": { "form": "{prefix}-{id}" } } }""", "lesson", "{prefix}-{id}");
    }
}
