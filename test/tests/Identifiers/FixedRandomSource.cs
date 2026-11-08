using Specht.Identifiers;

namespace Specht.Tests.Identifiers;

/// <summary>
/// A random source that answers with the draws it was given, in order and then again from the first, and records the
/// bound of every request. A draw that is not below its request's bound is refused, as a uniform source would never give
/// it, so a test cannot pass on a draw the real source cannot make.
/// </summary>
internal sealed class FixedRandomSource(params int[] draws) : IRandomSource
{
    /// <summary>Gets the bound of every request, in the order the requests came.</summary>
    public List<int> Bounds { get; } = [];

    /// <inheritdoc />
    public int Next(int maxExclusive)
    {
        var draw = draws[Bounds.Count % draws.Length];
        Bounds.Add(maxExclusive);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(draw, maxExclusive);
        return draw;
    }
}
