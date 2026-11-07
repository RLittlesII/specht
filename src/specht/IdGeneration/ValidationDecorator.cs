using System.Text.RegularExpressions;

namespace Specht.IdGeneration;

/// <summary>Validation: refuses a candidate the type's grammar from the manifest's <c>identifiers</c> does not match.</summary>
/// <param name="inner">The strategy or decorator it wraps.</param>
/// <param name="type">The id type, for the reason.</param>
/// <param name="grammar">The type's grammar.</param>
public sealed class ValidationDecorator(IIdGenerator inner, string type, Regex grammar) : IIdGenerator
{
    /// <inheritdoc />
    public string Next()
    {
        var candidate = inner.Next();
        return grammar.IsMatch(candidate)
            ? candidate
            : throw new IdRefusedException($"the {type} grammar does not match the minted id");
    }
}
