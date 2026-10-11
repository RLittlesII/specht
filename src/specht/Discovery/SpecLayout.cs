namespace Specht.Discovery;

/// <summary>A specification layout, as the manifest declares it (<c>0001-F6</c> B-001, decision 0004).</summary>
/// <param name="Name">The name the summary and the report give it (B-009).</param>
/// <param name="Glob">The glob a specification's root-relative path matches (C-6).</param>
/// <param name="Identity">The path segments that carry a specification's identity, or <see langword="null"/> when the layout declares none (B-004).</param>
public sealed record SpecLayout(string Name, string Glob, SpecPathIdentity? Identity = null);
