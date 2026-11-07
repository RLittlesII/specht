namespace Specht.IdGeneration;

/// <summary>Collision guard: draws again while the tree already holds the candidate, up to a number of attempts.</summary>
/// <param name="inner">The strategy or decorator it wraps.</param>
/// <param name="held">Whether the tree holds an id of the type, compared ignoring case so file names cannot fold together.</param>
/// <param name="attempts">How many candidates it tries.</param>
public sealed class CollisionGuardDecorator(IIdGenerator inner, Func<string, bool> held, int attempts) : IIdGenerator
{
    /// <summary>Candidates refused because the tree held them, since this decorator was made.</summary>
    public int Refused { get; private set; }

    /// <inheritdoc />
    public string Next()
    {
        for (var attempt = 0; attempt < attempts; attempt++)
        {
            var candidate = inner.Next();
            if (!held(candidate))
            {
                return candidate;
            }

            Refused++;
        }

        throw new IdRefusedException($"no free id in {attempts} attempts");
    }
}
