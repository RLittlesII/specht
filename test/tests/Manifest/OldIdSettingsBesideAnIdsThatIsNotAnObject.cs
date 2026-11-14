namespace Specht.Tests.Manifest;

/// <summary>
/// A manifest pinned to schema <c>0.4.0</c> that holds a customised claim grammar and a <c>mint</c> entry beside an <c>ids</c>
/// that is not an object, which the move leaves for the parse to reject (<c>0001-F7</c> B-020; <c>0001-F5</c> B-083): a list,
/// text, a number and <c>null</c>.
/// </summary>
public sealed class OldIdSettingsBesideAnIdsThatIsNotAnObject : TheoryData<string>
{
    public OldIdSettingsBesideAnIdsThatIsNotAnObject()
    {
        Add("""{"schemaVersion":"0.4.0","ids":[],"identifiers":{"claim":"^B-[0-9]{4}$"},"mint":{"claim":{"strategy":"nanoid"}}}""");
        Add("""{"schemaVersion":"0.4.0","ids":"claim","identifiers":{"claim":"^B-[0-9]{4}$"},"mint":{"claim":{"strategy":"nanoid"}}}""");
        Add("""{"schemaVersion":"0.4.0","ids":5,"identifiers":{"claim":"^B-[0-9]{4}$"},"mint":{"claim":{"strategy":"nanoid"}}}""");
        Add("""{"schemaVersion":"0.4.0","ids":null,"identifiers":{"claim":"^B-[0-9]{4}$"},"mint":{"claim":{"strategy":"nanoid"}}}""");
    }
}
