namespace Specht;

/// <summary>An input failure, returned as a value by every frame between the manifest and the command.</summary>
/// <param name="Kind">Which failure it is.</param>
/// <param name="Message">What failed, naming a path relative to the root and never an absolute one.</param>
public sealed record SpechtFailure(SpechtFailureKind Kind, string Message);
