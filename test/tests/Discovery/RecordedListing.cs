namespace Specht.Tests.Discovery;

/// <summary>One listing a directory was asked for, and every path it returned.</summary>
/// <param name="Asked">The path the listing was asked for.</param>
/// <param name="Returned">The paths it returned.</param>
internal sealed record RecordedListing(string Asked, IReadOnlyList<string> Returned);
