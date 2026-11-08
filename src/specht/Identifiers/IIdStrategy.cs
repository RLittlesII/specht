namespace Specht.Identifiers;

/// <summary>A strategy that draws one id (<c>0001-F19</c> C-3, C-4); a decorator is another strategy that wraps one.</summary>
/// <implements epic="0001" feature="F19" claim="B-008" />
public interface IIdStrategy
{
    /// <summary>Draws one id.</summary>
    /// <returns>The id.</returns>
    /// <implements epic="0001" feature="F19" claim="B-008" />
    string Next();
}
