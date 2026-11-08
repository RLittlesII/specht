namespace Specht.IdGeneration;

/// <summary>A uniform random source the strategies draw from, injected so a test can fix the draws.</summary>
/// <implements epic="0001" feature="F19" claim="B-006" />
public interface IRandomSource
{
    /// <summary>Returns an integer drawn uniformly from <c>[0, maxExclusive)</c>.</summary>
    /// <param name="maxExclusive">The exclusive upper bound, at least 1.</param>
    /// <returns>The integer.</returns>
    /// <implements epic="0001" feature="F19" claim="B-006" />
    int Next(int maxExclusive);
}
