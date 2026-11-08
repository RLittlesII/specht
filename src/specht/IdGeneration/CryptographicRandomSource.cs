using System.Security.Cryptography;

namespace Specht.IdGeneration;

/// <summary>Draws from <see cref="RandomNumberGenerator"/>, holding no state between draws and needing no network (<c>0001-F19</c> C-7).</summary>
/// <implements epic="0001" feature="F19" claim="B-006" />
public sealed class CryptographicRandomSource : IRandomSource
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F19" claim="B-006" />
    public int Next(int maxExclusive) => RandomNumberGenerator.GetInt32(maxExclusive);
}
