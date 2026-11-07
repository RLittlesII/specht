namespace Specht.IdGeneration;

/// <summary>Nano ID's own settings: the alphabet and the size.</summary>
/// <param name="Alphabet">The characters an id draws from, each once.</param>
/// <param name="Size">The number of characters.</param>
public sealed record NanoIdSettings(string Alphabet, int Size)
{
    /// <summary>Lower-case Crockford base32: no <c>i</c>, <c>l</c>, <c>o</c> or <c>u</c>, no case to fold and no separator.</summary>
    public const string Base32 = "0123456789abcdefghjkmnpqrstvwxyz";

    /// <summary>The reference implementation's URL-safe alphabet, mixed case with <c>_</c> and <c>-</c>.</summary>
    public const string UrlSafe = "useandom-26T198340PX75pxJACKVERYMINDBUSHWOLF_GQZbfghjklqvwyzrict";

    /// <summary>Bits of entropy in one id.</summary>
    public double Bits => Size * Math.Log2(Alphabet.Length);

    /// <summary>Refuses settings no id can be drawn from.</summary>
    /// <exception cref="IdRefusedException">The alphabet repeats a character or is out of range, or the size is not positive.</exception>
    public void Validate()
    {
        if (Alphabet.Length is < 2 or > 256 || Alphabet.Distinct().Count() != Alphabet.Length)
        {
            throw new IdRefusedException("nanoid alphabet must hold 2 to 256 distinct characters");
        }

        if (Size < 1)
        {
            throw new IdRefusedException("nanoid size must be positive");
        }
    }
}
