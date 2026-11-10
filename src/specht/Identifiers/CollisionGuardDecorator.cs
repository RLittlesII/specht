using System.Text.Json.Nodes;

namespace Specht.Identifiers;

/// <summary>
/// The collision guard (ADR-0033; <c>0001-F19</c> C-4): draws again from the strategy it wraps while a name the tree holds
/// starts with the candidate in any letter case, up to the type's <c>attempts</c>, and refuses once every candidate is held.
/// </summary>
/// <implements epic="0001" feature="F19" claim="B-034" />
/// <implements epic="0001" feature="F19" claim="B-035" />
/// <implements epic="0001" feature="F19" claim="B-036" />
/// <implements epic="0001" feature="F19" claim="B-037" />
/// <implements epic="0001" feature="F19" claim="B-052" />
public sealed class CollisionGuardDecorator : IIdStrategy
{
    /// <summary>Initializes a new instance of the <see cref="CollisionGuardDecorator"/> class, filling an absent number of attempts.</summary>
    /// <param name="strategy">The strategy each candidate is drawn from.</param>
    /// <param name="attempts">The type's <c>attempts</c> member, or nothing when the entry names none.</param>
    /// <param name="type">The id type, which names the key in a refusal.</param>
    /// <param name="held">The names the tree holds for the type.</param>
    /// <exception cref="IdRefusedException">The entry names an <c>attempts</c> that is not a positive integer.</exception>
    /// <implements epic="0001" feature="F19" claim="B-035" />
    /// <implements epic="0001" feature="F19" claim="B-037" />
    /// <implements epic="0001" feature="F19" claim="B-052" />
    public CollisionGuardDecorator(IIdStrategy strategy, JsonNode? attempts, string type, IReadOnlyList<string> held)
    {
        _attempts = attempts switch
        {
            null => DefaultAttempts,
            JsonValue value when value.TryGetValue<int>(out var number) && number > 0 => number,
            _ => throw new IdRefusedException($"'mint.{type}.attempts' is not a positive integer."),
        };

        _strategy = strategy;
        _type = type;
        _held = held;
    }

    /// <summary>The number of candidates a type that names no <c>attempts</c> tries (ADR-0033).</summary>
    /// <implements epic="0001" feature="F19" claim="B-035" />
    /// <implements epic="0001" feature="F19" claim="B-036" />
    public const int DefaultAttempts = 8;

    /// <inheritdoc />
    /// <exception cref="IdRefusedException">Every candidate drawn is held.</exception>
    /// <implements epic="0001" feature="F19" claim="B-017" />
    /// <implements epic="0001" feature="F19" claim="B-034" />
    /// <implements epic="0001" feature="F19" claim="B-035" />
    /// <implements epic="0001" feature="F19" claim="B-036" />
    /// <implements epic="0001" feature="F19" claim="B-037" />
    public string Next()
    {
        for (var attempt = 0; attempt < _attempts; attempt++)
        {
            var candidate = _strategy.Next();
            if (!_held.Any(name => name.StartsWith(candidate, StringComparison.OrdinalIgnoreCase)))
            {
                return candidate;
            }
        }

        throw new IdRefusedException($"The tree holds each of the {_attempts} '{_type}' candidates drawn, the number 'mint.{_type}.attempts' bounds.");
    }

    private readonly IIdStrategy _strategy;
    private readonly int _attempts;
    private readonly string _type;
    private readonly IReadOnlyList<string> _held;
}
