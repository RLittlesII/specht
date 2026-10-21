namespace Specht.Manifest;

/// <summary>The test files and their declaration forms a manifest declares in <c>tests</c> at schema 0.2.0 (B-055, B-056).</summary>
/// <param name="Files">The globs of the files holding tests, as the manifest wrote them.</param>
/// <param name="Declarations">The declaration forms, each holding <c>{test}</c> once; empty when the manifest leaves them out.</param>
/// <implements epic="0001" feature="F5" claim="B-048" />
public sealed record SpecTestSettings(IReadOnlyList<string> Files, IReadOnlyList<string> Declarations);
