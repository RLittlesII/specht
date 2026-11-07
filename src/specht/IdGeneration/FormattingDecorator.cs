using System.Globalization;
using System.Numerics;

namespace Specht.IdGeneration;

/// <summary>Formatting: re-encodes a numeric candidate and puts the type's prefix in front.</summary>
/// <param name="inner">The strategy or decorator it wraps.</param>
/// <param name="prefix">The prefix, such as <c>ADR-</c>; empty for none.</param>
/// <param name="encoding">The encoding.</param>
public sealed class FormattingDecorator(IIdGenerator inner, string prefix, IdEncoding encoding) : IIdGenerator
{
    /// <summary>Encodes a non-negative decimal integer as 13 base32 characters.</summary>
    /// <param name="decimalText">The integer as decimal digits.</param>
    /// <returns>The encoding.</returns>
    /// <exception cref="IdRefusedException">The candidate is not a decimal integer.</exception>
    public static string Base32(string decimalText)
    {
        if (!ulong.TryParse(decimalText, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
        {
            throw new IdRefusedException("base32 encoding needs a decimal candidate");
        }

        var characters = new char[13];
        for (var i = characters.Length - 1; i >= 0; i--)
        {
            characters[i] = NanoIdSettings.Base32[(int) (value & 31)];
            value >>= 5;
        }

        return new string(characters);
    }

    /// <summary>Decodes 13 base32 characters back to the integer.</summary>
    /// <param name="text">The encoding.</param>
    /// <returns>The integer.</returns>
    public static BigInteger Decode(string text) =>
        text.Aggregate(BigInteger.Zero, static (value, c) => (value * 32) + NanoIdSettings.Base32.IndexOf(c, StringComparison.Ordinal));

    /// <inheritdoc />
    public string Next()
    {
        var candidate = inner.Next();
        return prefix + (encoding == IdEncoding.Base32 ? Base32(candidate) : candidate);
    }
}
