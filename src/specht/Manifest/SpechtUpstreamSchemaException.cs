namespace Specht.Manifest;

/// <summary>A refused upstream fetch, naming the source without exposing transport exception details.</summary>
/// <param name="message">The source and the reason its content cannot be written.</param>
/// <implements epic="0001" feature="F5" claim="B-048" />
public sealed class SpechtUpstreamSchemaException(string message) : Exception(message);
