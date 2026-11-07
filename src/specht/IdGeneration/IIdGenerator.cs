namespace Specht.IdGeneration;

/// <summary>The Strategy, and the shape every decorator around one keeps: one candidate id per call (item 0256 trial).</summary>
public interface IIdGenerator
{
    /// <summary>Produces the next candidate id.</summary>
    /// <returns>The candidate.</returns>
    string Next();
}
