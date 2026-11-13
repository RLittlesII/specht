using System.Text.RegularExpressions;

namespace Specht.Identifiers;

/// <summary>
/// The validation decorator (ADR-0033; <c>0001-F19</c> C-4): returns each candidate the strategy it wraps draws when the
/// type's grammar matches it, the pattern as written and in one letter case, and it does not start with the other type's
/// longer head, and refuses one that fails either test, so nothing is printed and no other candidate is drawn. A type with no
/// grammar has its candidate as drawn.
/// </summary>
/// <implements epic="0001" feature="F19" claim="B-011" />
/// <implements epic="0001" feature="F19" claim="B-012" />
/// <implements epic="0001" feature="F19" claim="B-068" />
/// <implements epic="0001" feature="F19" claim="B-076" />
/// <implements epic="0001" feature="F19" claim="B-077" />
public sealed class ValidationDecorator : IIdStrategy
{
    /// <summary>Initializes a new instance of the <see cref="ValidationDecorator"/> class, compiling a grammar that is present.</summary>
    /// <param name="strategy">The strategy each candidate is drawn from.</param>
    /// <param name="grammar">The type's grammar as a pattern, or nothing when the manifest holds none for it.</param>
    /// <param name="key">The setting the grammar is read from, which a refusal names.</param>
    /// <param name="type">The id type, which a refusal names.</param>
    /// <param name="reserved">The other type's longer head, which no candidate may start with, or nothing.</param>
    /// <param name="comparison">How a candidate is compared with <paramref name="reserved"/>, the claim tag case setting.</param>
    /// <implements epic="0001" feature="F19" claim="B-011" />
    /// <implements epic="0001" feature="F19" claim="B-012" />
    /// <implements epic="0001" feature="F19" claim="B-068" />
    /// <implements epic="0001" feature="F19" claim="B-076" />
    /// <implements epic="0001" feature="F19" claim="B-077" />
    public ValidationDecorator(IIdStrategy strategy, string? grammar, string key, string type, string? reserved, StringComparison comparison)
    {
        _grammar = grammar is null ? null : new Regex(grammar, RegexOptions.CultureInvariant);
        _strategy = strategy;
        _key = key;
        _type = type;
        _reserved = reserved;
        _comparison = comparison;
    }

    /// <inheritdoc />
    /// <exception cref="IdRefusedException">The type's grammar does not match the candidate drawn, or it starts with the reserved head.</exception>
    /// <implements epic="0001" feature="F19" claim="B-011" />
    /// <implements epic="0001" feature="F19" claim="B-012" />
    /// <implements epic="0001" feature="F19" claim="B-068" />
    /// <implements epic="0001" feature="F19" claim="B-076" />
    /// <implements epic="0001" feature="F19" claim="B-077" />
    public string Next()
    {
        var candidate = _strategy.Next();
        if (_grammar is not null && !_grammar.IsMatch(candidate))
        {
            throw new IdRefusedException(
                $"The '{_type}' candidate '{candidate}' does not match '{_key}', the grammar '{_grammar}'.");
        }

        if (_reserved is not null && candidate.StartsWith(_reserved, _comparison))
        {
            throw new IdRefusedException(
                $"The '{_type}' candidate '{candidate}' starts with '{_reserved}', the head the check reads as the other type's.");
        }

        return candidate;
    }

    private readonly IIdStrategy _strategy;
    private readonly Regex? _grammar;
    private readonly string _key;
    private readonly string _type;
    private readonly string? _reserved;
    private readonly StringComparison _comparison;
}
