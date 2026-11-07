namespace Specht.IdGeneration;

/// <summary>A uniform random source, so the strategies take cryptographic randomness and the simulation a seeded one.</summary>
public interface IRandomSource
{
    /// <summary>Returns a uniform integer in <c>[0, maxExclusive)</c>.</summary>
    /// <param name="maxExclusive">The exclusive upper bound.</param>
    /// <returns>The integer.</returns>
    int Next(int maxExclusive);
}
