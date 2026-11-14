namespace Specht.Tests.Manifest;

/// <summary>
/// A manifest pinned to schema <c>0.4.0</c> that still holds the id settings <c>0001-F5</c> B-092 rejects and nothing else the
/// check rejects (<c>0001-F7</c> B-051): a <c>mint</c> entry, a customised grammar, a grammar the move would refuse, and a
/// <c>mint</c> entry that is not an object.
/// </summary>
public sealed class ZeroFourZeroOldIdSettings : TheoryData<string>
{
    public ZeroFourZeroOldIdSettings()
    {
        Add("""{"schemaVersion":"0.4.0","mint":{"claim":{"strategy":"nanoid"}}}""");
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"^B-[0-9]{4}$"}}""");
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"B-[0-9]{3}"}}""");
        Add("""{"schemaVersion":"0.4.0","mint":{"claim":"nanoid"}}""");
    }
}
