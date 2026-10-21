namespace Specht.Manifest;

/// <summary>The upstream schema source a manifest records in <c>spechtSchema</c> (<c>0001-F7</c> B-068, decision 0008).</summary>
/// <param name="Location">The base address, as the manifest wrote it.</param>
/// <param name="Version">The version label, as the manifest wrote it.</param>
/// <param name="Files">One file for each fetched kind (<c>0001-F7</c> B-075, B-076).</param>
/// <implements epic="0001" feature="F7" claim="B-068" />
public sealed record UpstreamSchemaSource(string Location, string Version, IReadOnlyList<UpstreamSchemaFile> Files);
