using System.Globalization;
using System.Text.Json.Nodes;

namespace Specht.Identifiers;

/// <summary>
/// The formatting decorator (ADR-0033; <c>0001-F19</c> C-4, C-12, C-15): starts each candidate the strategy it wraps draws
/// with the type's <c>prefix</c>, writes a Snowflake number in <c>base32</c> or leaves the draw as it is under <c>none</c>, and
/// refuses at construction a prefix or an encoding it cannot format, so nothing is drawn.
/// </summary>
/// <implements epic="0001" feature="F19" claim="B-009" />
/// <implements epic="0001" feature="F19" claim="B-030" />
/// <implements epic="0001" feature="F19" claim="B-031" />
/// <implements epic="0001" feature="F19" claim="B-032" />
/// <implements epic="0001" feature="F19" claim="B-033" />
/// <implements epic="0001" feature="F19" claim="B-053" />
/// <implements epic="0001" feature="F19" claim="B-054" />
/// <implements epic="0001" feature="F19" claim="B-055" />
/// <implements epic="0001" feature="F19" claim="B-056" />
public sealed class FormattingDecorator : IIdStrategy
{
    /// <summary>Initializes a new instance of the <see cref="FormattingDecorator"/> class, filling an absent prefix and encoding.</summary>
    /// <param name="strategy">The strategy each candidate is drawn from.</param>
    /// <param name="prefix">The type's <c>prefix</c> member, or nothing when the entry names none.</param>
    /// <param name="encoding">The type's <c>encoding</c> member, or nothing when the entry names none.</param>
    /// <param name="strategyName">The name of the strategy the entry names, which decides whether <c>base32</c> is offered and is the default.</param>
    /// <param name="type">The id type, which names the in-tree prefix and the key in a refusal.</param>
    /// <exception cref="IdRefusedException">
    /// The entry names a <c>prefix</c> that is not text, <c>base32</c> for a strategy other than <c>snowflake</c>, or an
    /// <c>encoding</c> formatting does not offer.
    /// </exception>
    /// <implements epic="0001" feature="F19" claim="B-030" />
    /// <implements epic="0001" feature="F19" claim="B-032" />
    /// <implements epic="0001" feature="F19" claim="B-033" />
    /// <implements epic="0001" feature="F19" claim="B-053" />
    /// <implements epic="0001" feature="F19" claim="B-054" />
    /// <implements epic="0001" feature="F19" claim="B-055" />
    /// <implements epic="0001" feature="F19" claim="B-056" />
    public FormattingDecorator(IIdStrategy strategy, JsonNode? prefix, JsonNode? encoding, string strategyName, string type)
    {
        _prefix = prefix is null
            ? InTreePrefixes.GetValueOrDefault(type, string.Empty)
            : TextOf(prefix) ?? throw new IdRefusedException($"'mint.{type}.prefix' is not text.");

        var snowflake = strategyName == Snowflake;
        var unset = snowflake ? Base32 : None;
        var named = encoding is null ? unset : TextOf(encoding);
        if (named == Base32 && !snowflake)
        {
            throw new IdRefusedException(
                $"'mint.{type}.encoding' is 'base32', which formatting offers Snowflake alone, and '{type}' names the strategy '{strategyName}'.");
        }

        if (named is not (None or Base32))
        {
            throw new IdRefusedException($"'mint.{type}.encoding' is neither 'none' nor 'base32'.");
        }

        _base32 = named == Base32;
        _strategy = strategy;
    }

    /// <inheritdoc />
    /// <implements epic="0001" feature="F19" claim="B-009" />
    /// <implements epic="0001" feature="F19" claim="B-030" />
    /// <implements epic="0001" feature="F19" claim="B-031" />
    /// <implements epic="0001" feature="F19" claim="B-032" />
    /// <implements epic="0001" feature="F19" claim="B-055" />
    /// <implements epic="0001" feature="F19" claim="B-056" />
    public string Next()
    {
        var draw = _strategy.Next();
        return _prefix + (_base32 ? Base32Of(draw) : draw);
    }

    /// <summary>A decimal number in <see cref="Base32Length"/> lower-case base32 characters, most significant first, padded with <c>0</c>.</summary>
    /// <param name="digits">The number in decimal digits.</param>
    /// <returns>The base32 text.</returns>
    /// <implements epic="0001" feature="F19" claim="B-030" />
    /// <implements epic="0001" feature="F19" claim="B-031" />
    private static string Base32Of(string digits)
    {
        var number = long.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture);
        var characters = new char[Base32Length];
        for (var i = characters.Length - 1; i >= 0; i--)
        {
            characters[i] = NanoIdStrategy.Base32[(int) (number & 31)];
            number >>= 5;
        }

        return new string(characters);
    }

    /// <summary>The text a member holds.</summary>
    /// <param name="member">The member's value.</param>
    /// <returns>The text, or <see langword="null"/> when the value is not a string.</returns>
    /// <implements epic="0001" feature="F19" claim="B-009" />
    /// <implements epic="0001" feature="F19" claim="B-053" />
    /// <implements epic="0001" feature="F19" claim="B-054" />
    private static string? TextOf(JsonNode member) =>
        member is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private const string None = "none";
    private const string Base32 = "base32";
    private const string Snowflake = "snowflake";
    private const int Base32Length = 13;

    /// <summary>The prefix each type the tree already numbers holds, ADR-0033's table, which a type naming none gets (B-055).</summary>
    /// <implements epic="0001" feature="F19" claim="B-055" />
    private static readonly Dictionary<string, string> InTreePrefixes = new(StringComparer.Ordinal)
    {
        ["claim"] = "B-",
        ["constraint"] = "C-",
        ["openQuestion"] = "OQ-",
        ["scenario"] = "S-",
    };

    private readonly IIdStrategy _strategy;
    private readonly string _prefix;
    private readonly bool _base32;
}
