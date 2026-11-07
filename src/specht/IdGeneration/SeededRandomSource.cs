namespace Specht.IdGeneration;

/// <summary>Randomness from a seeded <see cref="Random"/>, for a reproducible simulation.</summary>
/// <param name="random">The seeded generator.</param>
public sealed class SeededRandomSource(Random random) : IRandomSource
{
    /// <inheritdoc />
    public int Next(int maxExclusive) => random.Next(maxExclusive);
}
