namespace Specht.Identifiers;

/// <summary>The time a strategy reads, injected by the host so the engine references no clock and a test can fix the time.</summary>
/// <implements epic="0001" feature="F19" claim="B-007" />
public interface IClock
{
    /// <summary>Returns the current time, in universal time.</summary>
    /// <returns>The time.</returns>
    /// <implements epic="0001" feature="F19" claim="B-007" />
    DateTimeOffset GetUtcNow();
}
