namespace Specht.IdGeneration;

/// <summary>The Nano ID strategy: <see cref="NanoIdSettings.Size"/> characters drawn uniformly from the alphabet.</summary>
/// <param name="settings">The alphabet and the size.</param>
/// <param name="random">The random source.</param>
public sealed class NanoIdGenerator(NanoIdSettings settings, IRandomSource random) : IIdGenerator
{
    /// <inheritdoc />
    public string Next()
    {
        settings.Validate();
        var characters = new char[settings.Size];
        for (var i = 0; i < characters.Length; i++)
        {
            characters[i] = settings.Alphabet[random.Next(settings.Alphabet.Length)];
        }

        return new string(characters);
    }
}
