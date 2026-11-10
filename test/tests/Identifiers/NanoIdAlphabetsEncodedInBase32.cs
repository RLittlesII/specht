namespace Specht.Tests.Identifiers;

/// <summary>
/// Nano ID alphabets a type opted in with the encoding <c>base32</c> may draw from (<c>0001-F19</c> B-033, C-15): lower-case
/// base32 itself, and the decimal digits. B-033 refuses the encoding whatever the alphabet.
/// </summary>
public sealed class NanoIdAlphabetsEncodedInBase32 : TheoryData<string>
{
    /// <summary>Initializes the base32 and decimal rows.</summary>
    public NanoIdAlphabetsEncodedInBase32()
    {
        Add("0123456789abcdefghjkmnpqrstvwxyz");
        Add("0123456789");
    }
}
