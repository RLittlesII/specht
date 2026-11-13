using System.Text.Json.Nodes;

namespace Specht.Identifiers;

/// <summary>
/// The Nano ID strategy: a size of characters, each drawn uniformly from an alphabet, both read from a type's <c>nanoid</c>
/// sub-object alone (ADR-0033; <c>0001-F19</c> C-3).
/// </summary>
/// <implements epic="0001" feature="F19" claim="B-005" />
/// <implements epic="0001" feature="F19" claim="B-006" />
public sealed class NanoIdStrategy : IIdStrategy
{
    /// <summary>Initializes a new instance of the <see cref="NanoIdStrategy"/> class from the sub-object, filling an absent alphabet and size.</summary>
    /// <param name="settings">The type's <c>nanoid</c> sub-object, or nothing when the entry names none.</param>
    /// <param name="type">The id type, which names the default size.</param>
    /// <param name="random">The random source each character is drawn from.</param>
    /// <param name="key">The setting the entry is read from, which a refusal names: <c>mint.&lt;type&gt;</c> when nothing is named.</param>
    /// <exception cref="IdRefusedException">
    /// The sub-object is not an object, names an alphabet or a size no id can be drawn from, or names no size for a type with
    /// no default.
    /// </exception>
    /// <implements epic="0001" feature="F19" claim="B-019" />
    /// <implements epic="0001" feature="F19" claim="B-020" />
    /// <implements epic="0001" feature="F19" claim="B-021" />
    /// <implements epic="0001" feature="F19" claim="B-049" />
    /// <implements epic="0001" feature="F19" claim="B-050" />
    /// <implements epic="0001" feature="F19" claim="B-051" />
    public NanoIdStrategy(JsonNode? settings, string type, IRandomSource random, string? key = null)
    {
        key ??= $"mint.{type}";
        if (settings is not null and not JsonObject)
        {
            throw new IdRefusedException($"'{key}.nanoid' is not an object.");
        }

        _alphabet = settings?["alphabet"] switch
        {
            null => Base32,
            JsonValue value when value.TryGetValue<string>(out var text) && text.Length > 0 => text,
            _ => throw new IdRefusedException($"'{key}.nanoid.alphabet' is not a non-empty string."),
        };

        _size = settings?["size"] switch
        {
            null => DefaultSize(type) ?? throw new IdRefusedException($"'{key}.nanoid' names no size, and '{type}' has no default."),
            JsonValue value when value.TryGetValue<int>(out var number) && number > 0 => number,
            _ => throw new IdRefusedException($"'{key}.nanoid.size' is not a positive integer."),
        };

        _random = random;
    }

    /// <summary>The alphabet of a type that names none: lower-case base32, with no <c>i</c>, <c>l</c>, <c>o</c> or <c>u</c>.</summary>
    /// <implements epic="0001" feature="F19" claim="B-019" />
    public const string Base32 = "0123456789abcdefghjkmnpqrstvwxyz";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F19" claim="B-005" />
    /// <implements epic="0001" feature="F19" claim="B-006" />
    public string Next()
    {
        var characters = new char[_size];
        for (var i = 0; i < characters.Length; i++)
        {
            characters[i] = _alphabet[_random.Next(_alphabet.Length)];
        }

        return new string(characters);
    }

    /// <summary>
    /// The size of a type that names none (ADR-0033): 8 for a repository-wide type, 6 for a per-Feature type, and none for a type
    /// in neither scope.
    /// </summary>
    /// <implements epic="0001" feature="F19" claim="B-020" />
    /// <implements epic="0001" feature="F19" claim="B-021" />
    private static int? DefaultSize(string type) =>
        IdTypes.RepositoryWide.Contains(type) ? 8
        : IdTypes.PerFeature.Contains(type) ? 6
        : null;

    private readonly string _alphabet;
    private readonly int _size;
    private readonly IRandomSource _random;
}
