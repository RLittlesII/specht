namespace Specht.IdGeneration;

/// <summary>The Nano ID strategy: <see cref="NanoIdSettings.Size"/> characters, each drawn uniformly from the alphabet (ADR-0033).</summary>
/// <param name="settings">The alphabet and the size.</param>
/// <param name="random">The random source each character is drawn from.</param>
/// <implements epic="0001" feature="F19" claim="B-005" />
/// <implements epic="0001" feature="F19" claim="B-006" />
public sealed class NanoIdStrategy(NanoIdSettings settings, IRandomSource random) : IIdStrategy
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F19" claim="B-005" />
    /// <implements epic="0001" feature="F19" claim="B-006" />
    public string Next()
    {
        var characters = new char[settings.Size];
        for (var i = 0; i < characters.Length; i++)
        {
            characters[i] = settings.Alphabet[random.Next(settings.Alphabet.Length)];
        }

        return new string(characters);
    }
}
