using System.Security.Cryptography;

namespace Specht.Identifiers;

/// <summary>Draws each integer from <see cref="RandomNumberGenerator.GetInt32(int)"/>, keeping no state and needing no network (C-7).</summary>
/// <implements epic="0001" feature="F19" claim="B-006" />
public sealed class RandomSource : IRandomSource
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F19" claim="B-006" />
    public int Next(int maxExclusive) => RandomNumberGenerator.GetInt32(maxExclusive);
}
