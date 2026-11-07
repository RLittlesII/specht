namespace Specht.IdGeneration;

/// <summary>Randomness from <see cref="System.Security.Cryptography.RandomNumberGenerator"/>.</summary>
public sealed class CryptographicRandomSource : IRandomSource
{
    /// <inheritdoc />
    public int Next(int maxExclusive) => System.Security.Cryptography.RandomNumberGenerator.GetInt32(maxExclusive);
}
