using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Specht.Identifiers;

/// <summary>
/// The validation decorator (ADR-0033; <c>0001-F19</c> C-4): returns each candidate the strategy it wraps draws when the
/// type's <c>identifiers</c> grammar matches it, the pattern as written and in one letter case, and refuses one it does not
/// match, so nothing is printed and no other candidate is drawn. A type with no grammar has its candidate as drawn.
/// </summary>
/// <implements epic="0001" feature="F19" claim="B-011" />
/// <implements epic="0001" feature="F19" claim="B-012" />
public sealed class ValidationDecorator : IIdStrategy
{
    /// <summary>Initializes a new instance of the <see cref="ValidationDecorator"/> class, compiling a grammar that is present.</summary>
    /// <param name="strategy">The strategy each candidate is drawn from.</param>
    /// <param name="grammar">The type's member of <c>identifiers</c>, or nothing when the manifest holds none for it.</param>
    /// <param name="type">The id type, which names the key in a refusal.</param>
    /// <implements epic="0001" feature="F19" claim="B-011" />
    /// <implements epic="0001" feature="F19" claim="B-012" />
    public ValidationDecorator(IIdStrategy strategy, JsonNode? grammar, string type)
    {
        _grammar = grammar is null ? null : new Regex(grammar.GetValue<string>(), RegexOptions.CultureInvariant);
        _strategy = strategy;
        _type = type;
    }

    /// <inheritdoc />
    /// <exception cref="IdRefusedException">The type's grammar does not match the candidate drawn.</exception>
    /// <implements epic="0001" feature="F19" claim="B-011" />
    /// <implements epic="0001" feature="F19" claim="B-012" />
    public string Next()
    {
        var candidate = _strategy.Next();
        if (_grammar is null || _grammar.IsMatch(candidate))
        {
            return candidate;
        }

        throw new IdRefusedException(
            $"The '{_type}' candidate '{candidate}' does not match 'identifiers.{_type}', the grammar '{_grammar}'.");
    }

    private readonly IIdStrategy _strategy;
    private readonly Regex? _grammar;
    private readonly string _type;
}
