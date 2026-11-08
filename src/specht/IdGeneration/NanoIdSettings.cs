namespace Specht.IdGeneration;

/// <summary>Nano ID's own settings, read from a type's <c>nanoid</c> sub-object alone (<c>0001-F19</c> C-3, C-12).</summary>
/// <param name="Alphabet">The characters an id draws from.</param>
/// <param name="Size">The number of characters an id holds.</param>
/// <implements epic="0001" feature="F19" claim="B-005" />
/// <implements epic="0001" feature="F19" claim="B-006" />
public sealed record NanoIdSettings(string Alphabet, int Size)
{
    /// <summary>The alphabet of a type that names none: lower-case base32, with no <c>i</c>, <c>l</c>, <c>o</c> or <c>u</c> (B-019).</summary>
    public const string Base32 = "0123456789abcdefghjkmnpqrstvwxyz";
}
