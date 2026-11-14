namespace Specht.Tests.Manifest;

/// <summary>
/// A manifest pinned below schema <c>0.4.0</c> that holds a <c>mint</c> entry and a customised <c>identifiers</c> grammar for
/// a <c>0001-F19</c> C-13 type, which <c>specht upgrade</c> does not move (<c>0001-F7</c> B-160, § 5 row 34): pinned to
/// <c>0.3.1</c>, to <c>0.3.0</c>, and with no <c>schemaVersion</c>, which is <c>0.1.0</c>.
/// </summary>
public sealed class OldIdSettingsBelowZeroFourZero : TheoryData<string>
{
    public OldIdSettingsBelowZeroFourZero()
    {
        Add("""{"schemaVersion":"0.3.1","identifiers":{"claim":"^B-[0-9]{4}$"},"mint":{"claim":{"strategy":"nanoid","prefix":"B-"}}}""");
        Add("""{"schemaVersion":"0.3.0","identifiers":{"claim":"^B-[0-9]{4}$"},"mint":{"claim":{"strategy":"nanoid","prefix":"B-"}}}""");
        Add("""{"identifiers":{"claim":"^B-[0-9]{4}$"},"mint":{"claim":{"strategy":"nanoid","prefix":"B-"}}}""");
    }
}
