namespace Specht.Manifest;

/// <summary>A refused upstream fetch, naming the source without exposing transport exception details.</summary>
/// <param name="message">The source and the reason its content cannot be written.</param>
public sealed class SpechtUpstreamSchemaException(string message) : Exception(message);
